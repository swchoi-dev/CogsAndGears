---
name: write-tech-doc
description: Cogs & Gears의 docs 아래 기술·설계 문서를 작성하거나 갱신하고 관련 기술 학습 키워드를 정리할 때 사용한다. 게임 코드 수정과 GitHub 게시를 포함하지 않는다.
---

# 기술문서 관리

[역할 제한](../../rules/cg-role-boundary.md)과 [행동 기준](../../rules/behavior-guidelines.md)을 읽는다.

1. 요청한 주제·대상 경로와 기존 관련 문서를 확인한다. `docs/design/`는 설계, `docs/tech/`는 기술문서, `docs/learning/keywords.md`는 학습 키워드, `docs/roadmap.md`는 로드맵이다. 실제 폴더 존재를 확인한다.
2. 관련 Issue와 개발자 검증 결과, 코드·데이터 흐름을 읽기 전용 조사한다. 자료 부재·충돌은 명시한다. 정적 확인만으로 구현 완료·동작 성공을 확정하지 않는다.
3. 기존 문서 갱신과 신규 작성 중 목적에 맞는 방법을 선택한다. 신규 기술문서는 실제 `docs/tech/` 번호의 최댓값 + 1로 `NN-english-kebab-case.md`, 제목은 `# NN. 한국어 제목`을 사용한다. 기존 번호는 유지한다. 아직 번호가 없으면 `01`부터 시작한다.
4. [tech-doc.md](../../templates/tech-doc.md)를 바탕으로 의도·구성·컴포넌트·이벤트·데이터 흐름·제약·검증 근거를 기록한다. 요청에 포함된 경로·폴더 생성은 진행하며 초안만 요청된 경우 파일로 저장하지 않는다.
5. 관련 학습 키워드 추가가 요청 범위에 있으면 중복 없이 갱신한다. 문서 링크·번호·사실과 미검증 구분을 검증한다.

변경 파일과 검증 한계를 보고한다. 문서 작성 요청은 Commit·Push·Issue 생성·댓글·close 승인이 아니다. 외부 연결이 요청되면 [Issue 절차](../../rules/github-issue-workflow.md)에 따라 최종 내용을 준비한다.
