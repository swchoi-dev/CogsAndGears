# 기획·구현·GitHub 조사 기록

기준일: 2026-11-10. Editor·게임 테스트·빌드 실행 없이 파일과 GitHub 조회만 수행했다. 현재 사용자 지시가 과거 기획과 일반 Harness 정책보다 우선한다.

## 자료와 판본

- PDF: `D:\Cogs&Gears\docs\미션가이드 - 개인_백팩 빌딩 런앤건.pdf`, 9페이지, 1,283,330바이트. 전체 텍스트와 페이지 이미지·흐름도·격자 도해를 모두 확인했다.
- SHA-256: `ddc167bb4a978f2f485073490e3deddf07c93c82ef6ffb95d8d5b1d3634c4bfd`.
- 본문에서 언급한 `(2).pdf` 대신 마지막에 실제 제공한 경로의 파일을 분석했다. 다른 판본과 동일성은 미검증이다.
- 읽은 지침: `AGENTS.md`, Rules 7개, start-issue/write-tech-doc 스킬, 관련 템플릿.
- 저장소 기획: `README.md`, `docs/handover/claude-to-codex.md`.
- PDF와 같은 외부 폴더에서 확인한 기존 자료: `Cogs & Gears 기획서.md`(v0.2), `CogsAndGears_PROJECT_SPEC.md`(v1.0), `CogsAndGears_README.md`, `Cogs_and_Gears_개발_이슈_목록_v1.2.md`, `백팩_빌딩_런앤건_개발_이슈_목록.md`.
- 외부 자료는 수정·복사하지 않았다. Git 루트의 `docs/PROJECT_SPEC.md`는 없으며 README의 해당 링크는 현재 깨져 있다. 이번 범위에서 README는 수정하지 않는다.
- 실제 엔진 `6000.3.9f1`, URP `17.3.0`, Input System `1.18.0`, uGUI `2.0.0`. Animation Rigging은 manifest에 없으며 설치하지 않았다.

## 현재 구현

완료 표기는 정적 파일·설정 확인 범위다. 동작 성공은 개발자 확인이 필요하다.

| 영역 | 상태 | 근거·계획 처리 |
|---|---|---|
| Unity/URP 기반·폴더·패키지 | 완료(정적) | 재구축 Issue를 만들지 않음 |
| Title·GameMain | 완료(파일 존재) | End 없음. 기존 Scene 재생성 대신 연결 작업 T28 |
| Cogs 물리·바닥·Orthographic | 완료(설정), 동작 확인 불가 | Rigidbody2D·CapsuleCollider2D·GroundCollision·카메라 존재. 바닥 안착 성공은 과거 개발자 보고와 구별 |
| Cogs/Gears 모델·배경 | 부분 구현 | FBX2·PNG8·Material, GameMain 모델 원본 PrefabInstance 참조. 최종 허용 여부 T39 |
| 배경 Parallax | 부분 구현 | BG_Far SpriteRenderer1, BG_Mid/BG_Near는 오브젝트만. 동작 스크립트 없음 |
| SceneReference 도구 | 완료(코드), 동작 확인 불가 | SceneReference·SceneReferenceSample·Editor/SceneReferenceDrawer가 전체 사용자 C# 3개 |
| 이동·대시·조준·Facing | 미구현 | 해당 사용자 기능 스크립트 없음 |
| Idle/Walk | 확인 불가 | 외부 .anim/.controller·Scene 직접 Animator 없음. FBX 내장 클립/하위 모델 상태는 별도 확인 필요 |
| 전투·실드·적·풀·층 SO | 미구현 | 해당 스크립트·콘텐츠 SO 없음 |
| ItemSO·가방·시너지·확장·보상 | 미구현 | 해당 스크립트·SO·UI 없음 |
| Run·Goal·결과·재시작 | 미구현 | Scene 참조 샘플만 있고 진행 제어 없음 |
| HUD·버튼·옵션 | 미구현 | GameMain의 UI는 빈 Transform. Canvas/RectTransform 없음. Title은 카메라·조명·볼륨 |
| 빌드 Scene 목록 | 부분 구현 | Title만 등록. 개발자가 GameMain/End 연결·검증 필요 |
| 사용자 Prefab·InputAction 파일 | 파일 부재 | Assets의 .prefab/.inputactions/.controller/.anim 각각0. 패키지·FBX 내부 리소스까지 없다는 뜻은 아님 |
| 컴파일·Play Mode·빌드 | 확인 불가 | 이번 요청에서 실행 금지, 성공으로 기록하지 않음 |

