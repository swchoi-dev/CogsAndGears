# AI Harness 적용 기록

- 원본: https://github.com/swchoi-dev/ai-harness
- 기준 커밋: `38c8c8d` ([docs] 논리적 변경 단위 기준 커밋 정책 추가)
- 적용일: 2026-11-10
- 적용 방식: `shared/templates/project-claude.md` 주석의 수동 복사 방식 (원본의 `presets/`, `scripts/`는 아직 비어 있음)
- 원본 저장소는 수정하지 않았다.

## 파일 매핑

| 프로젝트 파일 | 원본 | 구분 | 변경 내용 |
|---|---|---|---|
| `CLAUDE.md` | `shared/templates/project-claude.md` | 재작성 | Jira·Confluence·Atlassian 항목 제거, 역할 제한·설계 원칙·GitHub Issues 반영 |
| `.claude/rules/behavior-guidelines.md` | `shared/rules/behavior-guidelines.md` | 복사 + 수정 | 3행·99행의 Jira 참조를 GitHub Issue·역할 제한 규칙 참조로 교체 |
| `.claude/rules/unity-guidelines.md` | `stacks/unity/unity-guidelines.md` | 복사 + 수정 | 6장(작업자별 Test Scene 격리, Atlassian 계정 식별) 삭제 |
| `.claude/rules/coding-conventions.md` | `stacks/unity/coding-conventions.md` | 그대로 복사 | 없음 (읽기 전용 분석·구현 가이드 기준으로 사용) |
| `.claude/rules/git-workflow.md` | `shared/rules/git-workflow.md` | 복사 + 수정 | 브랜치명 `<KEY>-번호-slug` → `feature/<Issue 번호>-<기능명>`, PR 제목 `Merge: #번호`, Git 작업 주체를 개발자로 변경, `Refs`/`Closes` 규칙 추가, 7장 배포 정책 신설 |
| `.claude/rules/github-issue-workflow.md` | `shared/workflows/github-issue.md` + `integrations/jira/jira-workflow.md` | 재구성 | 입력 분류·작업지시서 보관 + 단계별 승인 구조를 GitHub Issues와 개발자 직접 구현 모델로 재구성, 상위/하위 Issue(Sub-issues) 추가 |
| `.claude/rules/github-issue-template.md` | `shared/templates/github-issue.md` + `integrations/jira/jira-issue-template.md` | 재구성 | 기술 학습 키워드·참고 문서·작업 범위(포함/제외)·검증 기록 댓글 형식 추가 |
| `.claude/rules/cg-role-boundary.md` | — | 신규 | Claude Code 허용·금지 업무, 쓰기 가능 위치 |
| `.claude/skills/start-issue/` | `agents/claude/skills/start-issue/` | 재작성 | Jira·Atlassian MCP → `gh`, 브랜치 생성·구현 단계 제거, 구현 가이드 브리핑으로 변경 |
| `.claude/skills/write-tech-doc/` | `agents/claude/skills/write-tech-doc/` | 재작성 | Confluence → 저장소 `docs/`, `NN.` 넘버링을 파일명 규칙으로 변경, 학습 키워드 연계 |
| `.claude/skills/record-verification/` | — | 신규 | 개발자 구현 결과·검증 기록 및 완료 처리 판단 |
| `.claude/settings.json` | — | 신규 | Unity 파일 쓰기·Unity MCP·`unity` CLI·파괴적 Git 차단, Git·GitHub 쓰기 승인 요청(Bash·PowerShell 모두), Unity 플러그인 프로젝트 범위 비활성화 |

- 출처 기록은 이 문서에서만 관리한다. `CLAUDE.md`, `.claude/rules/`, `.claude/skills/`에는 원본 경로나 Jira 파일을 참조하지 않는다.

## 제외 항목

