# 작업지시서: #106 Cogs Rifle 모델 교체 및 양팔 Animation Rigging 개선

- Issue: https://github.com/swchoi-dev/CogsAndGears/issues/106
- 브랜치: `feature/106-rifle-animation-rigging` (원격 `develop` `10ae936` 기준)
- 관련: #104 Cogs 플레이어 애니메이션 및 리깅 개선

## 목적과 범위
- 목적: 무기 모델을 `Assets/Imports/Rifle/Rifle.fbx`로 교체하고, 상체와 양팔 Animation Rigging으로 총구가 조준 방향을 향하는 양손 파지 자세를 만든다.
- 제외: Walk 재추가, 무기 데이터·공격 성능·투사체 설정, 신규 무기 시스템

## Rifle 모델 분석
- 단일 메시(정점 53만), FBX 루트 회전 (270, 0, 0)·스케일 100, 크기 1.90 × 0.64 × 0.23. `Material.001.mat`(URP Lit, BaseMap·Normal·Metallic) 연결 상태
- FBX 루트 기준 +X = 총구 방향, +Y = 위. 측면 정사영 렌더와 마커로 파지점 확인
  - 오른손 파지(방아쇠 뒤 개머리 손목): (-0.42, -0.03)
  - 왼손 파지(총열 하단 핸드가드 앞): (0.08, 0.0)
  - 총구: (0.956, 0.13)

## Rifle 프리팹 구조 (`Cogs.prefab`)
```text
RightHand
└── WeaponSocket            lp (0.002, 0.073, 0.008), le (340.1, 346.2, 74.0)  — 기존 값 유지(손바닥 중심, 손 → 총 회전)
    └── Rifle               scale 0.45, 원점 = RightHandGrip, +X = 총열, +Y = 총 위
        ├── WeaponModel     Rifle.fbx 인스턴스, lp (0.420, 0.030, 0) (FBX 기본 회전·스케일)
        ├── RightHandGrip   lp (0, 0, 0)
        ├── LeftHandGrip    lp (0.500, 0.030, 0), le (22.2, 229.4, 69.6) — Pistol Idle의 왼손 방향
        └── MuzzlePoint     lp (1.376, 0.160, 0), 회전 0 (right = 총열)
```
- 기존 `WeaponVisual`(Hand Cannon)과 이전 `MuzzlePoint` 제거, `PlayerWeapon._muzzlePoint`를 새 `MuzzlePoint`로 연결
- 크기: 캐릭터 팔 길이(상완 0.325 + 전완 0.363)로 왼손이 핸드가드에 닿는 범위에서 0.45(월드 길이 약 1.28)로 정했다.

## Animation Rigging 구성과 평가 순서
```text
마우스 조준(PlayerAim, 최대 ±45°)
 → AimRig: UpperBodyConstraint(Multi-Rotation, 척추 ±25°) → RightArmIK(Two Bone IK)
 → 오른손에 장착된 Rifle → LeftHandGrip
 → LeftArmRig: LeftArmIK(Two Bone IK, Target = LeftHandGrip)
```
- RigBuilder 레이어: `AimRig` → `LeftArmRig`
- 오른팔 IK Target은 조준 방향과 가슴 위치로만 계산한다. 무기 Transform을 매 프레임 참조하지 않고, 손 → 소켓 오프셋은 시작 시 한 번 읽는다(순환 의존 없음).
  - 총 자세: +X = 조준 방향, +Y = 조준에 수직인 XY 방향(좌우 반전 시 뒤집히지 않게 FacingDirection 반영)
  - 오른손 파지점 = 양 어깨 중심 + 조준 방향 × `_armReach`, 오른손 Target = 총 자세 × 손→소켓 오프셋의 역. 팔 길이의 98%를 넘지 않게 제한
  - 팔꿈치 Hint: 팔 중간에서 총 아래쪽과 각 팔의 바깥쪽으로 `_elbowHintDistance`
- 상체와 팔에 같은 회전을 중복 적용하지 않는다(상체는 척추만, 오른손 Target은 월드 절대 위치).
- Dash 중에는 양팔 IK weight를 0으로 내리고 종료 시 복원한다.

## 스크립트 변경
- `PlayerAim.cs`
  - Inspector: `Max Aim Angle`(45), `Arm Rig` 항목(`_rightShoulder`, `_leftShoulder`, `_rightHand`, `_weaponSocket`, `_leftHandGrip`, `_rightHandTarget`, `_rightElbowHint`, `_leftElbowHint`, `Arm Reach` 0.32, `Elbow Hint Distance` 0.3, `_armIKConstraints`)
  - `UpdateArms`: 오른팔 IK Target·양 팔꿈치 Hint 계산
  - `ClampAimDirection`: 상하 조준 각도를 `_maxAimAngle`로 제한(자세·발사 공용)
  - 모델 좌우 회전을 임계값이 적용된 `FacingDirection`으로 결정(수직 근처 반복 전환 방지)
- `PlayerWeapon.cs`: 총구 → 포인터 방향에 같은 각도 제한 적용, 포인터가 총구보다 수평으로 뒤에 있으면 바라보는 쪽으로 발사
- Rig Weight는 각 Constraint·Rig 컴포넌트의 weight, 손목 회전은 `WeaponSocket`·`LeftHandGrip` Local Rotation으로 Inspector에서 조정한다(중복 필드 없음).

## 검증 (에이전트 Play Mode, GameMain)
조준 입력 주입, 몬스터 제거, 이동 상태는 Animator `Speed`를 직접 설정해 확인했다.

| 상태 | 조준 0° / ±45° / 135°(좌) | 89° 요청 |
|---|---|---|
| Pistol Idle | 총구 = 조준(오차 0.0°), z 0.00, 오른손 Target 오차 0, 왼손-Grip 0.000 | 45°로 제한 |
| Pistol Run | 동일 | 45°로 제한 |
| Pistol Run Backward | 동일(왼손-Grip 0.000) | 45°로 제한 |

- 투사체 방향 = 총구 방향(0°→(1.00,-0.04,0), 45°→43~45°, 89° 요청→45°), z 0.00
- 팔꿈치 각도: 오른팔 62~74°(초기 0.2 설정의 53°보다 펴짐), 왼팔 97~166°
- Dash: 팔 IK weight 0 → 종료 후 1 복원
- 측면·정면 캡처로 수평·상하 45°·좌측 조준 자세 확인
- Console 오류 0(경고는 기존 `MonsterController` CS0414), Missing Reference·Missing Script 0

## 미검증·주의
- 실제 키보드 입력 주입이 이번 세션에서 Input Action으로 전달되지 않아 이동·Dash는 Animator `Speed` 설정과 이벤트 직접 호출로 확인했다. 이동 코드는 변경하지 않았다.
- 실제 마우스 클릭 사격은 `OnFire` 직접 호출로 대체했다.
- 손가락 본이 없어 손가락이 총을 감싸지 않는다.
- `Assets/Imports/`는 `.gitignore` 대상이라 `Rifle.fbx`·텍스처·`.meta`는 커밋되지 않는다. 다른 환경에도 같은 GUID(`33c4c14cd432ab642997e68df8c1a11d`)의 Imports 폴더가 필요하다.
- 최대 조준 각도(45°), Arm Reach, 총 크기의 최종 값은 개발자 육안 검수가 필요하다.
