# Cogs & Gears — Claude Code → Codex 인수인계

- 작성일: 2026-11-10
- 작성: Claude Code (Opus 5.5)
- 기준 커밋: `dbd4cb3` ([docs] AI Harness 초기 설정 적용), 브랜치 `main`
- 목적: 프로젝트 관리 에이전트를 Claude Code에서 OpenAI Codex로 전환하기 위한 현황과 초기 작업 정리

> 이 문서의 "확인됨"은 도구로 직접 확인한 사실, "미검증"은 확인하지 못한 내용이다. 개발자에게 확인하기 전에는 미검증 항목을 사실로 기록하지 않는다.

---

## 1. 프로젝트 개요

### 게임
- 동화풍 스팀펑크 세계의 **2.5D 횡스크롤 런앤건 × 백팩 빌딩** 게임
- 주인공: 발명가 **Cogs**, 동료: 기계 고양이 **Gears**
- 핵심 게임플레이
  - `A`/`D` 좌우 이동, `Space` 회피(Dodge), 마우스 360° 자유 조준, 좌클릭 공격. **점프 없음**
  - Run(한 판)은 여러 Stage로 구성. Stage 오른쪽 끝 Goal 도착 시 클리어
  - Stage Clear → 보상 3개 중 1개 선택 → 연금술 가방(마법 스크롤) 격자 인벤토리에 배치·재배치·합성 → Next Stage
  - 인벤토리에 장착된 공격 아이템은 좌클릭 입력을 공유하고 각자 쿨타임으로 발동
  - 아이템의 모양·배치·인접 시너지가 전투에 영향. 전투 중 인벤토리 편집 불가
  - Cogs HP 0 → End Scene / Defeat
- 게임 소개·조작·진행 흐름의 원문은 Git 루트 `README.md`

### 저장소와 경로
| 항목 | 값 |
|---|---|
| GitHub | `swchoi-dev/CogsAndGears` |
| Git 루트 | `CogsAndGears/` (로컬: `D:\swchoi\UnityProject\CogsAndGears`) |
| Unity 프로젝트 루트 | `CogsAndGears/CogsandGears/` (대소문자 주의: `CogsandGears`) |
| 콘텐츠 루트 | `CogsandGears/Assets/CogsAndGears/` |
| Scene | `Scenes/Title.unity`, `Scenes/GameMain.unity` |

### 개발 환경 (확인됨)
- Unity `6000.3.9f1` (Unity 6), URP `17.3.0`, Input System `1.18.0`
- OS: Windows 11, IDE: JetBrains Rider (`.idea/` 존재)
- 캐릭터 제작 파이프라인: Meshy.ai → Blender → FBX → Unity Prefab (README 기준)

### 개발 원칙
- **Unity C# 코드 작성과 Scene·Prefab·Asset·Editor 작업은 개발자가 직접 수행한다.**
- 에이전트는 코드·에셋을 수정하지 않는다. 필요하면 읽기 전용으로 분석하고 구현 가이드(범위, 변경 예상 파일, 설계 방향, 완료 조건, 기술 학습 키워드)를 제시한다.
- Unity MCP 또는 `unity` CLI로 Editor를 조작하지 않는다.

---

## 2. 확정된 기술 아키텍처

| 영역 | 결정 |
|---|---|
| 렌더링 | Unity 6 URP (Universal Renderer) |
| 시점 | 2.5D 횡스크롤, 게임플레이는 XY 평면 |
| 비주얼 | 3D 캐릭터 + 2D 배경 Sprite (Parallax Scrolling 예정) |
| 물리 | Physics2D: `Rigidbody2D` / `Collider2D` |
| 카메라 | Orthographic 횡스크롤 카메라 |
| 구조 | MonoBehaviour 컴포넌트 중심, 책임 단위로 컴포넌트 분리 |
| 상태 처리 | **별도 FSM·State Pattern 미사용.** `enum` + `if-else`/`switch`로 처리 |
| 통신 | C# `event`로 컴포넌트 간 결합 완화 |
| 데이터 | ScriptableObject 중심 콘텐츠 설정 |
| 애니메이션 | Animator 상·하체 레이어 분리 + Animation Rigging IK (README 계획) |
| 대상 | PC 프로토타입 우선. 배포 대상 WebGL, Windows |

