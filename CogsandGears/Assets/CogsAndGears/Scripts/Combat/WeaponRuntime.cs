using System;
using UnityEngine;

public class WeaponRuntime
{
    public event Action<WeaponRuntime> FireRequested;
    
    private WeaponDataSO _data;
    private float _nextFireTime;
    
    public WeaponDataSO Data => _data;
    
    public WeaponRuntime(WeaponDataSO data)
    {
        _data = data;
    }

    private bool CanFire(float currentTime)
    {
        return _data.FireRate > 0f && currentTime >= _nextFireTime;
    }

    public void TryFire(float currentTime)
    {
        if (!CanFire(currentTime)) return;
        
        _nextFireTime = currentTime + 1f / _data.FireRate;
        
        FireRequested?.Invoke(this);
    }
}
