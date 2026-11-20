using System;
using UnityEngine;

public abstract class CharacterStats : MonoBehaviour
{
    [SerializeField] private int _health;
    [SerializeField] private int _maxHealth;

    public event Action OnDeath;
    public event Action OnHpChanged;

    public int Health
    {
        get => _health;
        set
        {
            _health = Mathf.Clamp(value, 0, _maxHealth);
            if (_health == 0)
            {
                OnDeath?.Invoke();
            }
            else
            {
                OnHpChanged?.Invoke();
            }
        }
    }
}
