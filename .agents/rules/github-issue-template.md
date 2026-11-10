# Issue 작성 규칙

초안의 실제 형식은 다음 템플릿을 사용한다.
- 하위 작업: [issue-task.md](../templates/issue-task.md)
- 상위 시스템: [issue-epic.md](../templates/issue-epic.md)
- 검증 댓글: [verification.md](../templates/verification.md)

제목은 짧은 기능 단위로 작성하고 Issue 번호·브랜치명을 넣지 않는다. 배경과 목적은 대화·관련 자료의 사실을 사용하며 모르면 확인한다. 경로는 실제 확인하거나 `(예상)`을 붙인다. 참고 링크는 존재를 확인한 문서·Issue만 쓴다.

완료 조건은 관찰·검증 가능해야 한다. Unity 구현 작업은 개발자의 컴파일·Console·Play Mode 확인을 포함한다. 순수 문서·Harness 작업에 Unity 검증 항목을 기계적으로 추가하지 않는다. 미검증 항목은 체크하지 않는다. 기술 학습 키워드는 관련 API·개념 중심으로 작성한다.

기존 본문은 보존하고 필요한 부분만 보완한다. 게시와 수정은 [github-issue-workflow.md](github-issue-workflow.md)의 승인 범위를 따른다.
