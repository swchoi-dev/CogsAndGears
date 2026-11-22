using UnityEngine;

// 이 캐릭터가 실제로 받은 피해(Health.OnDamaged)를 DamageNumberLayer에 숫자로 요청한다.
// 공격 처리와 분리되어 있으며, 대상이 제거돼도 숫자는 피격 순간 위치에서 끝까지 재생된다.
public class DamageNumberEmitter : MonoBehaviour
{
    [Tooltip("비워두면 부모의 Health를 사용한다")]
    [SerializeField] private Health _health;
    [SerializeField] private Color _color = new Color(1f, 0.93f, 0.55f, 1f);
    [Tooltip("Collider 윗면 기준 추가 오프셋(월드 단위)")]
    [SerializeField] private Vector3 _offset = new Vector3(0f, 0.2f, 0f);

    private Collider _collider;

    private void Awake()
    {
        if (_health == null)
            _health = GetComponentInParent<Health>();

        _collider = _health != null ? _health.GetComponent<Collider>() : null;
    }

    private void OnEnable()
    {
        if (_health != null)
            _health.OnDamaged += OnDamaged;
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.OnDamaged -= OnDamaged;
    }

    private void OnDamaged(float appliedDamage)
    {
        if (DamageNumberLayer.Instance == null) return;

        DamageNumberLayer.Instance.Show(GetAnchor(), appliedDamage, _color);
    }

    private Vector3 GetAnchor()
    {
        if (_collider == null)
            return _health.transform.position + _offset;

        Bounds bounds = _collider.bounds;
        return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z) + _offset;
    }
}
