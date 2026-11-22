# Claude Code 역할 범위 (Cogs & Gears)

> Cogs & Gears에서 Claude Code는 **개발 에이전트**다. 기획·문서·Issue 관리와 함께 게임 구현(C# 코드, Unity 에셋, Editor 작업)을 수행한다.
> 이 규칙은 `behavior-guidelines.md`보다 우선한다. 권한은 `.claude/settings.json`의 `permissions`로도 관리한다.

## 1. 허용 업무
- 기획·설계 문서, 기술문서 및 기술 학습 키워드 작성·관리
- 개발 로드맵 및 작업 목록 관리
- GitHub Issues 생성·수정·상태 기록 (승인 후, `github-issue-workflow.md`)
- Unity C# 코드 작성·수정·삭제
- Scene, Prefab, Animator Controller, Animation Clip, ScriptableObject, Behavior Graph 등 `CogsandGears/Assets/` 아래 에셋 수정
- Unity 공식 플러그인(`unity@unity-agent-plugin`)과 Unity CLI를 통한 Editor 조회·조작
- 브랜치 생성, Commit, PR 작성 (`git-workflow.md`)

## 2. 승인이 필요한 업무
- 기능 구현 착수 (`behavior-guidelines.md` 6장 구현 브리핑 후 승인)
- 패키지 추가·업그레이드, ProjectSettings, 빌드·배포 설정 변경
- Push, PR 생성·수정, GitHub Issues 변경 (생성, 본문 수정, 댓글, 라벨, 상태 변경)

## 3. 금지 업무
- PR 병합 (개발자가 수행)
- 강제 Push, `git reset --hard`, `git clean`, 브랜치 강제 삭제 등 이력·작업물 유실 작업
- 인증정보(`~/.ssh`, `~/.aws`, `gh` 토큰, `.env` 등) 조회·수정
- `Library/`, `Temp/` 등 Unity 생성 폴더 직접 수정

## 4. 적용 기준
- 개발자의 미커밋 변경을 보존한다. 확인 없이 되돌리거나 덮어쓰지 않는다.
- Scene·Prefab·에셋은 가능하면 Unity Editor(플러그인·CLI)를 통해 수정하고, YAML 직접 수정 시 `.meta`·GUID·직렬화 참조를 보존한다(`unity-guidelines.md`).
- 권한 설정을 우회하는 방법(Bash 리다이렉션, 인터프리터 스크립트 등)으로 금지 대상을 수정하지 않는다.
- 요청이 허용·금지 경계에 걸리면 작업을 멈추고 확인한다.
