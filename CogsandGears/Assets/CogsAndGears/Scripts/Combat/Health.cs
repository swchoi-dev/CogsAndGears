using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private StatDataSO _stats;
    [SerializeField] private float _currentHp;
    public event Action OnDeath;
    public event Action<float> OnHpChanged;

    private bool _isDead;
    private void Start()
    {
        _currentHp = _stats.MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHp = Mathf.Clamp(_currentHp - damage, 0, _stats.MaxHealth);

        OnHpChanged?.Invoke(_currentHp);

        if (_currentHp > 0) return;

        _isDead = true;
        OnDeath?.Invoke();
    }
}
