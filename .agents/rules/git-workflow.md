# Git·PR·배포 정책

역할 제한은 [cg-role-boundary.md](cg-role-boundary.md)를 따른다. Codex는 커밋 메시지·브랜치명·PR 내용 추천과 읽기 전용 확인만 수행한다. 실제 Git Commit·Push·브랜치 작업·Merge·PR 작업은 개발자가 직접 수행하며 Codex는 실행하지 않는다. 아래 정책은 개발자가 수행하는 작업의 기준이며 Unity 구현 권한을 부여하지 않는다.

- 브랜치: `feature/<실제 Issue 번호>-<영문 kebab-case 기능명>` → `develop` → `prod`.
- 기능 브랜치는 실제 존재를 확인한 `develop`에서 분기한다. `prod`에 직접 개발 커밋하지 않는다. 현재 브랜치와 기본 브랜치는 매번 조회하며 이름·존재를 인계 문서에서 추정하지 않는다.
- 기존 브랜치 이름 변경·삭제, 기본 브랜치·보호 규칙 변경은 해당 작업이 승인된 경우만 수행한다.
- 커밋 메시지는 [Conventional Commits v1.0.0 공식 명세](https://www.conventionalcommits.org/ko/v1.0.0/)를 따른다. 형식은 `<type>[optional scope][!]: <description>`이며, 선택 범위 표기는 실제 메시지에서 괄호로 작성한다: `type(scope)!: 설명`.
- `type`은 소문자 영어, `scope`는 선택적인 변경 범위, `description`은 간결한 한국어로 작성할 수 있다. 접두부와 설명 사이에는 콜론과 공백(`: `)을 사용한다. 대괄호 접두어는 사용하지 않는다.
- `feat`는 기능 추가, `fix`는 버그 수정이다. 나머지는 프로젝트에서 채택한 관례적 타입이며 공식 명세의 필수 타입 목록은 아니다.

  | 타입 | 용도 |
  |---|---|
  | `docs` | 문서 변경 |
  | `refactor` | 기능 변경 없는 코드 구조 개선 |
  | `test` | 테스트 추가·수정 |
  | `chore` | 유지보수 및 기타 작업 |
  | `build` | 빌드 시스템·의존성 변경 |
  | `ci` | CI/CD 설정 변경 |
  | `perf` | 성능 개선 |
  | `style` | 동작 변경 없는 코드 스타일 수정 |

- Breaking Change는 타입/범위 뒤, 콜론 바로 앞의 `!` 또는 본문과 빈 줄로 구분한 꼬리말 `BREAKING CHANGE: <설명>`으로 표시한다. 본문은 제목 다음 빈 줄로 구분한다. 어떤 타입에도 Breaking Change를 표시할 수 있다.
- 커밋 메시지 추천 예시:

  ```text
  feat(player): 플레이어 이동 기능 추가
  fix(inventory): 아이템 회전 시 배치 오류 수정
  docs(harness): Codex 작업 수행 규칙 개선
  refactor(combat): 공격 처리 로직 분리
  chore: 개발 환경 설정 정리
  ```

- 새 커밋 메시지에 적용하며 기존 커밋 이력은 변경하지 않는다.
- Commit은 논리적으로 완결된 변경 단위로 묶는다. 같은 목적의 변경을 파일별로 쪼개거나 무관한 작업을 합치지 않는다. 관련 파일만 명시적으로 stage한다.
- 중간 연결은 `Refs #번호`. `Closes #번호`는 완료 검증 후 사용하며 기본 브랜치 병합에 따른 자동 close 효과를 먼저 확인한다.
- 기능 PR 제목: `Merge: #<Issue 번호> 작업명`, 릴리스: `Release: 릴리스 내용`.
- PR 본문은 [pr.md](../templates/pr.md)를 사용해 문제·변경·검증·미검증·위험을 기술한다.
- 병합 방식은 Create a merge commit이다. Squash·Rebase merge로 변경하지 않는다. PR 생성 승인은 Merge 승인이 아니다. Merge 수행은 명시적으로 승인된 경우만 진행한다.
- 강제 Push, `reset --hard`, `clean`, 임의 restore·stash·브랜치 삭제를 하지 않는다. 승인 없는 merge/rebase를 하지 않는다. 사용자 변경과 `.meta`/GUID를 보존한다.
- 캐시·빌드 산출물·비밀 정보를 커밋하지 않는다. CI 성공은 실제 실행 결과로 확인한다.
- WebGL 운영 배포 기준은 `prod`, 향후 Actions 자동화 예정이다. Windows는 개발자가 수동 빌드한다. 정책만으로 빌드·배포 자동화를 생성하거나 실행하지 않는다.
