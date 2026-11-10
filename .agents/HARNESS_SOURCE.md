# Codex Harness 출처와 적용 기록

- 적용일: 2026-11-10. 범위: 승인된 Step 3의 `AGENTS.md`, `.agents/` Rules·Skills·Templates 생성.
- 로컬 기준: `1d7a38ed158b5743a6a928447aa5dfc4c0e1ba75`, 브랜치 `prod`. 시작 작업 트리는 깨끗했다. 인계 문서의 `main` 단독 상태는 과거 기록이며 현재 사실로 전용하지 않았다.
- 활용 자료: [인계 문서](../docs/handover/claude-to-codex.md), 기존 `CLAUDE.md`, `.claude/HARNESS_SOURCE.md`, `.claude/rules/` 7개와 `.claude/skills/` 3개. Claude 파일은 아래 정합성 검사를 거쳐 사용자 승인으로 제거했다. 이전 내용은 Git 기준 커밋 `1d7a38ed158b5743a6a928447aa5dfc4c0e1ba75`에서 조회할 수 있다. 예: `git show 1d7a38ed158b5743a6a928447aa5dfc4c0e1ba75:.claude/HARNESS_SOURCE.md`.
- 원본 AI Harness: `swchoi-dev/ai-harness`, 기존 Claude 출처 기록의 기준 `38c8c8d`. 이번 작업에서는 원본을 조회·동기화·변경하지 않았다. 원본 최신 버전 적용을 주장하지 않는다.

## 로컬 이관 매핑
| 생성 파일 | 활용 자료 | Codex 적용 차이 |
|---|---|---|
| `AGENTS.md` | `CLAUDE.md`, 인계 문서 | Codex 진입점·규칙 명시적 읽기·역할·프로젝트 기준 |
| `rules/cg-role-boundary.md` | 동명 Claude 규칙, 인계 문서 6장 | 승인된 Git 관리를 Codex 업무로 허용, Merge는 명시적 승인 필요, Unity 금지 유지 |
| `rules/behavior-guidelines.md` | 동명 Claude 규칙 | 승인 범위 재사용, 현재 명시적 제한 우선, 사실·근거 구별 |
| `rules/git-workflow.md` | 동명 Claude 규칙 | 승인된 Codex Git 관리로 수행 주체 변경, 브랜치·커밋·PR·배포 정책 유지 |
| `rules/github-issue-workflow.md` | 동명 Claude 규칙 | Issue 계층·검증·단계별 쓰기 승인 유지, 쓰기 금지 시 로컬 초안 완료 |
| `rules/github-issue-template.md` | 동명 Claude 규칙 | 작성 기준과 복사용 템플릿 분리 |
| `rules/unity-guidelines.md` | 동명 Claude 규칙 | 읽기 전용 분석으로 한정, 존재하지 않는 6장 참조 제거 |
| `rules/coding-conventions.md` | 동명 Claude 규칙 | 분석·가이드 기준임을 명시, 게임 테스트 생성 지시 제거 |
| `skills/start-issue/SKILL.md` | 동명 Claude 스킬 | Codex frontmatter, 상대 링크, 대화 입력, 저장소 명시 |
| `skills/record-verification/SKILL.md` | 동명 Claude 스킬 | 근거 구별·초안·승인된 반영과 close 분리 |
| `skills/write-tech-doc/SKILL.md` | 동명 Claude 스킬 | docs 경로·번호 유지, 승인된 로컬 작업의 중복 승인 제거 |
| `templates/issue-task.md`, `issue-epic.md`, `verification.md` | Claude Issue 템플릿 | 실제 복사용 분리 |
| `templates/implementation-brief.md`, `tech-doc.md`, `pr.md` | Claude 스킬과 Git 정책 | 관리용 초안 형식으로 정리 |

표의 생성 경로는 이 파일이 있는 `.agents/` 기준이다. `AGENTS.md`만 Git 루트에 있다. 공통 정책은 이 저장소용으로 재구성했으며 원본과의 자동 동기화는 없다. 동기화 시 원본 diff와 이 매핑을 대조하고 역할·사용자 승인 경계를 유지한다.