주요 시스템: Cogs 이동·조준·사격·회피, Gears 동료 추종, 연금술 가방 격자 인벤토리, Stage/Run 진행 및 보상 선택.

---

## 3. 현재 개발 현황

### 구현 현황
| 항목 | 상태 | 근거 |
|---|---|---|
| 배경 `BG_Far` Sprite 배치 | 완료 (개발자 보고) | `GameMain.unity`에 `Background/BG_Far`, SpriteRenderer 1개 확인. `BG_Mid`, `BG_Near` 오브젝트는 있으나 SpriteRenderer 없음 |
| Cogs 모델 및 Material 연결 | 완료 (개발자 보고) | `Cogs/VisualRoot` 구조 확인. Material 연결은 Scene 파일로 미검증 |
| Rigidbody2D / CapsuleCollider2D 바닥 안착 | 완료 (개발자 보고) | Scene에 `Rigidbody2D` 1, `CapsuleCollider2D` 1, `GroundCollision`의 `BoxCollider2D` 1 확인. Play Mode 동작은 에이전트 미검증 |
| Orthographic 카메라 | 설정됨 | `Main Camera`의 `orthographic: 1` 확인 |
| A/D 좌우 이동 | **구현 예정** | `Scripts/Player/` 비어 있음 |
| 카메라 추적 | **구현 예정** | 관련 스크립트 없음 |

- 현재 C# 스크립트: `Scripts/Core/SceneReference.cs`, `SceneReferenceSample.cs`, `Scripts/Editor/SceneReferenceDrawer.cs`만 있다.
- `Scripts/Combat/`, `Scripts/Inventory/`, `Scripts/Player/` 폴더는 비어 있다.

### Git 상태 (확인됨)
- 로컬·원격 브랜치: `main`만 존재. GitHub 기본 브랜치 `main`
- 작업 트리: 깨끗함 (이 문서 추가 전 기준)
- 최근 커밋
  ```text
  dbd4cb3 [docs] AI Harness 초기 설정 적용
  e805c78 [feat] 코그스, Texture 추가
  98c97c2 [feat] 코그스, 기어스 에셋 추가
  6f39e7b Update README.md
  969b845 Update README.md
  ```
- 참고: `dbd4cb3`에는 `.claude/`, `CLAUDE.md`와 함께 개발자의 `GameMain.unity` 변경(159+/14-)이 포함되어 있다.

### 문서·Issue 현황 (확인됨)
- GitHub Issues: 0개 / Pull Requests: 0개 / 라벨: GitHub 기본 라벨만 있음
- `docs/`: 이 인수인계 문서 외 없음
- `README.md`가 링크한 `docs/PROJECT_SPEC.md`는 **존재하지 않는다.**
- README의 "프로젝트 현황"에 `CG-01`~`CG-12` 로드맵 그룹이 계획으로만 있다. GitHub Issue 번호와는 무관하다.
  ```text
  CG-01        Unity 프로젝트 및 기본 환경 구축
  CG-04        Cogs 이동 / 회피 / 조준 / 애니메이션
  CG-05/06     전투 및 Monster 구현
  CG-07/03     Stage 및 Run 진행 연결
  CG-08/09     스크롤 Inventory 및 Stage 보상
  CG-10        Gears 동료 연출
  CG-11/12     비주얼·UI·사운드·테스트
  ```

---

## 4. Git 및 배포 정책

### 브랜치
| 브랜치 | 역할 |
|---|---|
| `prod` | 운영·릴리스 기준. `develop`에서 PR로만 반영. 직접 개발 커밋 금지 |
| `develop` | 기능 통합·검증 |
| `feature/<Issue 번호>-<기능명>` | Issue 단위 기능 개발. **`develop`에서 분기** (예: `feature/12-cogs-movement`) |

- 흐름: `feature/*` → PR → `develop` → PR → `prod`
- 병합 전략: Create a merge commit (Squash·Rebase merge 미사용)
- PR 제목: 기능 `Merge: #<Issue 번호> 작업명`, 릴리스 `Release: 릴리스 내용`
- 커밋: `[feat]`, `[fix]`, `[docs]`, `[chore]` + `변경 내용 요약`. Issue 연결은 `Refs #번호` / `Closes #번호`
- 커밋 단위: 논리적으로 완결된 변경 단위 (상세: `.claude/rules/git-workflow.md` 4.1)

