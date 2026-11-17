using Hovl;
using UnityEngine;

public abstract class Projectile : HS_ProjectileMover, IProjectile
{
    public abstract void Initialize(ProjectileContext projectileContext);

    public abstract void Launch();
}
