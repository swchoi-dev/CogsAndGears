# GitHub Issue 워크플로우

> Cogs & Gears의 작업은 GitHub Issues로 관리한다. 이 문서는 단계와 승인 시점을 정의한다. 본문 형식은 `github-issue-template.md`, Git 규칙은 `git-workflow.md`, 역할 제한은 `cg-role-boundary.md`를 따른다.
## 1. 기본 정책
- 도구는 GitHub CLI(`gh`)를 사용한다. 대상 저장소는 `swchoi-dev/CogsAndGears`다.
- **사용자 승인 없이 Issue 생성, 본문 수정, 댓글, 라벨·마일스톤·Sub-issue 연결, close/reopen을 하지 않는다.**
- 쓰기 전에 반영할 내용(초안 전체 또는 변경 diff)을 보여주고 승인받는다. 쓰기 후에는 `gh issue view`로 다시 조회해 실제 반영 결과를 보고한다.
- 도구가 반환한 실제 Issue 번호·URL·상태만 보고한다.
- 조회 실패 시 Issue 내용을 추측하지 않고 멈춘 뒤 알린다.

## 2. Issue 계층
| 구분 | 단위 | 라벨 | 예 |
|---|---|---|---|
| 상위 Issue | 시스템 또는 기능 단위 | `type:epic` | Cogs 조작, 연금술 가방 인벤토리 |
| 하위 Issue | 독립적으로 개발·검증 가능한 단위 | `type:task` / `type:bug` / `type:docs` | Cogs 좌우 이동, 마우스 360° 조준 |

- 하위 Issue는 GitHub Sub-issues로 상위 Issue에 연결한다.
  ```bash
  # 하위 Issue의 내부 id 조회 후 상위 Issue에 연결
  gh api repos/swchoi-dev/CogsAndGears/issues/<하위번호> --jq .id
  gh api -X POST repos/swchoi-dev/CogsAndGears/issues/<상위번호>/sub_issues -F sub_issue_id=<id>
  ```
- 하위 Issue 하나는 기능 브랜치 하나(`feature/<번호>-<기능명>`)로 개발할 수 있는 크기로 나눈다.
- 영역 라벨 `area:*`(예: `area:player`, `area:camera`, `area:combat`, `area:inventory`, `area:stage`, `area:gears`, `area:visual`)을 함께 붙인다. 라벨이 저장소에 없으면 생성 전에 승인받는다.

## 3. 입력 유형 분류
| 유형 | 판단 기준 | Issue | 원본 MD 저장 |
|---|---|---|---|
| A. 일반 대화 | 질문, 설명, 설계 의견, 읽기 전용 분석 | X | X |
| B. 작업 요청 | 기능 개발 계획, 문서 작성, 버그 기록 등 | O (승인 후) | X |
| C. 정식 작업지시서 | 제목·배경·범위·완료 조건 등을 갖춘 독립 Markdown 문서 전체 | O (승인 후) | O |

- 유형이 불명확하면 사용자에게 확인한다.
- B를 정식 작업지시서로 변환해 보관하지 않는다.
- C의 원본은 `docs/work-orders/issue-{번호}-{작업명}.md`에 수정 없이 저장한다. 커밋·Push는 개발자가 수행하며, Push가 확인된 뒤에만 Issue 본문에 링크를 추가한다.

## 4. 단계별 절차
| 단계 | 수행자 | 진행 조건 |
|---|---|---|
| 1. Issue 확인·생성 | Claude | 신규 생성 시 초안 승인 |
| 2. 구현 가이드 브리핑 | Claude | 브리핑 확인 |
| 3. Issue 반영 | Claude | 반영 내용 승인 |
| 4. 구현 | **개발자** | — |
| 5. 검증 결과 기록 | Claude | 기록 내용 승인 |
| 6. 완료 처리 | Claude | 완료 조건 충족 + close 승인 |

### 4.1 Issue 확인·생성
1. Issue 번호가 있으면 `gh issue view <번호>`로 조회한다.
2. 없으면 키워드로 기존 Issue를 검색한다(`gh issue list --state all --search "<키워드>"`). 닫힌 Issue도 포함한다.
3. 후보가 있으면 제시하고 대상 Issue를 확인받는다. **같은 작업이면 신규 Issue를 만들지 않는다.**
4. 관련 Issue가 없으면 `github-issue-template.md`로 초안을 작성한다. 배경/목적을 확인할 수 없으면 질문하고 생성하지 않는다.
5. 승인 후 생성하고, 실제 번호와 URL을 보고한다.

### 4.2 구현 가이드 브리핑
코드를 작성하지 않고 개발자가 구현할 수 있도록 다음을 정리한다.
- 작업 목적과 범위(포함/제외)
- 변경 예상 파일 (실제 구조를 확인한 경로, 확인 못 한 경로는 `(예상)`)
- 설계 방향 (프로젝트 설계 원칙 준수: 컴포넌트 + C# 이벤트 + if/switch, FSM 미사용)
- 완료 조건과 검증 방법
- 기술 학습 키워드
- 의존성·위험 요소
- 브랜치명 제안 (`git-workflow.md` 3장)
- **Issue에 반영할 내용**

범위가 크면 하위 Issue 분할을 제안하고, 승인되면 4.1로 돌아간다.

### 4.3 Issue 반영
- 브리핑에서 승인받은 항목만 반영한다. 기존 본문을 일괄 재작성하지 않고 누락 섹션 보완과 변경 부분만 수정한다.

### 4.4 구현 (개발자)
- 개발자가 기능 브랜치에서 직접 구현·커밋·PR을 수행한다.
- 범위가 바뀌면 개발자 요청에 따라 Issue 범위 수정 또는 신규 하위 Issue를 제안한다(4.1, 4.3 절차).

### 4.5 검증 결과 기록
`record-verification` 스킬 절차를 따른다.
- 개발자가 전달한 검증 결과와 Claude가 읽기 전용으로 확인한 내용을 구분해 기록한다.
- 완료 조건 체크박스는 실제로 충족·확인된 항목만 체크한다.

### 4.6 완료 처리
다음을 확인한다.
- [ ] 완료 조건 전체 검증 (미검증 항목 없음)
- [ ] 개발자 검수 완료
- [ ] 관련 PR의 `develop` 병합 여부 (`gh pr list --search "<번호>" --state all`로 실제 상태 조회)
- [ ] 기술문서 작성·갱신 완료 또는 불필요 확인 (`write-tech-doc`)

모두 충족하고 사용자가 close를 승인하면 `gh issue close <번호> --reason completed`로 닫는다. 미충족 항목이 있으면 사유를 보고하고 닫지 않는다. **PR 생성만으로 완료 처리하지 않는다.**

## 5. 기존 완료 작업과 예정 작업
- 이 워크플로 도입 전에 완료된 작업을 Issue로 기록할 때는 개발자가 확인한 사실만 쓰고, 본문에 "도입 전 완료 작업"임을 명시한다.
- 구현 여부가 불확실한 작업은 완료로 기록하지 않는다. 개발자에게 확인한 뒤 분류한다.
- 계획 단계의 작업은 열린 Issue로 두고 완료 조건을 체크하지 않는다.