### 배포
| 대상 | 기준 | 방식 |
|---|---|---|
| WebGL | `prod` | 향후 GitHub Actions로 빌드·배포 자동화 예정 (현재 미구현) |
| Windows | `prod` | 개발자가 수동 빌드. 자동화 도입 안 함 |

- `develop`, `feature/*`에서는 운영용 WebGL을 배포하지 않는다.

### 기존 `main` 브랜치
- `develop`, `prod`가 아직 없고 `main`이 유일한 브랜치이자 기본 브랜치다.
- **`main`의 처리(유지·이름 변경·보관)는 Codex가 현황을 분석한 뒤 정리 계획을 세워 개발자 승인을 받는다.** 승인 전에는 변경·삭제하지 않는다.
- 검토 포인트
  - GitHub 기본 브랜치: `develop`을 권장한다. PR 기본 대상이 되고, `Closes #번호`는 기본 브랜치에 병합될 때만 Issue를 자동으로 닫는다.
  - `prod` 초기 기준점: 현재 `main` HEAD에서 분기할지 결정한다.
  - 브랜치 보호 규칙(Rulesets): `prod` 직접 Push 금지 등.

---

## 5. 기존 AI Harness 적용 현황

### 원본
- https://github.com/swchoi-dev/ai-harness (기준 커밋 `38c8c8d`). 원본 저장소는 수정하지 않았다.
- 원본의 `presets/`, `scripts/`는 비어 있다. 설치는 `shared/templates/project-claude.md` 주석의 수동 복사 방식으로 진행했다.
- 원본 저장소에는 `agents/codex/rules`, `agents/codex/skills`가 빈 폴더로 준비되어 있다.
- 파일별 원본 매핑, 수정 내용, 제외 사유, 동기화 절차: **`.claude/HARNESS_SOURCE.md`**

### 적용 파일 (Git 루트)
| 파일 | 내용 | 구분 |
|---|---|---|
| `CLAUDE.md` | 프로젝트 지침, 설계 원칙, 규칙 우선순위, 세션 작업 디렉터리 확인 | Claude 전용 형식, 내용 대부분 공통 |
| `.claude/rules/behavior-guidelines.md` | 사고 우선, 단순함, 외과적 변경, 검증, 브리핑·승인 | **공통** |
| `.claude/rules/cg-role-boundary.md` | 에이전트 허용·금지 업무, 쓰기 가능 위치 | **공통** (Git 조항 개정 필요, 아래 참고) |
| `.claude/rules/git-workflow.md` | `feature → develop → prod`, 커밋·PR·배포 정책 | **공통** (Git 수행 주체 개정 필요) |
| `.claude/rules/github-issue-workflow.md` | Issue 계층(상위 `type:epic`/하위 Sub-issues), 중복 검색, 단계별 승인, 완료 처리 | **공통** |
| `.claude/rules/github-issue-template.md` | Issue 본문(배경/목적, 범위, 완료 조건, 기술 학습 키워드, 참고 문서), 검증 기록 댓글 | **공통** |
| `.claude/rules/unity-guidelines.md` | `.meta`/GUID 보존, Scene·에셋 보호, 검증 | **공통** |
| `.claude/rules/coding-conventions.md` | Unity C# 컨벤션 (분석·구현 가이드 기준) | **공통** |
| `.claude/skills/start-issue/` | Issue 조회·검색·생성 → 읽기 전용 조사 → 구현 가이드·브랜치명 브리핑 | 절차는 공통, 형식은 Claude 전용 |
| `.claude/skills/record-verification/` | 개발자 검증 결과를 Issue 댓글로 기록, 완료 조건 체크, close 판단 | 절차는 공통, 형식은 Claude 전용 |
| `.claude/skills/write-tech-doc/` | `docs/tech/NN-*.md` 기술문서, `docs/learning/keywords.md` 관리 | 절차는 공통, 형식은 Claude 전용 |
| `.claude/settings.json` | 권한 차단 38개·승인 요청 82개, Unity 플러그인 비활성화 | **Claude 전용** |
| `.claude/HARNESS_SOURCE.md` | 원본 추적, 권한 검증 기록 | 이관 시 출처 기록으로 유지 |

