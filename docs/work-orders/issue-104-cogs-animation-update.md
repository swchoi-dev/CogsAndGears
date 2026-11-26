# 작업지시서: #104 Cogs 플레이어 애니메이션 및 리깅 개선

- Issue: https://github.com/swchoi-dev/CogsAndGears/issues/104
- 브랜치: `feature/104-cogs-animation-update` (원격 `develop` `0b3d0b2` 기준)

## 목적과 범위
- 목적: 기본 이동 애니메이션을 Pistol 계열로 교체하고, 팔·손 자세는 애니메이션이, Animation Rigging은 상체 조준만 담당하도록 단순화한다. 총기는 손의 자세를 따라가며, 캐릭터는 기본적으로 완전한 측면을 본다.
- 제외: 발사 방향 계산(`PlayerWeapon`), 이동·Dash·무기 교체·인벤토리·피격 로직

## 작업 방식 결정: 기존 구조 정리 (원본 모델 재구성 대신)
- 현재 `CogsModel`은 원본 `Assets/Imports/Cogs/Cogs_EquipmentSeparated.fbx`의 Prefab 인스턴스이며, 척추·팔 본에 회전·위치 Override가 없다.
- 추가된 요소는 `WeaponSocket`, `AimRig`(`UpperBodyConstraint`만 남음), `Targets`(`UpperBodyAimTarget`만 남음), `RigBuilder`뿐이다.
- 같은 Pistol Idle 자세에서 Rig 평가 전후 척추·팔 본 12개의 회전 변화가 없었다(수평 조준).
- 따라서 원본 모델에서 재구성해도 동일한 구조가 되며, 반복된 정렬 문제의 원인은 IK 잔존이 아니라 총기 장착 오프셋이었다.

## 원인 분석
1. 팔·손목 꺾임: 기존 `RightArmIK`(Two Bone IK, weight 1)가 조준 방향 + 고정 손 회전 310°로 계산한 목표에 손을 강제로 맞췄다. 제거 후 팔·손 본을 덮어쓰는 Constraint·스크립트는 없다(Animator IK Pass 비활성).
2. 총구 약 45° 틀어짐
   - 총기 모델 `Alchemical_Hand_Cannon`의 기본 회전 (1.4, 328.8, 93.1)은 IK의 고정 손 회전과 짝을 이뤄 조정된 값이라 IK 제거 후 불필요한 중복 오프셋이 되었다.
   - 1차 정렬에서 총열을 손 본 Y축에 맞췄으나, Pistol Idle(양손 파지)에서 오른손 축은 몸 중앙 쪽으로 약 21°, 위로 약 15° 기울어 있다(총열과 25.3°).
   - 기존 3/4뷰 기본 회전 45°가 더해져 화면상 약 45° 틀어져 보였다.

## 변경 내용
| 경로 | 구분 | 내용 |
|---|---|---|
| `Art/Animations/Cogs/Pistol Idle.fbx`, `Pistol Run.fbx`, `Pistol Run Backward.fbx` (+ `.meta`) | 추가 | Humanoid(Create From This Model) 임포트, 클립 이름 지정, Loop Time, Root Rotation·Height Bake(Original) |
| `Art/Animations/Cogs/CogsAnimator.controller` | 수정 | Blend Tree(Speed) Motion 교체: th -2 RunBackward → Pistol Run Backward, th 0 Idle2 → Pistol Idle, th 2 RunForward → Pistol Run. Walk·Dash·파라미터·Apply Root Motion(false) 유지 |
| `Prefabs/Characters/Cogs.prefab` | 수정 | `AimRig/RightArmIK`, `Targets/RightHandIKTarget`, `Targets/RightElbowHint` 제거. 무기 Transform 정렬. `PlayerAim._defaultModelRotationAngle` = 0. GUID 유지 |
| `Scripts/Player/PlayerAim.cs` | 수정 | 오른팔 IK 제어(`_armTarget`, `_handTarget`, `_armReach`, `_handIKConstraint`, Dash 시 IK weight 전환) 제거. 상수 45° 대신 직렬화 필드 `_defaultModelRotationAngle`(기본 0) 사용 |

### 무기 Transform (전 → 후)
| Transform | 변경 전 | 변경 후 |
|---|---|---|
| `RightHand/WeaponSocket` | lp (0, 0, 0), le (0, 0, 0) | lp (0.0070, 0.1214, -0.0015), le (340.1, 346.2, 74.0) — Pistol Idle 수평 조준 자세에서 총열 = 캐릭터 정면, 총 위 = 월드 위, 손잡이 = 손 메시 중심 |
| `WeaponSocket/WeaponVisual` | lp 0, le 0 | lp 0, le 0 (유지) |
| `WeaponVisual/Alchemical_Hand_Cannon` | lp (0.0520, 0.2750, 0.0420), le (1.4, 328.8, 93.1) | lp (0.2762, 0.0291, 0.0002), le (270, 0, 0) — `WeaponVisual` 기준 +X = 총열(`MuzzlePoint.right`), +Y = 총 위, 원점 = 손잡이 중심 |
| `MuzzlePoint` | lp (0.0106, 0, 0.0028), le 0 | 유지 |

- 총기 모델 축을 정규화해 중복 회전을 없애고, 손 대비 장착 오프셋은 `WeaponSocket` 하나로 관리한다.
- 손 본에 손가락 본이 없어 손가락이 손잡이를 감싸지는 않는다. 손잡이는 양손 사이(손 메시 중심)에 위치한다.

