using UnityEngine;

// 공격 예상 지점의 바닥 경고 마커(월드 공간). ├───┤ 범위선과 위쪽 ↓ 화살표를 LineRenderer로 그린다.
// 생성자가 Show()로 위치·폭을 정하고, 공격이 끝나면 Hide()로 제거한다.
// 제거 신호를 받지 못해도 남지 않도록 _maxLifetime이 지나면 스스로 제거된다.
public class AttackTelegraph : MonoBehaviour
{
    [SerializeField] private LineRenderer _rangeLine;
    [SerializeField] private LineRenderer _leftTick;
    [SerializeField] private LineRenderer _rightTick;
    [SerializeField] private LineRenderer _arrow;

    [Header("Look")]
    [SerializeField] private Color _color = new Color(1f, 0.22f, 0.15f, 1f);
    [Tooltip("양 끝 세로선의 위·아래 길이")]
    [SerializeField] private float _tickHalfHeight = 0.18f;
    [Tooltip("바닥에서 화살표 끝까지의 높이")]
    [SerializeField] private float _arrowTipHeight = 0.35f;
    [SerializeField] private float _arrowLength = 0.6f;
    [SerializeField] private float _arrowHeadSize = 0.2f;
    [Tooltip("바닥 표면에서 살짝 띄울 높이(겹침 방지)")]
    [SerializeField] private float _groundOffset = 0.03f;

    [Header("Pulse")]
    [SerializeField] private float _pulseSpeed = 12f;
    [Range(0f, 1f)]
    [SerializeField] private float _minAlpha = 0.45f;

    [Tooltip("제거 신호가 없어도 이 시간(초)이 지나면 제거한다")]
    [SerializeField] private float _maxLifetime = 6f;

    private float _spawnTime;
    private LineRenderer[] _lines;

    private void Awake()
    {
        _lines = new[] { _rangeLine, _leftTick, _rightTick, _arrow };
    }

    public void Show(Vector3 groundPoint, float width)
    {
        transform.position = groundPoint + Vector3.up * _groundOffset;
        _spawnTime = Time.time;

        float half = Mathf.Max(0.05f, width * 0.5f);
        SetLine(_rangeLine, new Vector3(-half, 0f), new Vector3(half, 0f));
        SetLine(_leftTick, new Vector3(-half, -_tickHalfHeight), new Vector3(-half, _tickHalfHeight));
        SetLine(_rightTick, new Vector3(half, -_tickHalfHeight), new Vector3(half, _tickHalfHeight));

        float tipY = _arrowTipHeight;
        _arrow.positionCount = 5;
        _arrow.SetPositions(new[]
        {
            new Vector3(0f, tipY + _arrowLength),
            new Vector3(0f, tipY),
            new Vector3(-_arrowHeadSize, tipY + _arrowHeadSize),
            new Vector3(0f, tipY),
            new Vector3(_arrowHeadSize, tipY + _arrowHeadSize),
        });

        ApplyColor(1f);
    }

    public void Hide()
    {
        // 이미 수명 만료로 제거된 경우에도 안전하게 호출할 수 있다
        if (this == null) return;

        Destroy(gameObject);
    }

    private void Update()
    {
        if (Time.time - _spawnTime >= _maxLifetime)
        {
            Hide();
            return;
        }

        float pulse = 0.5f + 0.5f * Mathf.Sin((Time.time - _spawnTime) * _pulseSpeed);
        ApplyColor(Mathf.Lerp(_minAlpha, 1f, pulse));
    }

    private void ApplyColor(float alpha)
    {
        Color color = _color;
        color.a *= alpha;

        foreach (LineRenderer line in _lines)
        {
            line.startColor = color;
            line.endColor = color;
        }
    }

    private static void SetLine(LineRenderer line, Vector3 from, Vector3 to)
    {
        line.positionCount = 2;
        line.SetPosition(0, from);
        line.SetPosition(1, to);
    }

    // point 바로 아래의 바닥 표면을 찾는다. point가 바닥 안쪽이어도 위에서부터 내려 찍는다.
    public static bool TryFindGround(Vector3 point, LayerMask groundMask, float probeHeight, out Vector3 groundPoint)
    {
        Vector3 origin = point + Vector3.up * probeHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, probeHeight + 30f, groundMask, QueryTriggerInteraction.Ignore))
        {
            groundPoint = new Vector3(point.x, hit.point.y, point.z);
            return true;
        }

        groundPoint = point;
        return false;
    }
}