- Jira·Confluence·Atlassian MCP 관련 규칙과 스킬은 모두 제외했다.
- 규칙 파일들은 파일명으로 서로 참조한다. 이관할 때는 같은 디렉터리에 두거나 참조 경로를 함께 갱신해야 한다.

### 검증된 사항 (2026-11-10, Claude Code 2.1.302, 복제 저장소 headless 테스트)
- `Edit`/`Write` 도구로 `CogsandGears/**`, `*.cs`, `*.unity`, `*.meta` 등을 쓰는 시도는 차단된다.
- Bash·PowerShell의 `>`, `cp`, `sed -i`, `Set-Content`로 보호 경로를 쓰는 시도도 차단된다.
- Bash·PowerShell의 `git`/`gh` 쓰기 명령에는 승인 요청이 뜬다. 넓은 허용 규칙보다 승인 요청 규칙이 우선한다.
- `git reset --hard`, `git clean`, `unity` CLI는 차단된다.
- 프로젝트 설정의 `enabledPlugins`로 Unity 플러그인 스킬이 숨겨진다.
- Git 루트에서 새 세션을 열면 `CLAUDE.md`, 규칙 7개, 스킬 3개가 로드된다.

### 미검증 · 한계
- **Git 루트 실행 필수:** Unity 하위 폴더(`CogsandGears/`)에서 실행하면 규칙과 스킬은 로드되지만 `settings.json`은 적용되지 않는다. 이 경우 `.cs` 수정과 `unity` 실행이 모두 허용된다. 지금은 `CLAUDE.md`의 경고 지침으로만 보완되어 있다.
- **인터프리터 우회:** `node -e`, PowerShell `[IO.File]::WriteAllText` 등을 통한 쓰기는 권한 규칙으로 막을 수 없다. 사람의 승인이나 auto 모드 분류기에만 의존한다.
- **스킬 실행:** 스킬 3개를 실제 Issue로 실행해 보지 않았다. 형식과 정적 검토만 했다.
- **Unity MCP:** 서버가 미설정이라 `mcp__unity*` 차단은 이름을 추정한 예비 규칙이다.
- **사용자 전역 설정**(`~/.claude/settings.json`): Clawd on Desk의 `PermissionRequest` HTTP hook이 있고, `autoMode.environment`가 다른 프로젝트(Fourbeat, Jira) 기준이다. 프로젝트 범위 밖이라 수정하지 않았다.

### 공통 규칙과 Claude 전용 규칙
| 공통 (Codex로 이관해 재사용) | Claude 전용 (Codex에서 대응 방식 재설계) |
|---|---|
| 행동·승인 원칙, 역할 제한 내용 | `.claude/settings.json`의 `permissions` 문법 (→ Codex sandbox·승인 정책으로 대응) |
| Git·배포 정책 | `.claude/rules/` 자동 로드 방식 (→ `AGENTS.md`와 참조 구조) |
| GitHub Issue 워크플로·템플릿 | `SKILL.md` frontmatter(`disable-model-invocation`, `argument-hint`) |
| Unity 가이드·C# 컨벤션 | `enabledPlugins`(Unity 플러그인 비활성화) |
| 스킬 3개의 절차 본문 | "Git 루트에서 실행해야 설정이 적용됨" 제약 |

---

## 6. Codex의 담당 업무

- Git 브랜치·Commit·PR 관리
- GitHub Issues 및 개발 로드맵 관리
- 기획·설계·기술문서 관리 (`docs/`)
- 개발자가 구현한 기능의 검증 결과 기록
- AI Harness 공통 Rules·Skills·Templates 작성, 추후 원본 저장소(`swchoi-dev/ai-harness`) 반영

### 유지되는 제한
- Unity C# 코드 작성·수정 금지. Scene·Prefab·Asset·`.meta`·ProjectSettings·Packages 수정 금지
- Unity MCP·`unity` CLI로 Editor 조작 금지
- Git 쓰기(브랜치, Commit, Push, PR, Merge), GitHub Issue 쓰기, 원본 Harness 저장소 반영은 **개발자 승인 후** 수행
- 개발자의 미커밋 변경 보존. 강제 Push, `reset --hard`, `clean`, 승인 없는 merge/rebase 금지

