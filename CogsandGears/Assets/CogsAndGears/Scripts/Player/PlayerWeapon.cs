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
    // 현재 발사 대상 무기 목록(읽기 전용). 인벤토리 동기화에 사용한다.
    public IReadOnlyList<WeaponRuntime> Weapons => _weapons;
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

        // 총은 애니메이션의 손 자세를 따르므로(예: Pistol Run Backward) 총구가 화면 안팎(z)으로 기울 수 있다.
        // 총구 방향을 XY 플레이 평면에 투영하고 생성 위치도 플레이어 평면(z)에 맞춘다.
        Vector3 muzzleDirection = _muzzlePoint.right;
        muzzleDirection.z = 0f;
        muzzleDirection.Normalize();

        Vector3 origin = _muzzlePoint.position;
        origin.z = transform.position.z;

        // 총구 방향에 탄착각 적용
        Vector3 fireDirection =
            Quaternion.AngleAxis(spreadAngle, Vector3.forward) * muzzleDirection;

        // 최종 투사체 회전
        Quaternion projectileRotation = Quaternion.LookRotation(fireDirection);

        var projectile = Instantiate(
            data.ProjectilePrefab,
            origin,
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
