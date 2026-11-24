using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponData",
    menuName = "CogsAndGears/Weapon Data"
)]
public class WeaponDataSO : ItemDataSO
{
    [Min(1f)]
    [SerializeField] private float _damage;
    
    [Min(0.01f)]
    [SerializeField] private float _fireRate;
    
    [Min(1f)]
    [SerializeField] private float _projectileSpeed;
    
    [Header("Projectile Spread")]
    [Range(0f, 15f)]
    public float spreadAngle = 5f;
    
    [Header("Prefabs")]
    [SerializeField] private Projectile _projectilePrefab;
    
    public float Damage => _damage;
    public float FireRate => _fireRate;
    public float ProjectileSpeed => _projectileSpeed;
    public float SpreadAngle => spreadAngle;
    
    public Projectile ProjectilePrefab => _projectilePrefab;
}
