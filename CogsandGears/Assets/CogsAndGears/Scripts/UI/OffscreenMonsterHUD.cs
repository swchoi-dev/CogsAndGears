using System.Collections.Generic;
using UnityEngine;

// 카메라 화면 밖 몬스터의 방향·거리 경고.
// 판정: WorldToViewportPoint가 [0,1]×[0,1] 밖이거나 카메라 뒤(z<0)이면 화면 밖.
// 위치: 화면 중앙 → 대상 방향 벡터를 안전 영역(Safe Area - 여백) 사각형 경계에 투영한다(좌우·상하·대각 공통).
// 거리: 플레이어-몬스터 XY 평면 거리(월드 1 = 1m). 가까운 순으로 최대 _maxMarkers개, 겹치면 경계를 따라 밀어낸다.
public class OffscreenMonsterHUD : MonoBehaviour
{
    private struct Target
    {
        public Vector2 Direction;
        public float Distance;
    }

    [SerializeField] private GameFlowManager _gameFlow;
    [SerializeField] private Transform _player;
    [Tooltip("비워두면 Camera.main")]
    [SerializeField] private Camera _camera;
    [Tooltip("화면 전체를 덮는(stretch) 마커 배치 영역")]
    [SerializeField] private RectTransform _markerArea;
    [SerializeField] private OffscreenWarningMarker _markerPrefab;

    [Header("Safe Area 안쪽 여백(캔버스 단위). 상단 맵·하단 HUD와 겹치지 않게 위아래를 크게 둔다")]
    [SerializeField] private float _paddingHorizontal = 120f;
    [SerializeField] private float _paddingTop = 150f;
    [SerializeField] private float _paddingBottom = 190f;
    [SerializeField] private int _maxMarkers = 5;
    [Tooltip("마커 중심 간 최소 간격(캔버스 단위)")]
    [SerializeField] private float _minSpacing = 56f;

    private readonly List<OffscreenWarningMarker> _markers = new List<OffscreenWarningMarker>();
    private readonly List<Target> _targets = new List<Target>();
    private readonly List<Vector2> _placed = new List<Vector2>();

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;
    }

    private void OnDisable()
    {
        SetMarkerCount(0);
    }

    private void LateUpdate()
    {
        CollectTargets();
        PlaceMarkers();
    }

    private void CollectTargets()
    {
        _targets.Clear();

        StageController stage = _gameFlow != null ? _gameFlow.CurrentStage : null;
        if (stage == null || _camera == null || _player == null) return;

        Vector2 playerPosition = _player.position;
        IReadOnlyList<MonsterController> monsters = stage.AliveMonsters;

        for (int i = 0; i < monsters.Count; i++)
        {
            if (monsters[i] == null || !monsters[i].isActiveAndEnabled) continue;

            Vector3 worldPosition = monsters[i].transform.position;
            if (!TryGetOffscreenDirection(_camera.WorldToViewportPoint(worldPosition), out Vector2 direction)) continue;

            _targets.Add(new Target
            {
                Direction = direction,
                Distance = Vector2.Distance(playerPosition, worldPosition),
            });
        }

        _targets.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        if (_targets.Count > _maxMarkers)
            _targets.RemoveRange(_maxMarkers, _targets.Count - _maxMarkers);
    }

    // 화면 안이면 false. 화면 밖이면 화면 중앙 기준 방향(viewport 단위)을 반환한다.
    private static bool TryGetOffscreenDirection(Vector3 viewport, out Vector2 direction)
    {
        bool behind = viewport.z < 0f;
        bool inside = !behind && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;

        direction = new Vector2(viewport.x - 0.5f, viewport.y - 0.5f);

        if (inside) return false;

        // 카메라 뒤쪽은 투영이 뒤집히므로 방향을 반전한다
        if (behind) direction = -direction;

        if (direction.sqrMagnitude < Mathf.Epsilon) direction = Vector2.down;
        return true;
    }

    private void PlaceMarkers()
    {
        SetMarkerCount(_targets.Count);
        if (_targets.Count == 0) return;

        Rect safe = GetSafeRect();
        Vector2 center = _markerArea.rect.center;
        Vector2 areaSize = _markerArea.rect.size;
        _placed.Clear();

        for (int i = 0; i < _targets.Count; i++)
        {
            // viewport 방향을 캔버스 단위로 바꿔 화면 비율을 반영한다
            Vector2 direction = Vector2.Scale(_targets[i].Direction, areaSize);
            Vector2 position = ProjectToRect(center, direction, safe);
            position = AvoidOverlap(position, safe);

            _placed.Add(position);
            _markers[i].Set(position, direction, _targets[i].Distance);
        }
    }

    // center에서 direction으로 뻗은 반직선이 rect 경계와 만나는 점
    private static Vector2 ProjectToRect(Vector2 center, Vector2 direction, Rect rect)
    {
        float tX = direction.x > 0f ? (rect.xMax - center.x) / direction.x
                 : direction.x < 0f ? (rect.xMin - center.x) / direction.x
                 : float.PositiveInfinity;
        float tY = direction.y > 0f ? (rect.yMax - center.y) / direction.y
                 : direction.y < 0f ? (rect.yMin - center.y) / direction.y
                 : float.PositiveInfinity;

        return center + direction * Mathf.Max(0f, Mathf.Min(tX, tY));
    }

    // 이미 놓인 마커와 겹치면 경계를 따라(좌우 경계면 세로, 상하 경계면 가로) 밀어낸다
    private Vector2 AvoidOverlap(Vector2 position, Rect safe)
    {
        bool onSideEdge = Mathf.Approximately(position.x, safe.xMin) || Mathf.Approximately(position.x, safe.xMax);
        Vector2 step = onSideEdge ? new Vector2(0f, -_minSpacing) : new Vector2(_minSpacing, 0f);

        for (int attempt = 0; attempt < _maxMarkers; attempt++)
        {
            if (!OverlapsPlaced(position)) break;

            position += step;
            position.x = Mathf.Clamp(position.x, safe.xMin, safe.xMax);
            position.y = Mathf.Clamp(position.y, safe.yMin, safe.yMax);
        }

        return position;
    }

    private bool OverlapsPlaced(Vector2 position)
    {
        for (int i = 0; i < _placed.Count; i++)
        {
            if ((_placed[i] - position).sqrMagnitude < _minSpacing * _minSpacing)
                return true;
        }

        return false;
    }

    // Screen.safeArea를 마커 영역 로컬 좌표로 바꾸고 여백만큼 줄인다
    private Rect GetSafeRect()
    {
        Rect area = _markerArea.rect;
        Rect safeArea = Screen.safeArea;
        float screenWidth = Mathf.Max(1f, Screen.width);
        float screenHeight = Mathf.Max(1f, Screen.height);

        float xMin = area.xMin + safeArea.xMin / screenWidth * area.width + _paddingHorizontal;
        float xMax = area.xMin + safeArea.xMax / screenWidth * area.width - _paddingHorizontal;
        float yMin = area.yMin + safeArea.yMin / screenHeight * area.height + _paddingBottom;
        float yMax = area.yMin + safeArea.yMax / screenHeight * area.height - _paddingTop;

        if (xMax < xMin) xMin = xMax = area.center.x;
        if (yMax < yMin) yMin = yMax = area.center.y;

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private void SetMarkerCount(int count)
    {
        while (_markers.Count < count)
            _markers.Add(Instantiate(_markerPrefab, _markerArea));

        for (int i = 0; i < _markers.Count; i++)
        {
            bool active = i < count;
            if (_markers[i].gameObject.activeSelf != active)
                _markers[i].gameObject.SetActive(active);
        }
    }
}