### 기본 회전 Offset
- 위치: `PlayerAim.cs`의 상수 `DEFAULT_MODEL_ROTATION_ANGLE = 45f`가 좌우 방향 회전(45+90, 45+180)과 `Init`에 적용되고 있었다.
- 변경: Inspector 필드 `PlayerAim > Default Model Rotation Angle`(`_defaultModelRotationAngle`, 기본 0). 오른쪽 = 90° + Offset, 왼쪽 = -(90° + Offset). Offset 45이면 기존 135°/225°와 같다. `Init`은 오른쪽 방향 회전으로 시작한다.

## 검증 (에이전트, Play Mode·프리뷰 렌더)
단계별로 확인했다.
1. 원본 모델: 원본 FBX 인스턴스, 본 Override 없음
2. Pistol 애니메이션: Rig 평가 전후 팔·손 본 변화 없음, 정면·측면·후면·상단 렌더로 자세 확인
3. 총기 장착: Pistol Idle·Run에서 총열이 캐릭터 정면, Pistol Run Backward는 손 자세 차이로 약 10~15° 옆으로 틀어짐(손을 따름)
4. 상체 Rig: `UpperBodyConstraint`만 동작
5. 마우스 조준(입력 주입): 수평 조준 시 총구-조준 오차 -0.4°(오른쪽), 1.3°(왼쪽), z 성분 0.00~0.01. 총구 방향 오른쪽 (1.00, -0.03, 0.00), 왼쪽 (-1.00, -0.04, 0.00)
6. 기능: `PlayerWeapon.OnFire` 직접 호출 시 투사체 방향 0°→(1.00, 0.00, 0.01), 180°→(-1.00, -0.04, 0.00). D/A 입력 시 Pistol Run, 정지 시 Pistol Idle, Space 시 Dash 후 복귀. Console 오류·경고 0, Missing Reference 0

## 미검증·주의
- 상하 조준: 상체 회전 범위 `_bodyRotateRange`는 기존 값 25°(develop과 동일)이고, Pistol 자세의 척추가 측면 대비 약 40° 비틀려 있어 척추 X축 회전이 수직 피치로 약 77%만 반영된다. 조준 20°에서 총구 13°, 60°에서 약 18°로 상하 조준 시 총구와 조준 방향의 차이가 커진다. 발사 방향 로직은 요청대로 변경하지 않았다.
- 실제 마우스 클릭 입력 주입으로는 발사가 트리거되지 않아 `OnFire`를 직접 호출해 확인했다.
- 측면 자세의 시각적 판단, 기본 회전 각도 결정, 손 파지 육안 검수는 개발자 확인이 필요하다.

## 추가 작업 1: Walk 애니메이션 및 관련 로직 제거
- 출처: `c8a9fc3 feat: idle, walk, run, dash 애니메이션 연결`에서 Idle(0)→Walk(1)→Run(2)를 지나가도록 추가된 속도 램프
- Animator: Blend Tree에서 WalkBackward(-1)·WalkForward(1) 제거, 임계값을 -1 Pistol Run Backward / 0 Pistol Idle / 1 Pistol Run으로 변경. Walk 전용 State·Transition·Parameter는 없었다(파라미터 `Speed` 유지, Dash 유지). `WalkForward.anim`·`WalkBackward.anim` 에셋은 삭제하지 않았다.
- `PlayerMovement.cs`: `_runThreshold`, `_acceleration`, `_deceleration`, `_currentSpeed`와 램프 계산 제거. 바라보는 방향 기준 이동 방향(-1/0/1)을 그대로 `Speed`로 전달. 실제 이동 속도(`_moveSpeed`, `FixedUpdate`)는 변경 없음
- `PlayerAnimHandler.cs`: 램프 제거로 Blend 전환이 튀지 않도록 `SetFloat` 감쇠(0.1초) 적용
- 프리팹 재저장으로 제거된 필드의 직렬화 데이터 정리(씬 Override 없음)
- 검증(Play Mode): 정지 → Pistol Idle, 바라보는 방향 이동 → Pistol Run(Speed 1), 반대 이동 → Pistol Run Backward(Speed -1), 좌우 모두 확인. Dash 후 복귀, Console 오류 0

## 추가 작업 2: Pistol Run Backward 중 투사체 방향 오류 수정
- 원인: 총이 손 자세를 따르므로 Pistol Run Backward에서 총구가 화면 안팎으로 기울어(`muzzle.right.z` 약 ±0.16~0.30) 투사체가 XY 플레이 평면을 벗어났다. `PlayerWeapon`은 총구 3D 방향 그대로 발사했다.
- 수정(`PlayerWeapon.cs`): 총구 방향을 XY 평면에 투영하고 생성 위치 z를 플레이어 평면에 맞춘다(`DroneController.Fire`와 같은 방식). XY 각도는 기존대로 총구 방향을 따른다.
- 검증(`OnFire` 직접 호출): 모든 상태에서 투사체 방향 z 0.00, 생성 위치 z 오차 0.000. Run Backward 오른쪽 (0.99, -0.17, 0.00), 왼쪽 (-0.99, -0.15, 0.00)
- 남은 차이: Run Backward 손 자세로 총이 수평 조준 대비 약 8~10° 아래를 향해 투사체도 그 각도로 나간다.
