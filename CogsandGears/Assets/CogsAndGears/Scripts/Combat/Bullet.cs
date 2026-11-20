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

    protected override void OnCollisionEnter(Collision collision)
    {
        if (collided)
            return;

        if (collision.gameObject.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(_context.Damage);
        }

        base.OnCollisionEnter(collision);
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
