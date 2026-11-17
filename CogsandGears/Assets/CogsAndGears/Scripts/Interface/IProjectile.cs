using UnityEngine;

public interface IProjectile
{
    void Initialize(ProjectileContext projectileContext);
    void Launch();
}
