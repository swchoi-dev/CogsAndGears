using UnityEngine;

// 플레이어 HP·실드 게이지. Health가 단일 데이터 소스이며, 이벤트로만 갱신한다.
public class PlayerHealthHUD : MonoBehaviour
{
    [SerializeField] private Health _health;
    [SerializeField] private GaugeUI _hpGauge;
    [Tooltip("MaxShield가 0이면 숨긴다")]
    [SerializeField] private GaugeUI _shieldGauge;

    private Health _boundHealth;

    private void OnEnable()
    {
        Bind(_health);
    }

    private void OnDisable()
    {
        Unbind();
    }

    // 대상 Health를 교체할 때 호출한다. 기존 구독을 해제하고 새 Health 상태로 즉시 동기화한다.
    public void SetHealth(Health health)
    {
        _health = health;

        if (isActiveAndEnabled)
            Bind(health);
    }

    private void Bind(Health health)
    {
        Unbind();

        if (health == null) return;

        _boundHealth = health;
        _boundHealth.OnHpChanged += OnHpChanged;
        _boundHealth.OnShieldChanged += OnShieldChanged;

        RefreshHp();
        RefreshShield();
    }

    private void Unbind()
    {
        if (_boundHealth == null) return;

        _boundHealth.OnHpChanged -= OnHpChanged;
        _boundHealth.OnShieldChanged -= OnShieldChanged;
        _boundHealth = null;
    }

    private void OnHpChanged(float currentHp) => RefreshHp();

    private void OnShieldChanged(float currentShield) => RefreshShield();

    private void RefreshHp()
    {
        _hpGauge.SetValue(_boundHealth.CurrentHp, _boundHealth.MaxHp);
    }

    private void RefreshShield()
    {
        bool hasShield = _boundHealth.MaxShield > 0f;
        _shieldGauge.gameObject.SetActive(hasShield);

        if (!hasShield) return;

        _shieldGauge.SetValue(_boundHealth.CurrentShield, _boundHealth.MaxShield);
    }
}
