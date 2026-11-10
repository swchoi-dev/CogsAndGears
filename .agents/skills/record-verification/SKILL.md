---
name: record-verification
description: 개발자가 구현한 기능의 검증 근거를 정리해 GitHub Issue 댓글과 완료 조건 수정 초안을 만들고 완료 처리 가능 여부를 판단할 때 사용한다.
---

# 검증 기록과 완료 판단

[역할 제한](../../rules/cg-role-boundary.md)과 [Issue 절차](../../rules/github-issue-workflow.md)를 읽는다.

1. 대화의 Issue 번호를 확인한다. 브랜치명에서 추출한 번호는 후보이며 대상이 맞는지 확인한다. `gh issue view <번호> --repo swchoi-dev/CogsAndGears --comments`로 실제 요구사항과 기록을 조회한다.
2. 개발자 Play Mode·Console·빌드 보고와 로컬 diff·커밋·실제 PR 상태를 수집한다. 코드 읽기 확인은 동작 검증으로 계산하지 않는다. Unity를 실행하지 않는다. 근거가 없으면 미검증으로 남긴다.
3. [verification.md](../../templates/verification.md)로 댓글 초안을 작성하고 체크할 완료 조건의 변경분을 준비한다. 원문은 보존한다.
4. 쓰기가 허용되고 해당 내용이 승인된 경우만 댓글·본문 변경을 수행한다. `gh issue comment/edit <번호> --repo swchoi-dev/CogsAndGears --body-file <임시파일>`을 사용하고 재조회한다. 쓰기가 금지되면 초안만 제공한다.
5. 완료 조건·개발자 검수·관련 PR 병합·문서 필요 여부를 점검한다. close는 명시적으로 승인된 범위에 포함되고 조건이 충족된 경우만 수행한다. 댓글 승인만으로 close하지 않는다.

검증된 항목, 미검증 항목, 실제 반영 내용, close 가능 여부와 남은 하위 작업을 보고한다. 기술문서 갱신이 필요하면 그 내용을 제안하되 요청 범위를 임의로 확대하지 않는다.