## 인식과 보호의 구별
- [공식 Skills 문서](https://developers.openai.com/codex/skills)에 맞춰 `.agents/skills/<name>/SKILL.md`에 `name`, `description` frontmatter를 둔다. Claude 전용 `argument-hint`, `disable-model-invocation`, `${ARGUMENTS}`는 사용하지 않는다. 선택적 `openai.yaml`은 만들지 않으며 기본 자동 선택을 유지한다.
- [공식 AGENTS.md 문서](https://developers.openai.com/codex/guides/agents-md)에 맞춰 루트 지침을 둔다. `.agents/rules/`와 `templates/` 자체의 자동 로드를 가정하지 않고 진입점·스킬에서 연결한다.
- Claude `settings.json`은 Codex 설정이 아니다. `.codex/config.toml`이나 전역 설정을 만들거나 변경하지 않았다. 현재 sandbox의 workspace-write는 Unity 하위 파일까지 세밀하게 차단하는 보호와 동등하지 않다.
- Unity 플러그인 스킬은 현재 세션에도 노출되어 있다. 이 Harness가 플러그인을 비활성화하거나 Editor 조작을 기술적으로 차단했다고 주장하지 않는다. 역할 지침으로 사용을 금지한다.
- 보호 대상 쓰기나 외부 변경을 시도하는 파괴적 검증은 하지 않는다. 형식 검증과 런타임 인식 검증, 실제 Issue 실행 검증은 구별한다.

## 검증 기록
2026-11-10, Codex CLI `0.167.0`에서 확인했다.

| 검증 | 결과 |
|---|---|
| 생성 범위 | Markdown 18개: 루트 지침 1, 출처 기록 1, Rules 7, Skills 3, Templates 6 |
| 기존 파일 보존 | 시작 시 저장한 추적 파일 118개의 SHA-256과 비교, 변경·누락 없음. Claude 설정과 Unity 추적 파일 포함 |
| Git 상태 | `prod`, HEAD `1d7a38ed158b5743a6a928447aa5dfc4c0e1ba75` 유지, 새 파일만 untracked |
| 참조·인코딩 | 생성 Markdown의 로컬 링크 대상 모두 존재, UTF-8 읽기 및 대체 문자 검사 통과 |
| Skill 형식 | 필수 name·description, 폴더명 일치, 이름 길이·문자, Claude 전용 필드 부재 확인 |
| 실제 Skill 인식 | 로컬 app-server의 initialize → skills/list(forceReload)로 3개 모두 `scope: repo`, `enabled: true`, 검색 오류 없음 확인. 모델 turn·별도 에이전트 작업은 실행하지 않음 |
| 기본 검사기 한계 | skill-creator의 quick_validate.py는 Python 환경의 PyYAML 미설치로 실행 실패. 의존성을 설치하지 않고 위 정적 검사와 실제 Codex 로더로 대체 |
| Git whitespace | `git diff --check` 오류 없음. untracked 파일은 별도 검사 대상으로 취급 |

일반 sandbox 셸은 생성 후 setup refresh 오류로 시작되지 않아 읽기 전용 검증을 승인 경로에서 실행했다. `codex.ps1`은 PowerShell 실행 정책으로 실행되지 않아 설치된 `codex.cmd`를 사용했다. 실행 정책·sandbox·전역 설정은 변경하지 않았다.

실제 Issue 게시·수정·close, Unity 실행·빌드·게임 검증, Git Commit·Push·PR·Merge·브랜치 변경은 수행하지 않았다. 현재 대화의 시작 시점 스킬 목록 갱신 여부와 UI 선택기는 별도로 확인하지 않았으며, 위 인식 결과는 로컬 Codex app-server의 실제 검색 결과다.

## Claude Harness 제거 정합성 검사 (2026-11-10)

사용자가 이번 요청에서 Git 루트 `CLAUDE.md`와 `.claude/` 전체 삭제를 승인했다. 아래는 제거 전 비교 결과이며, 위 Step 3 기록은 당시 상태를 보존한다.

| 비교 대상·기준 | 결과·Codex 대응 |
|---|---|
| `CLAUDE.md` → `AGENTS.md` | 역할·설계 원칙·실제 경로·문서 우선 조회·Issue 번호 구별·검증 근거 유지. Codex 전용 운영으로 보존 문구를 정리 |
| Rules 7개 → Rules 7개 | 필수 관리 규칙 누락 없음. Unity 규칙은 읽기 전용 분석으로 한정. 구현 권한을 확대하지 않음 |
| Skills 3개 → Skills 3개 | Issue 시작·검증 기록·기술문서 절차 유지. Claude 전용 입력 변수와 frontmatter 제외 |
| GitHub Issues | 상위/하위·Sub-issues·라벨·중복 검색·단계별 승인·기존 본문 보존·재조회·검증 후 close 유지 |
| 기술문서 | `docs/design/`, `docs/tech/NN-*.md`, `docs/learning/keywords.md`, `docs/roadmap.md`, 기존 번호 보존·최댓값+1·설계 의도·흐름·근거·변경 이력 유지 |
| Git 형상관리 지원 | feature→develop→prod·논리적 커밋·Refs/Closes·PR 제목·merge commit·배포 정책 유지. 수행 주체는 인계에 따라 승인된 Codex 관리로 변경 |
| 사용자 승인 | 내용·대상·범위에 대한 승인과 쓰기 후 확인 유지. 같은 범위의 기존 승인 재사용. PR·댓글 승인을 Merge·close 승인으로 확대하지 않음 |
| Unity 금지 | 전체 `CogsandGears/`, C#·Scene·Prefab·Asset·Material·meta·Packages·ProjectSettings 변경 금지, Editor·MCP·CLI 조작 금지 유지 |
| settings.json → 실제 운영 | Claude deny/ask·enabledPlugins·hook은 Claude 전용으로 제외. Codex는 현재 sandbox와 명령 승인 정책 + 역할 지침을 사용. workspace-write가 Unity 파일별 deny를 재현하지는 않으며 플러그인도 비활성화되지 않음. 동등한 강제 보호를 주장하지 않음 |
| HARNESS_SOURCE | 로컬 이관 매핑·원본 기준·제외 항목·보호 한계를 보존. 삭제 경로의 로컬 링크를 Git 이력 조회 안내로 교체 |

문서 작업의 폴더 생성·초안 처리·Git 수행 주체 차이는 Step 3에서 기록한 Codex 운영상 변경이며 이관 누락으로 분류하지 않았다. 이번 검사로 새 필수 규칙을 만들어 누락을 숨기지 않았다.

### AI Harness 공통화 후보와 제외
- Agent 중립 후보: 행동·승인·증거 구분, 역할 경계, Git/PR/배포 정책, GitHub Issues 계층·진행·검증·완료 정책, 기술문서 번호·학습 키워드, 관리용 템플릿 6개.
- Unity 스택 후보: 읽기 전용 Unity 조사, GUID·직렬화 보호, C# 가이드 기준. Cogs & Gears 고유 경로·FSM 미사용·시스템·브랜치 정책은 프로젝트 설정으로 분리 검토한다.
- Codex 어댑터: 루트 `AGENTS.md` 진입점, 명시적 Rules 참조, `.agents/skills/` 검색 경로와 name/description, 대화 입력 방식.
- 제외: Claude permissions/프런트매터/자동 Rules 로드/enabledPlugins/hook/CLI 실행 의존성. Jira·Confluence·Atlassian 연동, 원본 저장소 전용 작업지시서, 작업자별 Scene 격리도 도입하지 않는다.
- 원본 출처 계보: 행동·Git 정책은 원본 `shared/rules/`, Unity·C#은 `stacks/unity/`, Issue 정책·형식은 `shared/workflows/github-issue.md`와 `shared/templates/github-issue.md`를 기반으로 기존 Claude Harness에서 재구성했다. 과거 Jira 자료에서 취한 구조는 현재 GitHub 규칙으로 재작성돼 실행 의존성이 없다. 세부 원본 매핑은 위 Git 이력의 출처 기록에서 조회할 수 있다.
- 후보 기록만 수행한다. 원본 `swchoi-dev/ai-harness` 반영·조회·변경은 이번 작업에 포함하지 않는다.

### 삭제 대상 — 제거 전 확인한 정확한 13개 파일
```text
CLAUDE.md
.claude/HARNESS_SOURCE.md
.claude/settings.json
.claude/rules/behavior-guidelines.md
.claude/rules/cg-role-boundary.md
.claude/rules/coding-conventions.md
.claude/rules/git-workflow.md
.claude/rules/github-issue-template.md
.claude/rules/github-issue-workflow.md
.claude/rules/unity-guidelines.md
.claude/skills/record-verification/SKILL.md
.claude/skills/start-issue/SKILL.md
.claude/skills/write-tech-doc/SKILL.md
```

보존 대상은 `AGENTS.md`, `.agents/` 전체, 인계 문서, Unity 프로젝트, Git 이력과 기존 사용자 작업이다. 현재 운영 지침의 제거 대상 참조만 정리한다. 인계 문서와 이 출처 기록에 남은 Claude·Jira·Confluence 이름은 과거 이관 기록 또는 제외 설명이며 실행 의존성이 아니다.

### 제거 후 검증 결과
- 이번 요청의 신규 파일 0개, 수정 3개(`AGENTS.md`, `rules/cg-role-boundary.md`, 이 기록), 삭제 13개. 앞선 Step 3에서 생성한 Codex 파일 18개는 아직 미추적 상태이므로 git status에는 수정 3개도 `??`로 표시된다. stage하지 않았다.
- 시작 시 파일 136개의 SHA-256을 기록해 비교했다. 승인된 삭제 13개·수정 3개 이외는 동일하다. Unity 102개(추적 파일과 Assets/Packages/ProjectSettings의 실제 파일 포함), 인계 문서, 나머지 Rules·Skills·Templates를 보존했다. Unity 캐시·생성 산출물 전체의 바이트 동일성은 검사 범위에 포함하지 않았다.
- 로컬 app-server `skills/list(forceReload)`에서 `start-issue`, `record-verification`, `write-tech-doc` 모두 `scope: repo`, `enabled: true`, 검색 오류 없음. 현재 세션의 제공 스킬 목록에도 3개가 노출돼 있다.
- `AGENTS.md`는 Git 루트에서 읽고 적용했다. 크기 4,276바이트, 루트 `AGENTS.override.md` 없음, `config/read`의 `project_doc_max_bytes=32768`, fallback 빈 목록 확인. 탐색 조건과 참조는 정상이지만 새 모델 세션에서 자동 주입되는 과정 자체는 모델 turn을 실행하지 않아 미검증이다.
- 루트 지침과 `.agents/` Markdown의 상대 링크는 모두 유효하다. Rules·Skills·Templates에 삭제된 Claude 설정 링크·실행 명령·Claude 전용 스킬 입력·Atlassian MCP 의존성 없음. Jira·Confluence 미사용 선언과 역사적 출처 기록은 유지했다.
- `git diff --check`와 미추적 Markdown의 별도 공백 검사 통과. Git 삭제 목록은 위 manifest와 정확히 일치한다. `prod`와 HEAD `1d7a38ed158b5743a6a928447aa5dfc4c0e1ba75` 유지.
- 현재 도구 세션의 권한은 workspace-write·명령별 승인 정책이다. 별도 app-server `config/read`에서 sandbox_mode·approval_policy가 null인 것은 해당 설정값이 명시되지 않았다는 뜻이며 현재 세션의 권한이 해제됐다는 증거로 해석하지 않았다.
- `.claude/` 삭제 전 절대 경로·재분석 지점 부재·manifest·파일 해시를 확인했다. 사용자 전역 설정·다른 프로젝트·원본 Harness·Git 이력·브랜치·GitHub에는 쓰지 않았다. 일반 셸의 sandbox setup 오류 때문에 검증과 승인된 삭제는 별도 승인 실행 경로를 사용했다.
