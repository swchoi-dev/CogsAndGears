---
name: start-issue
description: GitHub Issue를 조회하거나 작업 요청의 중복 Issue를 조사하고, 개발자용 구현 가이드와 Issue 초안을 준비할 때 사용한다. Unity 구현이나 자동 브랜치 생성은 하지 않는다.
---

# Issue 기반 작업 시작

[역할 제한](../../rules/cg-role-boundary.md), [Issue 절차](../../rules/github-issue-workflow.md), [작성 규칙](../../rules/github-issue-template.md)을 읽는다.

1. 대화에서 Issue 번호 또는 작업 요청을 확인한다. 번호가 있으면 `gh issue view <번호> --repo swchoi-dev/CogsAndGears --json number,title,body,state,labels,url`로 조회한다. 없으면 `gh issue list --repo swchoi-dev/CogsAndGears --state all --search <키워드>`로 중복을 찾는다. 여러 후보의 대상만 확인한다.
2. 관련 문서·상위 Issue·코드·Scene·Prefab을 읽기 전용 조사한다. 현재 Git 상태와 기준 브랜치 존재를 확인한다. 조회 실패는 알리고 추정 Issue를 만들지 않는다.
3. [구현 가이드](../../templates/implementation-brief.md)를 작성한다. 범위·예상 파일·컴포넌트와 이벤트 설계·완료 조건·학습 키워드·위험·브랜치명 제안·Issue 반영 예정 내용을 포함한다. 분할이 필요하면 하위 작업 초안을 준비한다.
4. 신규 Issue는 [하위](../../templates/issue-task.md) 또는 [상위](../../templates/issue-epic.md) 초안을 사용한다. 외부 쓰기 금지나 계획만 요청된 작업이면 초안 결과로 마친다.
5. 반영이 승인된 경우만 생성·수정하고 재조회한다. 본문은 임시 파일에 실제 줄바꿈으로 저장해 `--body-file`로 전달한다. 기존 승인 범위를 넘어 라벨·연결·브랜치 작업을 추가하지 않는다.

실제 확인한 Issue 번호·URL, 조사 근거, 가이드, 반영 여부와 남은 질문을 보고한다. 게임 구현과 Git 쓰기를 이 스킬의 부수 작업으로 실행하지 않는다.
