# 작업지시서: #102 Golem 연속 공격 행동 및 애니메이션 전환 개선

- Issue: https://github.com/swchoi-dev/CogsAndGears/issues/102
- 브랜치: `feature/102-golem-attack-behavior` (원격 `develop` `ea28ec0` 기준)
- 관련: #80 Golem 점프 공격 개선 및 착지 피해

## 목적과 범위
- 목적: Golem이 Jump Attack 종료 직후 거리를 다시 판단해 사거리 안이면 즉시 다음 Jump Attack, 밖이면 즉시 Chase를 실행한다. 공격 사이 Idle·대기 자세·대기 시간을 두지 않는다.
- 포함: `GolemAnimator.controller`, `MonsterAnimHandler`의 Jump Attack 종료·이동 판정, `ChaseAction` 도착 판정, Golem 스탯의 AttackInterval
- 제외: 공격 판정·데미지·Animation Event·사망·스테이지 클리어, Drone 등 다른 몬스터

## 변경 이력
- 1차(`1a6408a`): 공격 대기 중 Idle 대신 `CombatIdle`(JumpAttack 0.95 자세 고정) 유지 → 2차 요청으로 폐기
- 2차(현재): 공격 대기 자체를 제거하고 Jump Attack을 연속 실행

## 원인 분석
- `GolemGraph`는 이미 매 Tick 거리를 재판단한다: Repeat → Selector [Distance ≤ AttackRange → Jump Attack] / [Distance > AttackRange → Chase] / [Wait 0.1초]. Graph 구조 변경은 필요하지 않다.
- 공격 사이 대기는 `MonsterController`의 쿨다운(`_nextAttackTime = 착지 시각 + AttackInterval 3초`) 때문에 Jump Attack이 실패하고 Wait 분기로 내려가며 생겼다. 이때 Animator는 `JumpAttack → Idle`(조건 없음)로 Idle 클립을 재생했다.
- `ChaseAction`은 수평 거리, 그래프 공격 조건은 3D 거리로 도착을 판단해 사거리 경계에서 불일치가 있었다.

## 변경 내용
| 경로 | 구분 | 내용 |
|---|---|---|
| `Assets/CogsAndGears/Art/Animations/Monster/GolemAnimator.controller` | 수정 | 상태는 JumpAttack·Run·Idle(기존과 동일, `CombatIdle` 없음). `JumpAttack → Idle` 제거, `JumpAttack → Run`(IsMoving, Exit Time 없음) 추가, `Any State → JumpAttack` Can Transition To Self 활성화. Idle은 스폰 직후 기본 상태와 `Run → Idle`(기존)에서만 사용 |
| `Assets/CogsAndGears/Scripts/Monster/MonsterAnimHandler.cs` | 수정 | `IsPlayingJumpAttack`: JumpAttack 클립을 끝까지 재생(normalizedTime ≥ 1, 전환 중 아님)하면 종료로 판단. `IsMoving`: JumpAttack 재생 중에는 도약 속도를 이동으로 보지 않음 |
| `Assets/CogsAndGears/Scripts/Behaviour/ChaseAction.cs` | 수정 | 도착 판정을 그래프 조건과 같은 3D 거리로 변경, 수평 도착 후 높이 차로 사거리 밖이면 제자리 대기 |
| `Assets/CogsAndGears/SO/Stat/GolemStat.asset`, `MecahniGolem.asset` | 수정 | `_attackInterval` 3 → 0 (Golem 전용 스탯. Drone은 `DroneStat` 사용) |

- 동작 흐름: 공격 종료(nt ≥ 1) → `IsJumpAttacking=false` → `JumpAttackAction` Success → 그래프가 거리 재판단 → 사거리 안이면 `JumpAttack` 트리거로 JumpAttack 재진입(처음부터 재생), 밖이면 Chase 속도 → `IsMoving=true` → Run.
- `MonsterAnimHandler`는 Drone도 사용하지만 Drone Animator에는 JumpAttack 상태가 없어 `IsPlayingJumpAttack`이 항상 false이므로 영향이 없다.
- Animation Event(`OnJumpTakeoff`, `OnJumpLanded`)·착지 판정·데미지 로직은 변경하지 않았다.

## 검증 (에이전트 Play Mode, GameMain)
`EditorApplication.update` 로거로 상태 변화를 프레임 단위로 기록했다.

- [x] 사거리 밖: Chase(Run) 실행
- [x] 사거리 진입: Run → JumpAttack
- [x] 사거리 안 연속 공격: JumpAttack 종료(nt 1.01) 0.01~0.02초 후 JumpAttack 재진입, 재진입 후 nt 0.10부터 재생
- [x] 공격 도중 사거리 이탈(nt 0.69에서 Golem을 dx 10으로 이동): 현재 공격 완료(nt 1.01) 후 0.02초 내 Run
- [x] 공격 사이 Idle·대기 자세·대기 시간 없음
- [x] 연속 공격 시 공격 판정 1회: 단일 Golem 기준 플레이어 피격 간격 약 2.57초(공격 길이 3.8초 / speed 1.5 = 2.53초)로 공격당 1회
- [x] FortGolem·MechGolem(Stage 스폰 지점에서 스폰) 모두 동일 동작
- [x] 방향: 좌측 플레이어 faceY 225, 우측 135(1차 검증) 유지
- [x] Console 오류·경고 0, 컴파일 성공
- 테스트 편의를 위해 런타임에서만 플레이어 `Health._isDead`·HP를 조작했다(저장 없음).

## 미검증·주의
- 공격 간격이 3초 쿨다운에서 공격 길이(약 2.5초)로 줄어 Golem의 공격 빈도가 높아졌다. 밸런스는 개발자 확인이 필요하다.
- `ChaseAction` 경계 교착은 코드상 불일치로 수정했으며 실제 재현은 하지 못했다.
- `Old/MechanicalGolem.prefab`도 `MecahniGolem` 스탯과 `ChaseAction`을 사용한다.
- 작업 중 `Jump Attack.fbx.meta`의 `OnJumpTakeoff` 이벤트 시간이 0.158 → 0.0735로 변경되어 있었다. 에이전트 스크립트가 변경한 것이 아니어서 커밋에 포함하지 않았다. 위 검증은 변경된 값이 적용된 상태에서 수행했다.
- 개발자 육안 검수(연속 공격 전환의 자연스러움)는 남아 있다.
