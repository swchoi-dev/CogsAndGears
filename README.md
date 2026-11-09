# Cogs & Gears

<p align="center">
 <img width="1672" height="941" alt="톱니와 기어_ 스팀펑크 모험 포스터" src="https://github.com/user-attachments/assets/3709ce99-c21f-4f26-9cea-994675dc45ea" />
</p>

<p align="center"><strong>A Run & Gun Backpack Adventure</strong></p>

> **개인 개발 프로젝트 · 기획 및 프로토타입 단계**  
> 동화풍 스팀펑크 세계에서 발명가 **Cogs**와 기계 고양이 **Gears**가 함께하는 **2.5D 횡스크롤 런앤건 × 백팩 빌딩** 게임입니다.

## 게임 소개

마우스로 360도 자유롭게 조준하며 횡스크롤 스테이지를 돌파합니다. 스테이지를 클리어하면 세 가지 보상 중 하나를 골라 **마법 스크롤 형태의 격자 인벤토리**에 배치합니다. 무기와 유물, 아이템의 모양·배치·인접 시너지가 다음 전투를 바꿉니다.

- **Run → Stage:** 한 판(Run)은 여러 스테이지로 구성됩니다. 각 스테이지의 오른쪽 끝 Goal에 도착하면 클리어합니다.
- **보상과 빌드:** Stage Clear → 보상 3개 중 1개 선택 → 스크롤 배치·재배치·합성 → Next Stage. 전투 도중 인벤토리 편집은 할 수 없습니다.
- **독립 공격:** 스크롤에 장착된 공격 아이템은 마우스 좌클릭 입력을 공유하되, 각자의 쿨타임으로 발동합니다.
- **2.5D 비주얼:** 핸드페인팅 배경과 3D 캐릭터를 결합하며, Physics2D로 게임플레이 충돌을 처리합니다.

## 조작

| 입력 | 동작 |
|---|---|
| `A` / `D` | 좌 / 우 이동 |
| `Space` | 회피(Dodge) |
| 마우스 이동 | 360° 자유 조준 |
| 마우스 왼쪽 버튼 | 장착된 공격 아이템 사용 |

**점프는 없습니다.** 회피 중 이동 방향, 무적 구간, 재사용 대기시간은 플레이테스트 과정에서 수치를 조정합니다.

## 게임 진행

```text
Title Scene
   └─ New Game
       └─ GameMain Scene / Run
           ├─ Stage 1 → Goal
           ├─ 보상 3개 중 선택 → Inventory 편집
           ├─ Stage 2 → Goal
           ├─ 보상 → Inventory 편집
           └─ ... 최종 Stage
               └─ End Scene / Clear

Cogs HP가 0이 되면 언제든 End Scene / Defeat
```

## 기술 스택

| 항목 | 선택 |
|---|---|
| Engine | Unity 6 LTS |
| Render | Universal Render Pipeline (URP), Universal Renderer |
| Gameplay | 2.5D, XY 평면, Rigidbody2D / Collider2D |
| View | Orthographic 횡스크롤 카메라 |
| Character | Meshy.ai → Blender → FBX → Unity Prefab |
| Animation | Animator 상·하체 레이어 분리 + Animation Rigging IK |
| Data | ScriptableObject 중심 콘텐츠 설정 |
| Target | PC 프로토타입 우선 |
| Development | **개발자가 직접 C# 코딩 및 Unity Editor 작업** |

## 콘셉트 아트

> 아래 이미지는 **기획 및 비주얼 방향을 확인하기 위한 콘셉트**입니다. 게임 내 구현 화면이나 완성된 3D 리소스가 아닙니다.

| 캐릭터 디자인 |
|---|
| Cogs |
| <img width="1254" height="1254" alt="image" src="https://github.com/user-attachments/assets/066f0ffa-52e9-4840-834c-9776412f9778" />|
| Gears |
|<img width="1122" height="1402" alt="ChatGPT 이미지 2026년 11월 6일 오후 01_37_18-1" src="https://github.com/user-attachments/assets/aeb5de89-c8d4-4cb9-a960-8690df323bb4" />|



## 프로젝트 현황

- [ ] **CG-01** Unity 프로젝트 및 기본 환경 구축
- [ ] **CG-04** Cogs 이동 / 회피 / 조준 / 애니메이션
- [ ] **CG-05 / CG-06** 전투 및 Monster 구현
- [ ] **CG-07 / CG-03** Stage 및 Run 진행 연결
- [ ] **CG-08 / CG-09** 스크롤 Inventory 및 Stage 보상
- [ ] **CG-10** Gears 동료 연출
- [ ] **CG-11 / CG-12** 비주얼·UI·사운드·테스트

> 이 체크리스트는 계획이며, 완료 상태를 의미하지 않습니다.

## 문서

- [프로젝트 스펙 / 기능 명세 (PROJECT_SPEC.md)](docs/PROJECT_SPEC.md)

## 디렉터리 구조 (예정)

```text
CogsAndGears/
├── Assets/                    # Unity 프로젝트 에셋 (프로젝트 생성 후)
├── Packages/
├── ProjectSettings/
├── README.md
└── docs/
    ├── PROJECT_SPEC.md
    └── images/
        ├── title-concept.jpg
        ├── character-concept.jpg
        ├── world-concept.jpg
        └── cogs-model-reference.jpg
```

## 개발 원칙

이 프로젝트는 **AI 코딩 에이전트 없이 직접 구현**합니다. AI는 기획·기술 리서치·설계 검토·문서 정리에만 보조적으로 활용하며, 설계는 단순성과 책임 분리를 우선합니다.

---

*Project specification: 2026-11-06 · Concept art is subject to change.*
