using System.Collections.Generic;
using UnityEngine;

// 화면(HUD) 위 피해 숫자 표시 레이어. DamageNumberEmitter가 Show()로 요청하고, DamagePopup을 풀로 재사용한다.
// 표시 시간은 실제 시간 기준이라 Hit Stop·Slow 중에도 일정하게 사라진다. Stage가 바뀌면 남은 숫자를 모두 지운다.
public class DamageNumberLayer : MonoBehaviour
{
    public static DamageNumberLayer Instance { get; private set; }

    [SerializeField] private GameFlowManager _gameFlow;
    [Tooltip("비워두면 Camera.main")]
    [SerializeField] private Camera _camera;
    [SerializeField] private DamagePopup _popupPrefab;

    [Header("Animation")]
    [Tooltip("표시 시간(실제 시간, 초)")]
    [SerializeField] private float _duration = 0.85f;
    [Tooltip("떠오르는 높이(월드 단위)")]
    [SerializeField] private float _riseDistance = 0.8f;
    [Tooltip("등장 순간 배율")]
    [SerializeField] private float _popScale = 1.35f;
    [Tooltip("연속 피격 숫자가 겹치지 않도록 주는 좌우 무작위 폭(월드 단위)")]
    [SerializeField] private float _horizontalJitter = 0.35f;

    private readonly List<DamagePopup> _pool = new List<DamagePopup>();
    private readonly List<DamagePopup> _active = new List<DamagePopup>();

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;
    }

    private void OnEnable()
    {
        Instance = this;

        if (_gameFlow != null)
            _gameFlow.StageStarted += OnStageStarted;
    }

    private void OnDisable()
    {
        if (_gameFlow != null)
            _gameFlow.StageStarted -= OnStageStarted;

        ClearAll();

        if (Instance == this)
            Instance = null;
    }

    public void Show(Vector3 worldPosition, float amount, Color color)
    {
        if (_camera == null || _popupPrefab == null) return;

        // 소수 피해도 최소 1로 보이게 반올림한다
        int shown = Mathf.Max(1, Mathf.RoundToInt(amount));
        worldPosition.x += Random.Range(-_horizontalJitter, _horizontalJitter);

        DamagePopup popup = Get();
        _active.Add(popup);
        popup.Play(this, (RectTransform)transform, _camera, worldPosition, shown, color, _duration, _riseDistance, _popScale);
    }

    public void Release(DamagePopup popup)
    {
        popup.gameObject.SetActive(false);
        _active.Remove(popup);
        _pool.Add(popup);
    }

    private DamagePopup Get()
    {
        if (_pool.Count == 0)
            return Instantiate(_popupPrefab, transform);

        DamagePopup popup = _pool[_pool.Count - 1];
        _pool.RemoveAt(_pool.Count - 1);
        return popup;
    }

    private void OnStageStarted(StageController stage) => ClearAll();

    private void ClearAll()
    {
        for (int i = _active.Count - 1; i >= 0; i--)
            Release(_active[i]);
    }
}
