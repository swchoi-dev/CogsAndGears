using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Animation camAnim;
    
    [SerializeField] private WeaponDataSO _defaultData;
    [SerializeField] private Transform _muzzlePoint;
    
    private PlayerInputHandler _playerInput;
    private List<WeaponRuntime> _weapons;
    private WeaponRuntime _defaultWeapon;

    private void Awake() => CacheComponents();

    private void Start() => InitWeaponData();

    private void Update()
    {
        if (!_playerInput.IsFirePressed) return;
        
        float currentTime = Time.time;
        
        foreach (var weapon in _weapons)
        {
            weapon.TryFire(currentTime);
        }
    }
    
    public WeaponRuntime AddWeapon(WeaponDataSO data)
    {
        var weapon = new WeaponRuntime(data);
        _weapons.Add(weapon);
        
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
        
        Quaternion projectileRotation =
            Quaternion.LookRotation(_muzzlePoint.right);
        
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
            HitEffectPrefab = data.HitEffectPrefab,
            Owner = gameObject,
        };
        
        projectile.Initialize(context);
        projectile.Launch();
        
    }
    
    private void InitWeaponData()
    {
        _defaultWeapon = new WeaponRuntime(_defaultData);
        _weapons = new() { _defaultWeapon };
        
        _defaultWeapon.FireRequested += OnFire;
    }

    private void CacheComponents()
    {
        _playerInput = GetComponent<PlayerInputHandler>();
    }
}
