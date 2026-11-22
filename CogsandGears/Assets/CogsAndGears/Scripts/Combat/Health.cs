using System;
using UnityEngine;
using UnityEngine.Serialization;

// 플레이어·몬스터 공통 HP 처리. 실드는 _maxShield > 0인 오브젝트(예: 플레이어)에서만 동작한다.
// 피해는 실드 → HP 순으로 적용하고, 마지막 피해 시각부터 _shieldRechargeDelay초가 지나면 실드를 재충전한다.
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private StatDataSO _stats;
    [SerializeField] private float _currentHp;

    [Header("Shield (0이면 실드 없음)")]
    [Min(0f)]
    [SerializeField] private float _maxShield;
    [Tooltip("마지막 피해 시각부터 재충전을 시작하기까지의 대기 시간(초)")]
    [Min(0f)]
    [SerializeField] private float _shieldRechargeDelay;
    [Tooltip("재충전 중 초당 회복량")]
    [Min(0f)]
    [SerializeField] private float _shieldRechargePerSecond;

    public event Action OnDeath;
    public event Action<float> OnHpChanged;
    public event Action<float> OnShieldChanged;
    // 실드 또는 HP에 실제로 피해가 적용됐을 때만 호출된다(재충전·초기화·무적 제외).
    // 인자는 실제 적용량(실드 흡수 + HP 감소)이다. 피격 피드백·피해 숫자용.
    public event Action<float> OnDamaged;

    private bool _isDead;
    private float _currentShield;
    private float _lastDamageTime = float.NegativeInfinity;
    private IInvulnerable[] _invulnerables;

    public float CurrentHp => _currentHp;
    public float MaxHp => _stats.MaxHealth;
    public float CurrentShield => _currentShield;
    public float MaxShield => _maxShield;
    public bool IsDead => _isDead;

    private void Awake()
    {
        // 같은 GameObject의 무적 상태 제공자(예: PlayerDash). 없으면 항상 피해를 받는다.
        _invulnerables = GetComponents<IInvulnerable>();
    }

    private void Start()
    {
        _currentHp = _stats.MaxHealth;
        _currentShield = _maxShield;

        // 초기값을 구독자(HUD·HP 바)에 알린다
        OnHpChanged?.Invoke(_currentHp);
        OnShieldChanged?.Invoke(_currentShield);
    }

    private void Update()
    {
        RechargeShield(Time.time, Time.deltaTime);
    }

    public void TakeDamage(float damage)
    {
        // 0 이하·NaN 피해는 무시한다
        if (_isDead || !(damage > 0f)) return;

        // 무적이면 피해·재충전 대기·피격 이벤트 모두 없이 무시한다
        if (IsInvulnerable()) return;

        _lastDamageTime = Time.time;

        float remaining = damage;
        float applied = 0f;

        if (_currentShield > 0f)
        {
            float absorbed = Mathf.Min(_currentShield, remaining);
            _currentShield = Mathf.Clamp(_currentShield - absorbed, 0f, _maxShield);
            remaining -= absorbed;
            applied += absorbed;
            OnShieldChanged?.Invoke(_currentShield);
        }

        if (remaining > 0f)
        {
            float hpBefore = _currentHp;
            _currentHp = Mathf.Clamp(_currentHp - remaining, 0, _stats.MaxHealth);
            applied += hpBefore - _currentHp;
            OnHpChanged?.Invoke(_currentHp);
        }

        if (applied > 0f)
            OnDamaged?.Invoke(applied);

        if (remaining <= 0f || _currentHp > 0) return;

        _isDead = true;
        OnDeath?.Invoke();
    }

    private bool IsInvulnerable()
    {
        foreach (IInvulnerable source in _invulnerables)
        {
            if (source != null && source.IsInvulnerable) return true;
        }

        return false;
    }

    // 경계 포함: now - 마지막 피해 시각 >= 대기 시간이면 충전한다. (지연 3초, t=2 피해 → t=5부터 충전)
    private void RechargeShield(float now, float deltaTime)
    {
        if (_isDead || _currentShield >= _maxShield || _shieldRechargePerSecond <= 0f) return;

        if (now - _lastDamageTime < _shieldRechargeDelay) return;

        _currentShield = Mathf.Min(_currentShield + _shieldRechargePerSecond * deltaTime, _maxShield);
        OnShieldChanged?.Invoke(_currentShield);
    }
}
