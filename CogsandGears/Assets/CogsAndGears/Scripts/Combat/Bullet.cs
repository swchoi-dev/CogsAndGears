using UnityEngine;

public class Bullet : Projectile
{
    private ProjectileContext _context;
    private bool _launched;
    
    protected override void FixedUpdate()
    {
        if (!_launched || collided || rb == null)
            return;

        rb.linearVelocity = transform.forward * _context.Speed;
    }

    public override void Initialize(ProjectileContext projectileContext)
    {
        _context = projectileContext;
        rb.linearVelocity = Vector3.zero;
        _launched = false;
    }

    public override void Launch()
    {
        _launched = true;
    }
}
