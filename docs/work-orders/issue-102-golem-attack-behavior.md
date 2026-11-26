# 작업지시서: #102 Golem 연속 공격 행동 및 애니메이션 전환 개선

- Issue: https://github.com/swchoi-dev/CogsAndGears/issues/102
- 브랜치: `feature/102-golem-attack-behavior` (원격 `develop` `ea28ec0` 기준)
- 관련: #80 Golem 점프 공격 개선 및 착지 피해

## 목적과 범위
- 목적: Jump Attack 종료 후 공격 대기 중 Idle(옆을 보는 자세)이 재생되지 않고, 거리에 따라 Jump Attack 재수행 또는 추적으로 바로 이어지게 한다.
- 포함: `GolemAnimator.controller` 전환 구조, `ChaseAction` 도착 판정
- 제외: 피격·데미지·사망·스테이지 클리어, `MonsterController` 공격 타이밍, Drone 등 다른 몬스터

## 원인 분석
- `GolemGraph`는 이미 매 Tick 거리를 재판단한다: Repeat → Selector [Distance ≤ AttackRange → Jump Attack] / [Distance > AttackRange → Chase] / [Wait 0.1초].
  - 사거리 안이지만 쿨다운(AttackInterval 3초) 중이면 Jump Attack이 실패해 Wait 분기로 내려가고, `IsMoving=false`가 된다.
- `GolemAnimator`
  - `JumpAttack → Idle`이 조건 없이 Exit Time 0.95로 전환되어, 쿨다운 동안 Idle 클립이 재생됐다.
  - Idle 클립의 자세 방향이 공격·이동 클립과 달라 플레이어 대신 옆(등)을 보는 것처럼 보였다. 방향 회전(faceY 225/135)은 정상이었다.
  - `JumpAttack → Run` 전환이 없어 추적 시에도 Idle을 경유했다. 추적 후 정지(`Run → Idle`)도 쿨다운 중 Idle을 재생했다.
- `ChaseAction`은 수평 거리(|dx| ≤ AttackRange)로 도착을 판단하지만, 그래프 공격 조건은 Transform 간 3D 거리다. 사거리 경계에서 높이 차가 있으면 추적 성공·공격 조건 실패가 반복되어 멈출 수 있다(코드상 불일치, 아래 검증 참고).

## 변경 내용
| 경로 | 구분 | 내용 |
|---|---|---|
| `Assets/CogsAndGears/Art/Animations/Monster/GolemAnimator.controller` | 수정 | `CombatIdle` 상태 추가: JumpAttack 클립, speed 0, cycleOffset 0.95(공격 종료 전환 시점 자세 고정). `JumpAttack → Run`(IsMoving, Exit 0.95), `JumpAttack → CombatIdle`(Exit 0.95), `CombatIdle → Run`(IsMoving), `Run → CombatIdle`(!IsMoving, 기존 → Idle 대체). 파라미터·Any State → JumpAttack 유지, Idle은 스폰 직후 기본 상태로만 사용 |
| `Assets/CogsAndGears/Scripts/Behaviour/ChaseAction.cs` | 수정 | 도착 판정을 그래프 조건과 같은 3D 거리로 변경. 수평 도착(0.1 이내)이지만 높이 차로 사거리 밖이면 제자리 대기(좌우 왕복 방지) |

- Behavior Graph·`MonsterController`·`JumpAttackAction`은 변경하지 않았다. 공격 시작·종료 판정(`IsPlayingJumpAttack`)은 상태 이름 `JumpAttack` 기준이라 `CombatIdle`에서는 공격 종료로 판단된다.
- Animation Event(`OnJumpTakeoff`, `OnJumpLanded`)는 `IsJumpAttacking`·`_hasTakenOff`로 보호되어 `CombatIdle`에서 중복 실행되지 않는다(speed 0이라 이벤트 구간을 지나지 않음).

## 검증 (에이전트 Play Mode, GameMain)
- 기준선(변경 전): JumpAttack(약 2.5초) 후 쿨다운 동안 Idle 약 1.5~2초, 캡처에서 Idle 자세가 옆·뒤를 향함
- 변경 후
  - [x] 사거리 안: JumpAttack → CombatIdle → JumpAttack 반복, 스폰 직후 이후 Idle 미발생. 연속 공격 시 JumpAttack 처음부터 재생(nt≈0.1부터)
  - [x] 사거리 밖으로 이동(FortGolem을 dx 10으로 이동): Run → 범위 진입 시 JumpAttack
  - [x] MechGolem(사거리 7, Stage 스폰 지점에서 스폰): Run → dx 6.9에서 JumpAttack → CombatIdle 반복
  - [x] 방향: 좌측 플레이어 faceY 225, 우측 플레이어 faceY 135 유지
  - [x] CombatIdle 캡처: 플레이어를 향한 전투 자세, 발 접지
  - [x] Console 오류·경고 0, 컴파일 성공
- 테스트 편의를 위해 런타임에서만 플레이어 `Health._isDead`를 설정해 피해를 무시시켰다(저장 없음).

## 미검증·주의
- `ChaseAction` 경계 교착은 코드상 불일치로 수정했으며 실제 재현은 하지 못했다(관찰된 정지 사례는 테스트 스폰 위치 오류로 낙하한 경우였다).
- 플레이어가 높은 발판에 있어 수평 도착 후에도 사거리 밖이면 Golem은 제자리 대기한다(기존에는 Idle 정지).
- `Old/MechanicalGolem.prefab`의 `MechanicalGolem` 그래프도 `ChaseAction`을 사용한다. Animator는 공유하지 않는다.
- 공격 판정 중복은 정적 근거(기존 `_hasTakenOff`·착지 판정 로직 미변경)로 판단했다. 개발자 육안 검수(전환 자연스러움)는 남아 있다.