| 원본 | 제외 사유 |
|---|---|
| `integrations/jira/jira-workflow.md`, `jira-issue-template.md` | Jira 미사용 (구조만 GitHub 규칙에 재구성) |
| `agents/claude/skills/create-pr/` | PR은 개발자가 직접 생성. `Merge: <KEY>` 형식 미사용 |
| `unity-guidelines.md` 6장 | 1인 프로젝트, Atlassian 계정 기반 작업자 식별 불필요 |
| `project-claude.md`의 Jira·Confluence·Atlassian MCP 문서 조회 정책 | Jira·Confluence 미사용 |
| `docs/work-orders/` (원본 저장소의 작업지시서) | 원본 저장소 전용 |

## 동기화 절차
1. 원본 저장소의 `38c8c8d` 이후 변경을 확인한다: `gh api repos/swchoi-dev/ai-harness/compare/38c8c8d...main --jq '.files[].filename'`
2. 위 매핑 표에서 해당 파일의 구분을 확인한다.
   - 그대로 복사: 원본으로 교체한다.
   - 복사 + 수정: 원본 변경을 반영한 뒤 "변경 내용" 열의 수정을 다시 적용한다.
   - 재작성·재구성: 원본 변경 취지만 검토해 반영 여부를 판단한다.
3. 반영 후 이 문서의 기준 커밋과 적용일을 갱신한다.

## 권한 설정 검증 기록 (2026-11-10, Claude Code 2.1.302)
스크래치 경로의 복제 저장소에서 headless 세션(`claude -p`)으로 확인했다. 실제 프로젝트 파일은 사용하지 않았다.

| 항목 | 결과 |
|---|---|
| `Edit`/`Write`로 `CogsandGears/**`, `*.cs`, `*.meta` 쓰기 | 차단됨 |
| Bash `>`, `cp`, `sed -i` / PowerShell `Set-Content`로 보호 경로 쓰기 | 차단됨 (Edit 차단 규칙이 경로 분석에 적용) |
| `node -e`, PowerShell `[IO.File]::WriteAllText` 등 인터프리터 쓰기 | **규칙으로 차단 안 됨.** 승인 모드에서는 사용자 승인 필요, auto 모드에서는 분류기가 거부 |
| Bash·PowerShell `git`/`gh` 쓰기 명령 | `ask` 적용. 넓은 `allow`가 있어도 `ask`가 우선 |
| `git reset --hard`, `unity` CLI (Bash·PowerShell) | 차단됨 (`deny`가 `ask`·`allow`보다 우선) |
| `enabledPlugins`로 Unity 플러그인 비활성화 | `unity:*` 스킬 미노출 확인 |
| Git 루트에서 실행 | `CLAUDE.md`, rules 7개, skills 3개 로드, 권한 적용 |
| Unity 하위 폴더(`CogsandGears/`)에서 실행 | `CLAUDE.md`·rules·skills는 로드되지만 **`settings.json` 미적용**(권한 차단·플러그인 비활성화 모두 무효) |

## 사용 시 주의
- Claude Code는 반드시 **Git 루트(`CogsAndGears/`)에서 실행**한다. 하위 폴더에서 실행하면 권한 차단이 적용되지 않는다(`CLAUDE.md`의 세션 작업 디렉터리 확인 지침으로만 보완).
- 인터프리터를 통한 파일 쓰기는 권한 규칙으로 완전히 막을 수 없다. 승인 프롬프트에서 `CogsandGears/` 경로를 쓰는 명령을 승인하지 않는다.
- 사용자 전역 설정(`~/.claude/settings.json`)의 `PermissionRequest` HTTP hook(Clawd on Desk)이 승인 요청을 처리할 수 있다. 해당 앱에서의 승인도 동일하게 취급한다.
- 사용자 전역 `autoMode.environment`는 다른 프로젝트(Fourbeat) 기준으로 작성되어 있다. auto 모드 분류기가 이 프로젝트를 다른 저장소로 인식할 수 있다.
- Unity MCP 서버는 현재 미설정이다. `mcp__unity*` 차단은 예비 규칙이며, 서버를 추가하면 실제 서버 이름을 `deny`에 등록한다.