## 기획 충돌

- 과거 v1.2의 점프·2D 스프라이트·성 허브·매 전투 귀환은 현재 2.5D/점프 없음/Title→GameMain→End 요청으로 대체한다. 새 허브·세이브·상점은 확정하지 않는다.
- 과거 선택적 홀드·풀링은 PDF/현재 요청에서 필수다. 가방·UI의 과거 P1/P2 표기를 P0 필수로 재정렬한다.
- 합성·Gears·방향/범위 시너지는 후순위다. 층수·중첩·수치는 미정이고 3층은 검토 후보다.
- PDF p.9는 무료 Asset Store/학원 제공 에셋을 안내한다. Meshy 자산의 허용을 담당자 확인 없이 추정하지 않는다. 공개 저장소에 학원 에셋 원본을 올리지 않는다.

## Git·GitHub 조회 결과

- 시작 작업 트리 깨끗함. 현재 `prod`, HEAD `2cd7aa9` (`[chore] 프로젝트 관리 하네스 Codex 전환`). 과거 인계의 main 단독 상태를 재사용하지 않았다.
- 로컬 prod, 원격 추적 및 GitHub 실제 브랜치 develop/prod. 기본 브랜치 prod, 공개 저장소 `swchoi-dev/CogsAndGears`.
- 전체 Issues(열림/닫힘)0, Milestones(열림/닫힘)0. 기존 기본 Labels10개. 정확한 목록은 [생성 계획](github-issues-plan.md)에 기록했다.
- CLI는 PATH에 없어 `C:\Program Files\GitHub CLI\gh.exe`로 조회했다. GitHub 쓰기·Git 변경은 수행하지 않았다.
- 현재 요청에 따라 실제 브랜치·Commit·Push·PR·Merge는 개발자가 수행하며 Codex는 제안만 한다. develop 병합을 기본 브랜치 병합/자동 close와 혼동하지 않는다.

## 산출물·검증 범위

[로드맵](../roadmap.md), [추적표](mission-requirements.md), [Issue 전체 초안](github-issues-plan.md), 이 조사 기록의 문서4개만 생성한다. 기존 파일 SHA-256·새 문서 링크·요구사항/Issue ID·선행 관계를 검사한다. PDF 도구·추출 페이지·시작 해시는 임시 폴더에만 저장했다. 원본 PDF·외부 기획·게임 파일·전역 설정은 수정하지 않는다.

### 문서 검증 결과

- 신규 문서4개 외 작업 트리 변경 없음. 시작 기준 기존 파일123개의 SHA-256 동일, 그중 Unity 파일102개 동일. 브랜치 prod와 HEAD 2cd7aa9 유지.
- 상위 E01~E11 11개, 하위 T01~T45 45개의 식별자·상위 연결·본문 필수 항목 확인. 선행 참조 대상 모두 존재하고 순환 없음.
- 추적 R01~R65 65행 모두 존재하는 하위 ID에 연결. 로컬 Markdown 링크 유효, 인코딩·공백·git diff --check 오류 없음.
- 요구사항 반영의 검증이며 게임 구현 완료·동작·컴파일·빌드 검증은 아니다. GitHub 생성/수정·Label/Milestone 변경·Git 쓰기 없음.
- 가용시간·Meshy 에셋 허용·3층 MVP 질문은 문서 작성 시 답변 미확정이다. 질문의 기본 선택이나 경과 시간을 승인으로 간주하지 않는다.