### 이관 시 주의
- 현재 `.claude/rules/git-workflow.md`와 `cg-role-boundary.md`에는 "브랜치 생성, Commit, Push, PR, Merge는 개발자가 직접 수행한다"고 되어 있다. Codex가 Git 관리를 맡으므로 이관할 때 이 조항을 **"승인 후 Codex가 수행"**으로 개정해야 한다.
- PR **Merge**를 Codex가 할지, 개발자가 할지는 아직 정하지 않았다. 개정 전에 확인한다.
- Claude 설정의 쓰기 금지 범위는 Git 루트 기준 `CLAUDE.md`, `.claude/`, `docs/`, `README.md`만 허용한다. Codex 설정을 만들 때는 `AGENTS.md`와 Codex 설정 경로를 허용 범위에 추가해야 한다.

---

## 7. Codex 초기 작업

권장 순서다. 각 단계는 계획을 브리핑하고 개발자 승인을 받은 뒤 진행한다.

1. **기존 Claude 설정 분석 및 이관 설계**
   - 이 문서, `.claude/HARNESS_SOURCE.md`, `CLAUDE.md`, `.claude/rules/*`, `.claude/skills/*`를 읽는다.
   - 공통 규칙의 이관 위치와 6장 "이관 시 주의"의 규칙 개정안을 정한다.
   - 기존 `.claude/`와 `CLAUDE.md`를 유지할지, 보관할지, 제거할지는 개발자가 결정한다. 승인 전에는 삭제하지 않는다.
2. **`AGENTS.md` 및 Codex Skills 구성**
   - Git 루트에 `AGENTS.md`를 두고, 역할 제한·설계 원칙·규칙 참조를 옮긴다.
   - 스킬 3개(start-issue, record-verification, write-tech-doc)를 Codex Skills 형식으로 옮긴다.
   - Claude의 `settings.json`이 하던 보호를 Codex의 sandbox·승인 정책으로 어디까지 재현할 수 있는지 검증하고 기록한다.
3. **원본 AI Harness와 공통화 방향 정리**
   - 공통 규칙(Agent 중립)과 Agent별 어댑터(Claude/Codex)를 나누는 구조를 제안한다.
   - 원본 저장소 반영은 원본 저장소의 Issue 워크플로를 따르고, 별도 승인을 받는다.
4. **develop/prod 브랜치 초기화 계획**
   - 4장 "기존 `main` 브랜치" 검토 포인트를 기준으로 계획을 세운다.
   - 브랜치 생성 순서, 기본 브랜치 변경, 보호 규칙, `main` 처리 방안을 정리한다.
5. **`docs/PROJECT_SPEC.md` 및 개발 로드맵 정리**
   - README와 이 문서 1~3장을 기준으로 스펙을 작성하고, README의 깨진 링크를 해결한다.
   - 로드맵은 README의 `CG-xx` 그룹을 기준으로 정리한다. `CG-xx` 표기를 계속 쓸지 개발자에게 확인한다.
6. **GitHub Issues 초기 등록 계획**
   - 상위 Issue는 `CG-xx` 그룹 단위로, 하위 Issue는 독립적으로 개발·검증 가능한 단위로 나눈다.
   - 3장의 완료 항목은 "도입 전 완료 작업"으로 표시하고, 개발자 확인을 받은 뒤 close한다.
   - A/D 이동, 카메라 추적, Parallax 등은 예정 Issue로 둔다.
   - 라벨(`type:*`, `area:*`) 체계를 먼저 승인받고 만든다.
   - 중복 검색 후 초안 전체를 승인받고 등록한다.

### 개발자에게 확인할 사항
- PR Merge 수행 주체 (Codex 또는 개발자)
- 기존 `.claude/`, `CLAUDE.md` 처리 방침
- `main` 브랜치 처리 방침
- `CG-xx` 로드맵 ID 유지 여부
- `BG_Mid`, `BG_Near` 배경 배치 계획과 Parallax 구현 시점
