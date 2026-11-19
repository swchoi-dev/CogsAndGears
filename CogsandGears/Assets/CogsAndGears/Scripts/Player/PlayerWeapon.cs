using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Animation camAnim;
    
    [SerializeField] private WeaponDataSO _defaultData;
    [SerializeField] private Transform _muzzlePoint;

    [SerializeField] private List<WeaponRuntime> _weapons = new();

    private PlayerDash _playerDash;
    private WeaponRuntime _defaultWeapon;

    public WeaponDataSO DefaultData => _defaultData;
    private void Awake() => CacheComponents();

    private void OnEnable()
    {
        foreach (var weapon in _weapons)
        {
            weapon.FireRequested += OnFire;
        }
    }
    
    private void Update()
    {
        if (_playerDash.IsDashing) return;
        if (!PlayerInputHandler.Instance.IsFirePressed) return;
        
        float currentTime = Time.time;
        
        foreach (var weapon in _weapons)
        {
            weapon.TryFire(currentTime);
        }
    }
    
    private void OnDisable()
    {
        foreach (var weapon in _weapons)
        {
            weapon.FireRequested -= OnFire;
        }
    }
    
    public WeaponRuntime AddWeapon(WeaponDataSO data)
    {
        var weapon = new WeaponRuntime(data);
        _weapons.Add(weapon);
        
        if (isActiveAndEnabled)
            weapon.FireRequested += OnFire;

        return weapon;
    }

    public bool RemoveWeapon(WeaponRuntime weapon)
    {
        if (!_weapons.Remove(weapon)) return false;
        
        weapon.FireRequested -= OnFire;

        return true;
    }
    
    private void OnFire(WeaponRuntime weapon)
    {
        var data = weapon.Data;
        
        camAnim.Play(camAnim.clip.name);
        
        // 무기별 랜덤 탄착각
        float spreadAngle = Random.Range(-data.SpreadAngle, data.SpreadAngle);

        // 총구 방향에 탄착각 적용
        Vector3 fireDirection =
            Quaternion.AngleAxis(spreadAngle, Vector3.forward) * _muzzlePoint.right;

        // 최종 투사체 회전
        Quaternion projectileRotation = Quaternion.LookRotation(fireDirection);
        
        var projectile = Instantiate(
            data.ProjectilePrefab,
            _muzzlePoint.position,
            projectileRotation
        );
        
        // Bullet Instantiate
        var context = new ProjectileContext
        {
            Damage = data.Damage,
            Speed = data.ProjectileSpeed,
            Owner = gameObject,
        };
        
        projectile.Initialize(context);
        projectile.Launch();
        
    }

    private void CacheComponents()
    {
        _playerDash = GetComponent<PlayerDash>();
    }
}
