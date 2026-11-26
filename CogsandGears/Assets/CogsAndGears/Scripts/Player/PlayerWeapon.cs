using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerWeapon : MonoBehaviour
{
    private const float MIN_AIM_DISTANCE_SQR = 0.01f;

    [SerializeField] Camera cam;
    [SerializeField] Animation camAnim;
    
    [SerializeField] private WeaponDataSO _defaultData;
    [SerializeField] private Transform _muzzlePoint;

    [SerializeField] private List<WeaponRuntime> _weapons = new();

    private PlayerDash _playerDash;
    private PlayerAim _playerAim;
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

        // 총은 애니메이션의 손 자세를 따르므로(예: Pistol Run Backward) 총구 방향이 조준과 어긋날 수 있다.
        // 생성 위치는 총구(플레이어 평면 z)로 두고, 방향은 총구에서 마우스 포인터를 향하도록 XY 평면에서 계산한다.
        Vector3 origin = _muzzlePoint.position;
        origin.z = transform.position.z;

        Vector2 aimDirection = GetAimDirection(origin);

        // 조준 방향에 탄착각 적용
        Vector3 fireDirection =
            Quaternion.AngleAxis(spreadAngle, Vector3.forward) * new Vector3(aimDirection.x, aimDirection.y, 0f);

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

    // 총구 → 마우스 포인터 방향. 포인터가 총구와 너무 가깝거나 총구 뒤쪽이면 어깨 기준 조준 방향을 쓴다.
    private Vector2 GetAimDirection(Vector3 origin)
    {
        Vector2 toPointer = _playerAim.AimPoint - (Vector2)origin;

        if (toPointer.sqrMagnitude < MIN_AIM_DISTANCE_SQR || Vector2.Dot(toPointer, _playerAim.AimDirection) <= 0f)
            return _playerAim.AimDirection;

        return toPointer.normalized;
    }

    private void CacheComponents()
    {
        _playerDash = GetComponent<PlayerDash>();
        _playerAim = GetComponent<PlayerAim>();
    }
}
