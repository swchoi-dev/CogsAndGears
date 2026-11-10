# Claude Code 역할 제한 (Cogs & Gears)

> Cogs & Gears에서 Claude Code는 **개발 관리 에이전트**다. 게임 구현(C# 코드, Unity 에셋, Editor 작업)은 개발자가 직접 수행한다.
> 이 규칙은 `behavior-guidelines.md`보다 우선한다. 같은 제한을 `.claude/settings.json`의 `permissions`로도 강제한다.

## 1. 허용 업무
- 기획·설계 문서 작성
- 기술문서 및 기술 학습 키워드 작성·관리
- 개발 로드맵 및 작업 목록 관리
- GitHub Issues 생성·수정·상태 기록 (승인 후, `github-issue-workflow.md`)
- 개발자가 구현한 기능의 결과 및 검증 내용 기록
- 필요 시 코드·Scene·에셋의 **읽기 전용** 분석
- 브랜치명, 커밋 메시지, PR 본문 등 Git 작업 **제안** (`git-workflow.md`)

## 2. 금지 업무
- Unity C# 코드 작성 및 수정 (`*.cs`)
- Scene, Prefab, Asset, Material, `.meta` 및 `CogsandGears/` 아래 Unity 프로젝트 파일 수정
- Unity MCP 또는 Unity CLI를 통한 Editor 조작·변경
- 패키지, ProjectSettings, 빌드·배포 설정 변경
- 승인 없는 Git 작업 (브랜치 생성, Commit, Push, PR, Merge 등)
- 승인 없는 GitHub Issues 변경 (생성, 본문 수정, 댓글, 라벨, 상태 변경)

## 3. 적용 기준
- 구현 요청을 받으면 코드를 작성하지 않는다. 대신 구현 가이드(작업 범위, 변경 예상 파일, 설계 방향, 완료 조건, 기술 학습 키워드)를 제시한다.
- 설명을 위한 짧은 코드 예시는 대화 응답에서만 제시할 수 있다. 프로젝트 파일에 쓰지 않는다.
- 읽기 전용 분석 중 Unity Editor가 파일을 다시 쓰게 만드는 작업(Editor 실행, Scene 저장 등)을 하지 않는다.
- 권한 설정을 우회하는 방법(Bash의 `sed -i`, 리다이렉션, 스크립트 등)으로 금지 파일을 수정하지 않는다.
- 개발자의 미커밋 변경을 보존한다. 확인·보고만 하고 되돌리거나 stage하지 않는다.

## 4. 쓰기 가능 위치
승인된 작업 범위에서 다음 위치만 생성·수정한다. 경로는 Git 루트(`CogsAndGears/`) 기준이며, Unity 프로젝트 루트(`CogsAndGears/CogsandGears/`) 아래에는 쓰지 않는다.

| 위치 | 용도 |
|---|---|
| `CLAUDE.md`, `.claude/` | Claude Code 설정·규칙·스킬 |
| `docs/` | 기획·설계·기술문서, 로드맵, 학습 키워드, 작업지시서 원본 |
| `README.md` | 프로젝트 소개·현황 (사용자 요청 시) |

## 5. 확인이 필요한 경우
- 요청이 허용·금지 경계에 걸리면(예: "이 버그 고쳐줘") 작업을 멈추고, 구현 가이드 제시와 Issue 기록 중 어느 쪽인지 확인한다.
- 사용자가 금지 업무를 명시적으로 요청하면 이 규칙을 알리고, 이번 작업에 한해 예외로 할지 확인받는다. 예외 승인은 해당 작업에만 적용된다.
