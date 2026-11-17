using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponData",
    menuName = "CogsAndGears/Weapon Data"
)]
public class WeaponDataSO : ScriptableObject
{
    [Min(1f)]
    [SerializeField] private float _damage;
    
    [Min(0.01f)]
    [SerializeField] private float _fireRate;
    
    [Min(1f)]
    [SerializeField] private float _projectileSpeed;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject _muzzleFlashPrefab;
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private GameObject _hitEffectPRefab;
    
    public float Damage => _damage;
    public float FireRate => _fireRate;
    public float ProjectileSpeed => _projectileSpeed;
    
    public GameObject MuzzleFlashPrefab => _muzzleFlashPrefab;
    public Projectile ProjectilePrefab => _projectilePrefab;
    public GameObject HitEffectPrefab => _hitEffectPRefab;
}
