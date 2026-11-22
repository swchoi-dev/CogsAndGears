using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 상단 스테이지 축소 맵. 플레이어·살아 있는 몬스터의 월드 X를 Stage 범위(CameraBounds)에 정규화해 마커로 표시하고,
// 남은 몬스터 수(살아 있음 + 스폰 예정)를 표시한다. 상태는 StageController가 단일 데이터 소스다.
public class StageProgressHUD : MonoBehaviour
{
    [SerializeField] private GameFlowManager _gameFlow;
    [SerializeField] private Transform _player;

    [Header("Map")]
    [Tooltip("마커를 배치할 막대 영역. 마커 anchor.x를 정규화 위치(0~1)로 설정한다")]
    [SerializeField] private RectTransform _markerArea;
    [SerializeField] private RectTransform _playerMarker;
    [SerializeField] private RectTransform _monsterMarkerPrefab;
    [Tooltip("이 정규화 거리보다 가까운 몬스터 마커는 위로 쌓아 겹침을 줄인다")]
    [SerializeField] private float _overlapDistance = 0.015f;
    [SerializeField] private float _stackOffsetY = 8f;

    [Header("Remaining")]
    [SerializeField] private TMP_Text _remainingText;

    private StageController _stage;
    private readonly List<RectTransform> _monsterMarkers = new List<RectTransform>();
    private readonly List<float> _monsterPositions = new List<float>();
    private int _shownRemaining = -1;

    private void OnEnable()
    {
        if (_gameFlow == null) return;

        _gameFlow.StageStarted += BindStage;
        BindStage(_gameFlow.CurrentStage);
    }

    private void OnDisable()
    {
        if (_gameFlow != null)
            _gameFlow.StageStarted -= BindStage;

        UnbindStage();
    }

    private void LateUpdate()
    {
        if (_stage == null || _stage.CameraBounds == null)
        {
            _playerMarker.gameObject.SetActive(false);
            SetMonsterMarkerCount(0);
            return;
        }

        Bounds bounds = _stage.CameraBounds.bounds;

        _playerMarker.gameObject.SetActive(_player != null);
        if (_player != null)
            SetMarkerX(_playerMarker, Normalize(_player.position.x, bounds), 0f);

        UpdateMonsterMarkers(bounds);
    }

    private void BindStage(StageController stage)
    {
        UnbindStage();

        _stage = stage;
        if (_stage == null) return;

        _stage.RemainingMonsterCountChanged += RefreshRemaining;
        RefreshRemaining();
    }

    private void UnbindStage()
    {
        if (_stage == null) return;

        _stage.RemainingMonsterCountChanged -= RefreshRemaining;
        _stage = null;
    }

    private void RefreshRemaining()
    {
        int remaining = _stage != null ? Mathf.Max(0, _stage.RemainingMonsterCount) : 0;
        if (remaining == _shownRemaining) return;

        _shownRemaining = remaining;
        _remainingText.text = $"Monster × {remaining}";
    }

    private void UpdateMonsterMarkers(Bounds bounds)
    {
        _monsterPositions.Clear();

        IReadOnlyList<MonsterController> monsters = _stage.AliveMonsters;
        for (int i = 0; i < monsters.Count; i++)
        {
            if (monsters[i] == null) continue;

            _monsterPositions.Add(Normalize(monsters[i].transform.position.x, bounds));
        }

        // 정렬 후 가까운 마커는 한 칸씩 위로 쌓는다
        _monsterPositions.Sort();
        SetMonsterMarkerCount(_monsterPositions.Count);

        int stack = 0;
        for (int i = 0; i < _monsterPositions.Count; i++)
        {
            bool overlaps = i > 0 && _monsterPositions[i] - _monsterPositions[i - 1] < _overlapDistance;
            stack = overlaps ? stack + 1 : 0;

            SetMarkerX(_monsterMarkers[i], _monsterPositions[i], stack * _stackOffsetY);
        }
    }

    private void SetMonsterMarkerCount(int count)
    {
        while (_monsterMarkers.Count < count)
            _monsterMarkers.Add(Instantiate(_monsterMarkerPrefab, _markerArea));

        for (int i = 0; i < _monsterMarkers.Count; i++)
        {
            bool active = i < count;
            if (_monsterMarkers[i].gameObject.activeSelf != active)
                _monsterMarkers[i].gameObject.SetActive(active);
        }

        // 플레이어 마커는 항상 몬스터 마커 위에 그린다
        _playerMarker.SetAsLastSibling();
    }

    // Stage 범위 길이가 0 이하이면 중앙(0.5)으로 표시한다
    private static float Normalize(float worldX, Bounds bounds)
    {
        if (bounds.size.x <= Mathf.Epsilon) return 0.5f;

        return Mathf.Clamp01(Mathf.InverseLerp(bounds.min.x, bounds.max.x, worldX));
    }

    private static void SetMarkerX(RectTransform marker, float normalizedX, float offsetY)
    {
        marker.anchorMin = new Vector2(normalizedX, 0.5f);
        marker.anchorMax = new Vector2(normalizedX, 0.5f);
        marker.anchoredPosition = new Vector2(0f, offsetY);
    }
}
