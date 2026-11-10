---
name: record-verification
description: 개발자가 직접 구현한 기능의 결과와 검증 내용을 해당 GitHub Issue에 댓글로 기록하고, 확인된 완료 조건만 체크하며, 완료 처리 가능 여부를 판단할 때 사용한다.
argument-hint: "12"
disable-model-invocation: false
---

# 구현 결과 및 검증 기록

> `github-issue-workflow.md` 4.5~4.6을 실행하는 절차다. 댓글 형식은 `github-issue-template.md` 5장을 따른다. 모든 Issue 쓰기는 승인 후 수행한다.

## 1. 대상 확인
1. `${ARGUMENTS}` 또는 대화에서 Issue 번호를 확인한다. 없으면 현재 브랜치명(`feature/<번호>-...`)에서 추출하고, 맞는지 사용자에게 확인한다.
2. `gh issue view <번호> --comments`로 본문, 완료 조건, 기존 검증 기록을 조회한다.

## 2. 근거 수집
- **개발자 확인:** 개발자가 전달한 Play Mode 동작, Console 로그, 빌드 결과. 전달받지 않은 항목은 질문한다.
- **읽기 전용 확인:** 관련 브랜치·커밋·diff(`git log`, `git diff develop...<브랜치>`), 관련 PR 상태(`gh pr list --search "<번호>" --state all`).
- 코드를 읽고 판단한 내용은 "코드 읽기 전용 확인"으로 표기하고 동작 검증으로 간주하지 않는다.
- Unity Editor를 실행하거나 Scene을 열어 저장하지 않는다.

## 3. 초안 작성 및 승인
1. 검증 기록 댓글 초안을 작성한다(`github-issue-template.md` 5장).
2. 완료 조건별로 체크할 항목과 그대로 둘 항목을 구분해 제시한다. **실제로 확인된 항목만 체크한다.**
3. 댓글 추가와 본문 체크박스 변경을 승인받는다.

## 4. 반영
1. 댓글: `gh issue comment <번호> --body-file <파일>` (본문 파일은 스크래치 경로에 만든다)
2. 체크박스: 기존 본문을 받아 해당 줄만 `[ ]` → `[x]`로 바꾼 뒤 `gh issue edit <번호> --body-file <파일>`. 다른 내용은 바꾸지 않는다.
3. `gh issue view <번호> --comments`로 다시 조회해 반영 결과를 보고한다.

## 5. 완료 처리 판단
- `github-issue-workflow.md` 4.6 체크리스트를 확인해 충족 여부를 보고한다.
- 모두 충족하면 close 여부를 묻고, 승인받은 경우에만 `gh issue close <번호> --reason completed`를 실행한다.
- 하위 Issue를 닫은 뒤 상위 Issue의 남은 하위 작업을 함께 보고한다.
- 기술문서 반영이 필요해 보이면 `write-tech-doc` 진행 여부를 묻는다.
