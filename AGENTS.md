# Cogs & Gears — 에이전트 작업 지침

에이전트(Codex, Claude Code)는 기획·설계·기술문서, 로드맵, Git·GitHub Issues 관리, 검증 기록과 함께 Unity 구현·Editor 설정·Play Mode 검증을 수행한다. 개발자는 최종 설계 결정, 작업 방향, 결과 검토와 Merge 승인을 담당한다. 한국어로 소통한다.

## 작업 경계
- Git 루트에서 작업한다. 시작 시 `git rev-parse --show-toplevel`, `git status --short`, `git branch --show-current`로 실제 상태를 확인한다. 인계 문서의 과거 상태를 현재 상태로 간주하지 않는다.
- C# 코드, Scene·Prefab·Asset·Material·Animator·ScriptableObject와 Import Settings는 승인된 작업 범위에서 Unity Editor(Unity MCP·Editor API·Editor Script)를 통해 변경한다. YAML 직접 수정은 최후의 수단이며 GUID·fileID를 추정하지 않는다. Packages·ProjectSettings는 필요한 범위를 밝히고 승인된 경우만 변경한다. 세부 범위는 [역할과 쓰기 범위](.agents/rules/cg-role-boundary.md)를 따른다.
- 승인된 범위에서 `AGENTS.md`, `.agents/`, `docs/`, 요청된 `README.md`를 관리한다. 에이전트별 도구 연결 설정(예: Unity MCP)은 작업에 필요한 범위만 구성한다.
- Git 쓰기와 GitHub 쓰기는 해당 대상·내용·작업에 대한 사용자 승인 범위에서만 수행한다. 기존 승인은 같은 범위에서 재사용하며 새 작업으로 확대하지 않는다. 명시적 금지가 있으면 실행하지 않는다.
- PR Merge 수행 주체는 미확정이다. Merge가 명시적으로 승인되지 않으면 제안까지만 한다. 원본 AI Harness 저장소 반영은 별도 작업이다.
- 개발자의 변경을 보존하고 임의로 stage·restore·stash하지 않는다.

## 규칙 읽기
`.agents/rules/`는 자동 로드 설정이 아니다. 모든 작업 전에 다음 두 규칙을 읽고 준수한다.
- [역할과 쓰기 범위](.agents/rules/cg-role-boundary.md)
- [행동·승인·검증](.agents/rules/behavior-guidelines.md)

관련 작업에서는 다음 규칙도 읽는다. 프로젝트 내부 충돌은 역할 제한, 작업별 규칙 순으로 해석하되 상위 지침과 현재 사용자의 명시적 범위를 우선한다.

| 작업 | 규칙 |
|---|---|
| Git·PR·릴리스 | [git-workflow](.agents/rules/git-workflow.md) |
| Issue·로드맵·완료 판단 | [github-issue-workflow](.agents/rules/github-issue-workflow.md) |
| Issue 초안·검증 댓글 | [github-issue-template](.agents/rules/github-issue-template.md) |
| Unity 구현·Editor 설정·검증 | [unity-guidelines](.agents/rules/unity-guidelines.md), [coding-conventions](.agents/rules/coding-conventions.md) |

## 프로젝트 기준
- GitHub: `swchoi-dev/CogsAndGears`. Unity 루트: `CogsandGears/`. 콘텐츠: `CogsandGears/Assets/CogsAndGears/`.
- Unity 6 URP, 3D 캐릭터 + 2D 배경, XY 평면 Physics2D, Orthographic 횡스크롤, Parallax 배경.
- 컴포넌트 책임 분리 + C# 이벤트 + `enum`/`if-else`/`switch`. 별도 FSM·State Pattern은 사용하지 않는다. 콘텐츠 데이터는 ScriptableObject 중심이다.
- Cogs 이동·360° 조준·공격·회피(점프 없음), Gears 추종, 격자 가방, Stage/Run 및 보상 선택이 핵심 시스템이다.
- 작업 관리에는 GitHub Issues를 사용한다. Jira·Confluence는 사용하지 않는다. `CG-01`~`CG-12`는 로드맵 그룹이며 Issue 번호가 아니다.
- 설계 시 관련 `docs/design/`와 상위 Issue, 기능 조사 시 관련 `docs/tech/`와 Issue를 우선 확인한다. 없는 문서나 확인하지 못한 상태는 명시한다.

## Skills와 Templates
스킬은 `.agents/skills/<name>/SKILL.md`에 둔다. `$start-issue`, `$record-verification`, `$write-tech-doc`를 명시적으로 호출하거나 관련 작업에 적용한다. 스킬 호출 자체는 외부 쓰기 승인이 아니다.

| Skill | 목적 |
|---|---|
| [start-issue](.agents/skills/start-issue/SKILL.md) | Issue 조회·중복 조사·구현 가이드 |
| [record-verification](.agents/skills/record-verification/SKILL.md) | 근거별 검증 기록·완료 조건·close 판단 |
| [write-tech-doc](.agents/skills/write-tech-doc/SKILL.md) | 기술·설계 문서와 학습 키워드 관리 |

초안은 [.agents/templates/](.agents/templates/)를 사용한다. 출처·이관 차이·보호 한계·검증 기록은 [HARNESS_SOURCE](.agents/HARNESS_SOURCE.md)에 관리한다. 이 문서와 규칙은 행동 지침이며 파일 시스템 접근 차단 설정이 아니다.
