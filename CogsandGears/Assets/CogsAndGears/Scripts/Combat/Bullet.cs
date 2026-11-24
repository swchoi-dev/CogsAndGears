using System;
using UnityEngine;

public class Bullet : Projectile
{
    private ProjectileContext _context;
    private bool _launched;
    private bool _finished;

    // 충돌하거나 제거(수명 만료 포함)되면 한 번 호출된다. 공격 예고 마커 제거 등에 사용한다.
    public event Action Finished;
    
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
        Finish();
    }

    private void OnDestroy()
    {
        Finish();
    }

    private void Finish()
    {
        if (_finished) return;

        _finished = true;
        Finished?.Invoke();
    }
    
    public override void Initialize(ProjectileContext projectileContext)
    {
        _context = projectileContext;
        rb.linearVelocity = Vector3.zero;
        _launched = false;
        _finished = false;
    }

    public override void Launch()
    {
        _launched = true;
    }
}
