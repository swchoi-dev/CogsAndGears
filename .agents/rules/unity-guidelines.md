# Unity 읽기 전용 분석 지침

프로젝트 기준은 [AGENTS.md](../../AGENTS.md)를 따른다. 이 문서는 구현 권한을 부여하지 않는다.

- 현재 버전과 패키지는 `CogsandGears/ProjectSettings/ProjectVersion.txt`, `CogsandGears/Packages/manifest.json`을 읽어 확인한다. 업그레이드하지 않는다.
- 실제 콘텐츠 구조와 Scene·Prefab 직렬화 참조를 읽기 전용으로 확인한다. 파일 저장을 유발하는 Editor 실행·임포트·빌드·테스트·MCP·CLI 조작을 하지 않는다.
- 구현 가이드는 기존 콘텐츠 루트와 구조를 따른다. `.meta`·GUID, 직렬화 필드, Material·Sprite 연결을 보존하도록 안내한다.
- Physics2D·Input System·URP의 실제 사용 설정을 확인하고, 정적 파일 존재와 Play Mode 동작을 구별한다.
- 3D 캐릭터·2D 배경, Pivot·Sorting Layer·애니메이션·플랫폼 압축·WebGL 성능 영향을 필요한 범위만 검토한다.
- Addressables·새 렌더 파이프라인·패키지·빌드 자동화 도입을 임의로 추가하지 않는다.
- Console·컴파일·Play Mode·대상 플랫폼 빌드 결과는 개발자가 제공한 근거를 기록한다. 확인하지 못한 것은 미검증이다.
