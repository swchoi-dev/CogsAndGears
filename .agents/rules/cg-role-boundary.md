# 에이전트 역할과 쓰기 범위

- 개발자: 최종 설계 결정, 작업 방향 설정, 결과 검토, Merge 승인을 담당한다.
- 에이전트: 기획·설계·기술문서·로드맵·Issue 관리와 함께 Unity 실제 구현과 Editor 설정까지 수행한다.
- 에이전트가 직접 수행할 수 있는 Unity 작업:
  - C# 스크립트 생성·수정, ScriptableObject 생성·수정
  - Scene·GameObject·컴포넌트·Prefab 생성·수정, Inspector 필드와 SerializeField 참조 연결
  - FBX 임포트와 Model Import Settings, Animator Controller, Animation Clip 설정·연결, Material·Texture 설정
  - Unity MCP 또는 Editor API·Editor Script를 이용한 Editor 조작, Play Mode 실행·검증, Console 오류 확인·수정
- Unity 에셋 변경은 Editor(MCP·Editor API)를 통해 수행한다. Prefab·Animator Controller·`.meta` 등 YAML 직접 수정은 최후의 수단이며 GUID·fileID를 추정하지 않는다. 기존 `.meta`·GUID와 외부 참조를 보존한다.
- Packages·ProjectSettings·빌드 설정 변경과 패키지 추가·업그레이드는 작업에 필요한 범위를 밝히고 승인된 경우만 수행한다.
- 쓰기 가능 문서 위치는 `AGENTS.md`, `.agents/`, `docs/`, 요청된 `README.md`다. 사용자 전역 에이전트 설정·다른 프로젝트 설정·원본 Harness 저장소는 별도 요청 없이 변경하지 않는다.
- Git 브랜치·Commit·Push·PR 관리는 [git-workflow](git-workflow.md)에 따라 사용자가 승인한 대상과 범위에서 수행한다. Merge는 명시적 승인이 있는 작업만 수행한다. 현재 작업에서 금지된 작업은 실행하지 않는다.
- GitHub Issue 생성·수정·댓글·라벨·마일스톤·Sub-issue·상태 변경도 승인된 내용만 반영한다.
- 도구·셸·인터프리터를 바꿔 금지 범위를 우회하지 않는다. 사용자 변경을 삭제·덮어쓰기·stage·stash하지 않는다.
- 승인된 작업의 일반적인 구현·설정 단계마다 승인을 반복 요청하지 않는다. 기존 작업 손실 위험이나 도구 접근 권한처럼 에이전트가 해결할 수 없는 문제만 확인한다.

이 문서는 지침이다. 파일 쓰기 차단이나 플러그인 비활성화가 기술적으로 강제된다고 간주하지 않는다.
