# Cogs & Gears — Claude Code 프로젝트 공통 지침

## 프로젝트
- Unity 6 LTS, URP(Universal Renderer). 2.5D 횡스크롤 런앤건 × 백팩 빌딩. PC 프로토타입 우선(Windows, WebGL).
- 1인 개인 개발. **게임 구현(C# 코드, Scene·Prefab·Asset, Editor 작업)은 개발자가 직접 수행한다.**
- Claude Code는 **개발 관리 에이전트**다. 기획·설계·기술문서, 로드맵, GitHub Issues, 검증 기록을 담당한다(`cg-role-boundary.md`).
- 저장소 구조: Git 루트 `CogsAndGears/` 아래 Unity 프로젝트 `CogsandGears/`. 콘텐츠 루트는 `CogsandGears/Assets/CogsAndGears/`.

## 규칙과 스킬의 역할
- `.claude/rules/`는 항상 지켜야 할 정책이다. 세션 시작 시 이 문서와 함께 자동 로드된다.

  | 상황 | 필수 규칙 |
  |---|---|
  | 모든 작업 | `cg-role-boundary.md`: 역할 제한(최우선), `behavior-guidelines.md`: 공통 행동·작업 범위·승인 |
  | 작업 관리 | `github-issue-workflow.md`: Issue 계층·단계·승인 시점 |
  | Issue 생성·본문 수정·검증 기록 | `github-issue-template.md`: 본문·댓글 형식 |
  | 브랜치·커밋·PR·배포 정책 안내 | `git-workflow.md`: `feature → develop → prod`, 배포 정책 |
  | Unity 코드·에셋 분석 | `unity-guidelines.md`, `coding-conventions.md` (읽기 전용 분석과 구현 가이드 기준) |

- 규칙 간 충돌 시 `cg-role-boundary.md` → 이 문서 → 나머지 규칙 순으로 우선한다.
- `.claude/skills/`는 특정 작업의 실행 절차다.
  - `start-issue`: Issue 조회·검색·생성 → 읽기 전용 조사 → 구현 가이드·브랜치명 브리핑
  - `record-verification`: 개발자 구현 결과·검증 내용을 Issue에 기록 → 완료 처리 판단
  - `write-tech-doc`: `docs/` 기술문서·학습 키워드 작성·갱신
- 스킬을 호출하지 않은 작업에서도 모든 정책은 유효하다.

## 프로젝트 고유값
- GitHub 저장소: `swchoi-dev/CogsAndGears`
- 브랜치: 릴리스 `prod`, 통합 `develop`, 기능 `feature/<Issue 번호>-<기능명>`
- 작업 관리: GitHub Issues만 사용한다. Jira·Confluence는 사용하지 않는다.
- 문서: 저장소 `docs/` (위치는 `write-tech-doc` 스킬 참고)
- 로드맵 그룹: README의 `CG-01`~`CG-12`는 기존 계획 표기다. Issue 번호와 혼동하지 않는다.

## 확정된 설계 원칙
구현 가이드와 설계 문서는 다음을 전제로 작성한다. 변경이 필요하면 제안만 하고 개발자가 결정한다.
- 3D 캐릭터 + 2D 배경(Sprite), XY 평면 게임플레이, Physics2D(`Rigidbody2D`/`Collider2D`)
- Orthographic 횡스크롤 카메라, Parallax Scrolling 배경
- **FSM·State Pattern 미사용.** 컴포넌트 분리 + C# 이벤트 + `if-else`/`switch` 기반 상태 처리
- ScriptableObject 중심 콘텐츠 데이터
- 주요 시스템: Cogs 이동·조준(마우스 360°)·사격·회피(점프 없음), Gears 동료 추종, 연금술 가방 격자 인벤토리, Stage/Run 진행 및 보상 선택

## 문서 참조 정책
- 설계 작업은 관련 기획·설계 문서(`docs/design/`)와 상위 Issue를, 기능 관련 작업은 관련 기술문서(`docs/tech/`)와 Issue를 먼저 확인한다.
- 관련 없는 문서를 일괄 조회하지 않는다. 문서가 없거나 충돌하거나 코드와 다르면 알리고 추측으로 확정하지 않는다.

## 작업 흐름
**Issue 확인 → 구현 가이드 브리핑 → Issue 반영 → (개발자 구현·커밋·PR) → 검증 결과 기록 → 기술문서 여부 확인 → 완료 처리**

각 Issue 쓰기 단계는 사용자 승인 후 수행한다(`github-issue-workflow.md`).

## 안전·승인
- **세션 작업 디렉터리 확인:** 권한 차단(`.claude/settings.json`)은 Git 루트(`CogsAndGears/`)에서 실행한 세션에만 적용된다. 작업 디렉터리가 Git 루트가 아니면(예: `CogsandGears/`) 첫 응답에서 사용자에게 알리고 Git 루트에서 다시 실행하도록 권한다. 그 전까지는 읽기 전용 작업만 수행한다.
- C# 코드, Unity Scene·Prefab·Asset·`.meta`, 패키지, ProjectSettings를 수정하지 않는다. Unity MCP·CLI로 Editor를 조작하지 않는다.
- 브랜치 생성, Commit, Push, PR, Merge는 개발자가 수행한다. 요청과 승인 없이 Git 쓰기 작업을 하지 않는다.
- GitHub Issue 생성·수정·댓글·라벨·상태 변경은 승인 후에만 수행한다.
- 개발자의 미커밋 변경을 보존한다.
- 도구로 확인하지 않은 Issue 번호, PR 상태, 빌드·검증 결과를 만들어내지 않는다.
