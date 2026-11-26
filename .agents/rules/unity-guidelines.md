# Unity 구현·Editor 작업 지침

프로젝트 기준은 [AGENTS.md](../../AGENTS.md), 쓰기 범위는 [역할과 쓰기 범위](cg-role-boundary.md)를 따른다.

- 현재 버전과 패키지는 `CogsandGears/ProjectSettings/ProjectVersion.txt`, `CogsandGears/Packages/manifest.json`을 읽어 확인한다. 승인 없이 업그레이드하지 않는다.
- 변경 전 실제 콘텐츠 구조와 Scene·Prefab 직렬화 참조를 확인한다. 기존 콘텐츠 루트와 구조를 따른다.
- 에셋 변경은 Unity MCP를 우선 사용하고, MCP가 지원하지 않는 작업은 Editor API 또는 Editor Script로 수행한다. Prefab·Animator Controller·`.meta` YAML 직접 수정은 최후의 수단이며 GUID·fileID를 추정하지 않는다.
- `.meta`·GUID, 직렬화 필드, Material·Sprite 연결과 외부 참조를 보존한다. 일회성 Editor Script는 작업 후 제거하고 커밋하지 않는다.
- Physics2D·Input System·URP의 실제 사용 설정을 확인하고, 정적 파일 존재와 Play Mode 동작을 구별한다.
- 3D 캐릭터·2D 배경, Pivot·Sorting Layer·애니메이션·플랫폼 압축·WebGL 성능 영향을 필요한 범위만 검토한다.
- Addressables·새 렌더 파이프라인·패키지·빌드 자동화 도입을 임의로 추가하지 않는다.
- 변경 후 컴파일·Console 오류와 Missing Reference를 확인하고, 가능한 범위에서 Play Mode로 동작을 검증한다. 직접 실행한 검증과 개발자가 제공한 근거를 구분해 기록하며 확인하지 못한 것은 미검증이다.
