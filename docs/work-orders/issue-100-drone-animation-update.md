# 작업지시서: #100 Drone 애니메이션 업데이트

- Issue: https://github.com/swchoi-dev/CogsAndGears/issues/100
- 브랜치: `feature/100-drone-animation-update` (원격 `develop` 기준)
- 관련: #83 Drone 원거리 AI 및 애니메이션 구현

## 목적과 범위
- 목적: `Drone.prefab`의 시각 모델을 `SteampunkDrone_Animated_fixed.fbx`로 교체하고 Idle / Move / Attack 애니메이션을 기존 행동 로직과 연동한다.
- 포함: 모델 교체, Material, Animator Motion 교체, MuzzlePoint·Animation Event 연결
- 제외: MonsterController·DroneController·Behavior Graph·다른 몬스터 공통 로직 변경, 신규 상태·파라미터 추가

## 조사 결과
- 기존 연동 구조는 그대로 사용할 수 있다. `MonsterAnimHandler`가 Rigidbody 속도로 `IsMoving`(bool)을, `DroneController.TryStartAttack`이 `Attack`(trigger)을 설정한다. 발사는 Attack 클립의 `OnFire` Animation Event → `DroneAnimationEvents` → `DroneController.Fire()`이며 `_hasFired`로 공격당 1발을 보장한다. C# 변경은 필요하지 않았다.
- 신규 FBX
  - Rig는 이미 Generic(`animationType: 2`)이다.
  - 통합 Take `IDLE` / `MOVE`는 정지 자세만 담고 있고 실제 움직임은 오브젝트별 Take(`BodyMotion|IDLE`, `CannonBarrel|Attack`, `WingPivot_L|MOVE` 등)로 분리되어 있다. 통합 `Attack` Take는 없다. Animator 상태당 하나의 클립을 쓰기 위해 병합 클립을 생성했다.
  - 모델 크기는 기존 `drone.fbx`의 정확히 1/10이며 노드 구조·경로(`BodyMotion/CannonPivot/CannonBarrel/MuzzlePoint` 등)는 동일하다.
  - 내장 `Material.001`에 텍스처가 연결되지 않는다(흰색). 기존 `drone.fbx` 머티리얼은 BaseMap·NormalMap을 사용했다.
- 기존 클립의 루트 커브가 런타임에 모델 루트 회전을 (270, 0, 0)으로 덮어쓰고 있었다(프리팹 값 (270, 270, 0)은 무시됨). 기준선 Play Mode에서 확인했다.

## 변경 내용
| 경로 | 구분 | 내용 |
|---|---|---|
| `Assets/CogsAndGears/Art/Animations/Monster/Drone/Drone_Idle.anim` | 생성 | `*|IDLE` Take 병합, Loop |
| `Assets/CogsAndGears/Art/Animations/Monster/Drone/Drone_Move.anim` | 생성 | `*|MOVE` Take 병합, Loop |
| `Assets/CogsAndGears/Art/Animations/Monster/Drone/Drone_Attack.anim` | 생성 | `*|Attack` Take 병합, Loop 해제, `OnFire` 이벤트 2/24초(기존과 동일, 총신 반동 최대 시점) |
| `Assets/CogsAndGears/Art/Animations/Monster/DroneAnimator.controller` | 수정 | Idle·Move·Attack 상태의 Motion만 교체. 파라미터·전환 유지 |
| `Assets/CogsAndGears/Art/Materials/Drone.mat` | 생성 | 기존 drone 머티리얼(URP Lit, BaseMap·NormalMap) 복사 |
| `Assets/CogsAndGears/Prefabs/Monsters/Drone.prefab` | 수정 | ModelRoot 아래 모델 교체, Animator·DroneAnimationEvents 추가, `_muzzlePoint`·`MonsterAnimHandler._animator` 재연결, Renderer 8개에 `Drone.mat` 적용. GUID `3df81f354dc355445b9c377d0e4b88b7` 유지 |

- 병합 클립은 루트("") 커브를 제외해 모델 루트 Transform을 프리팹 값이 결정한다. 모델 루트는 회전 (-90, 0, 0), 스케일 1000(FBX 기본 100 × 10)으로 기존 런타임 자세·크기와 맞췄다.
- Apply Root Motion 비활성화, Culling·Update Mode는 기존 Animator 값을 유지했다.
- 작업은 Unity CLI(Pipeline) `script eval`로 Editor API를 사용해 수행했다. YAML 직접 수정은 하지 않았다.

## 검증 (에이전트 Play Mode, GameMain)
테스트용 Drone을 `StageController.SpawnMonster`와 같은 방식(`Initialize(_playerTarget)`)으로 스폰해 확인했다.

- [x] 대기 시 Idle, 공격 시 Attack 재생 후 Idle 복귀
- [x] 사거리 밖에서 Move 재생(`IsMoving=true`, 속도 (3.0, -0.1)) 후 사격 위치 도착 시 Attack
- [x] 좌우 전환: 플레이어 왼쪽일 때 faceY=180·총구 x -1.26·탄 속도 (-9.4, -3.3), 오른쪽일 때 faceY=0·총구 x +1.26·탄 속도 (9.7, -2.6)
- [x] 총구 위치가 기준선(기존 모델 -1.25, 0.21)과 일치
- [x] 피격(HP 50→49)·사망(`IsDead`, 오브젝트 제거) 정상
- [x] Console 오류·경고 0, 컴파일 실패 없음, 프리팹 Missing Reference·Missing Script 0
- [x] Game View 캡처로 텍스처·크기 확인

## 미검증·주의
- 공격당 1발은 정적 근거(클립 이벤트 1개 + `_hasFired` 가드)와 관찰된 탄 수로 판단했다. 프레임 단위 계수는 하지 않았다.
- 테스트 Drone은 Stage 목록 밖에서 스폰해 스테이지 클리어 집계는 Play Mode로 확인하지 않았다. `StageController` 로직은 변경하지 않았다.
- `Assets/Imports/`는 `.gitignore` 대상이다. FBX·텍스처와 그 `.meta`는 커밋되지 않으므로 다른 환경에도 같은 GUID의 Imports 폴더가 필요하다.
- 개발자 육안 검수(애니메이션 품질·Attack 펌프 연출)는 남아 있다. FBX에 SteamPump 애니메이션 Take는 없다.
