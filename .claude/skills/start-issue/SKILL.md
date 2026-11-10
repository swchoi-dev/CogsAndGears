---
name: start-issue
description: 개발 작업을 시작할 때 GitHub Issue(#번호)를 조회하거나 기존 Issue 검색·신규 생성을 확인하고, 관련 문서와 코드를 읽기 전용으로 조사해 개발자용 구현 가이드와 브랜치명을 브리핑할 때 사용한다. 코드 구현과 브랜치 생성은 하지 않는다.
argument-hint: "12"
disable-model-invocation: false
---

# GitHub Issue 기반 작업 시작

> `github-issue-workflow.md` 4.1~4.3을 실행하고 개발자의 구현(4.4)으로 넘기는 절차다. 승인 시점과 정책은 해당 규칙을 따른다. 코드 작성, 에셋 수정, Git 쓰기는 하지 않는다(`cg-role-boundary.md`).

## 입력
- Issue 번호(`12`, `#12`) 또는 작업 요청 내용을 받는다.
- `${ARGUMENTS}`가 있으면 Issue 번호를 추출한다.

## 1. Issue 확인
1. 번호가 있으면 `gh issue view <번호> --json number,title,body,state,labels,url`로 조회한다. 실패하면 멈추고 알린다.
2. 번호가 없으면 `gh issue list --state all --search "<키워드>"`로 기존 Issue를 검색해 후보를 제시하고 대상을 확인받는다.
3. 관련 Issue가 없으면 신규 생성 여부를 묻는다. 생성하기로 하면 `github-issue-template.md`로 초안을 작성하고, 승인 후에만 생성한다.

## 2. 조사 (읽기 전용)
1. 관련 기획·설계 문서(`docs/`)와 상위 Issue를 확인한다. 문서가 없거나 서로 충돌하면 알린다.
2. `CogsandGears/Assets/CogsAndGears/` 아래 관련 코드·Scene·Prefab을 읽기 전용으로 확인한다. Unity Editor를 실행하거나 파일을 저장하지 않는다.
3. `git status`, 현재 브랜치, 원격 `develop` 존재 여부를 확인한다. 미커밋 변경은 보고만 한다.

## 3. 구현 가이드 브리핑
`github-issue-workflow.md` 4.2 항목으로 작성한다.

```text
## 구현 가이드: #<번호> <제목>

**작업 범위**
- 포함 / 제외

**변경 예상 파일** (개발자가 직접 작성·수정)
- 생성: 경로 (역할)
- 수정: 경로 (변경 내용)

**설계 방향**
- (컴포넌트 구성, 이벤트 흐름, 직렬화 필드 등. 코드는 작성하지 않는다)

**완료 조건 / 검증 방법**
- [ ] 조건 → Play Mode 확인 방법

**기술 학습 키워드**
- 키워드

**의존성·위험**
- 내용

**브랜치명 제안**
- feature/<번호>-<기능명> (develop에서 분기)

**Issue 반영 예정 내용**
- (본문 보완 항목, 라벨, Sub-issue 연결 등. 없으면 "없음")
```
- 범위가 크면 하위 Issue 분할을 제안한다.
- 계획 수립만 요청받았다면 브리핑을 제시하고 멈춘다.
- **승인 전에는 Issue 쓰기를 하지 않는다.**

## 4. Issue 반영
1. 승인받은 항목만 반영한다(`gh issue edit <번호> --body-file <파일>` 등). 본문 파일은 스크래치 경로에 만든다.
2. `gh issue view`로 다시 조회해 실제 반영 결과를 보고한다.

## 마무리
- Issue 번호·제목·URL, 반영 내용, 조사한 문서, 브랜치명 제안, 남은 질문을 보고한다.
- 브랜치 생성과 구현은 개발자가 수행한다고 안내한다.
