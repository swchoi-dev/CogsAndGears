# Git·PR·배포 정책

역할 제한은 [cg-role-boundary.md](cg-role-boundary.md)를 따른다. 읽기 전용 상태 확인과 초안 작성은 가능하다. Git 쓰기는 승인된 범위에서만 수행하며 Unity 구현 권한을 포함하지 않는다.

- 브랜치: `feature/<실제 Issue 번호>-<영문 kebab-case 기능명>` → `develop` → `prod`.
- 기능 브랜치는 실제 존재를 확인한 `develop`에서 분기한다. `prod`에 직접 개발 커밋하지 않는다. 현재 브랜치와 기본 브랜치는 매번 조회하며 이름·존재를 인계 문서에서 추정하지 않는다.
- 기존 브랜치 이름 변경·삭제, 기본 브랜치·보호 규칙 변경은 해당 작업이 승인된 경우만 수행한다.
- Commit 형식: `[feat|fix|docs|chore] 변경 내용`. Bug Issue 수정은 `fix`, 문서 작업은 `docs`다.
- Commit은 논리적으로 완결된 변경 단위로 묶는다. 같은 목적의 변경을 파일별로 쪼개거나 무관한 작업을 합치지 않는다. 관련 파일만 명시적으로 stage한다.
- 중간 연결은 `Refs #번호`. `Closes #번호`는 완료 검증 후 사용하며 기본 브랜치 병합에 따른 자동 close 효과를 먼저 확인한다.
- 기능 PR 제목: `Merge: #<Issue 번호> 작업명`, 릴리스: `Release: 릴리스 내용`.
- PR 본문은 [pr.md](../templates/pr.md)를 사용해 문제·변경·검증·미검증·위험을 기술한다.
- 병합 방식은 Create a merge commit이다. Squash·Rebase merge로 변경하지 않는다. PR 생성 승인은 Merge 승인이 아니다. Merge 수행은 명시적으로 승인된 경우만 진행한다.
- 강제 Push, `reset --hard`, `clean`, 임의 restore·stash·브랜치 삭제를 하지 않는다. 승인 없는 merge/rebase를 하지 않는다. 사용자 변경과 `.meta`/GUID를 보존한다.
- 캐시·빌드 산출물·비밀 정보를 커밋하지 않는다. CI 성공은 실제 실행 결과로 확인한다.
- WebGL 운영 배포 기준은 `prod`, 향후 Actions 자동화 예정이다. Windows는 개발자가 수동 빌드한다. 정책만으로 빌드·배포 자동화를 생성하거나 실행하지 않는다.
