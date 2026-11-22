using UnityEngine;

// Drone 전용 비행 이동과 원거리 공격. 공통 처리(스탯·체력·타깃 초기화·방향 전환)는 MonsterController가 담당한다.
// Behavior Graph의 FlyAction / DroneAttackAction이 이 컴포넌트를 호출한다.
[RequireComponent(typeof(Rigidbody), typeof(MonsterController))]
public class DroneController : MonoBehaviour
{
    // 트리거 후 Animator가 Attack 상태로 진입할 때까지 기다리는 시간
    private const float ATTACK_START_GRACE_SECONDS = 0.3f;

    [Header("Flight")]
    [Tooltip("플레이어와의 최소 수평 거리. 이보다 가까우면 이 거리까지 물러난다")]
    [SerializeField] private float _preferredDistance = 3.5f;
    [Tooltip("플레이어 기준 유지할 높이")]
    [SerializeField] private float _hoverHeight = 2.5f;
    [Tooltip("사격 위치에서 이 거리 이상 벗어나면 다시 이동한다")]
    [SerializeField] private float _repositionThreshold = 1f;
    [Tooltip("사격 위치까지 이 거리 이내면 도착으로 본다")]
    [SerializeField] private float _arriveTolerance = 0.1f;
    [Tooltip("사거리 밖에서 접근할 때 AttackRange보다 이만큼 안쪽까지 들어간다 (경계 진동 방지)")]
    [SerializeField] private float _attackRangeMargin = 0.5f;

    [Header("Ranged Attack")]
    [SerializeField] private Transform _muzzlePoint;
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private float _projectileSpeed = 10f;

    private Rigidbody _rb;
    private MonsterAnimHandler _animHandler;
    private MonsterController _monster;

    private Transform _attackTarget;
    private bool _hasFired;
    private float _attackStartTime;
    private float _nextAttackTime;

    // 공격 트리거부터 Attack 애니메이션 종료까지 true. 이 동안 FlyAction은 이동하지 않는다.
    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animHandler = GetComponent<MonsterAnimHandler>();
        _monster = GetComponent<MonsterController>();
    }

    private void Update()
    {
        UpdateAttackState();
    }

    // 공격 범위를 벗어났거나 사격 위치에서 충분히 벗어나 다시 이동해야 하면 true.
    // 공격 범위는 그래프의 Distance 조건과 같은 기준(Transform 간 3D 거리)으로 판단한다.
    // 사거리 안에서는 사격 위치가 현재 수평 위치를 유지하므로, 너무 가깝거나 높이가 크게 어긋난 경우에만 이동한다.
    public bool NeedsReposition(Transform target)
    {
        if (Vector3.Distance(transform.position, target.position) > _monster.Stats.AttackRange)
            return true;

        return (GetFirePosition(target) - _rb.position).magnitude > _repositionThreshold;
    }

    // 사격 위치로 한 프레임 이동한다. 도착했으면 정지하고 false를 반환한다.
    public bool FlyToFirePosition(Transform target, float speed)
    {
        Vector3 delta = GetFirePosition(target) - _rb.position;
        float distance = delta.magnitude;

        if (distance <= _arriveTolerance)
        {
            StopFlying();
            return false;
        }

        // 도착 지점을 지나치지 않도록 남은 거리만큼만 속도를 낸다
        float moveSpeed = Mathf.Min(speed, distance / Time.fixedDeltaTime);
        _rb.linearVelocity = delta / distance * moveSpeed;
        return true;
    }

    public void StopFlying()
    {
        _rb.linearVelocity = Vector3.zero;
    }

    // Behavior Graph(DroneAttackAction)에서 호출한다. 쿨다운 중이거나 이미 공격 중이면 false.
    public bool TryStartAttack(Transform target)
    {
        if (IsAttacking || target == null || _animHandler == null || _monster == null)
            return false;

        if (Time.time < _nextAttackTime)
            return false;

        IsAttacking = true;
        _hasFired = false;
        _attackTarget = target;
        _attackStartTime = Time.time;
        _nextAttackTime = _attackStartTime + _monster.Stats.AttackInterval;

        StopFlying();
        _animHandler.PlayAttack();
        return true;
    }

    // Attack 클립의 발사 프레임 Animation Event(OnFire)에서 호출한다.
    public void Fire()
    {
        // 공격 중이 아니거나 이미 발사했으면 중복 이벤트로 보고 무시한다
        if (!IsAttacking || _hasFired)
            return;

        _hasFired = true;

        if (_attackTarget == null || _muzzlePoint == null || _projectilePrefab == null)
            return;

        // 모델 회전과 무관하게 MuzzlePoint → 플레이어 중심 방향으로 XY 평면에서 발사한다.
        // MuzzlePoint는 모델 회전에 따라 z가 달라지므로 생성 위치를 게임플레이 평면(루트 z)에 맞춘다.
        Vector3 origin = _muzzlePoint.position;
        origin.z = _rb.position.z;
        Vector3 direction = GetAimPoint(_attackTarget) - origin;
        direction.z = 0f;

        if (direction.sqrMagnitude < Mathf.Epsilon)
            return;

        Projectile projectile = Instantiate(
            _projectilePrefab,
            origin,
            Quaternion.LookRotation(direction.normalized)
        );

        projectile.Initialize(new ProjectileContext
        {
            Damage = _monster.Stats.Damage,
            Speed = _projectileSpeed,
            Owner = gameObject,
        });
        projectile.Launch();
    }

    // 현재 있는 쪽과 수평 거리를 최대한 유지하는 사격 위치.
    // 수평 거리는 [_preferredDistance, 사거리 안쪽 최대 수평 거리]로 제한하고, 높이는 플레이어 + _hoverHeight.
    // → 사거리 안이면 x는 그대로(높이만 보정), 너무 가까우면 물러나고, 사거리 밖이면 사거리 안쪽까지 접근한다.
    private Vector3 GetFirePosition(Transform target)
    {
        Vector3 targetPosition = target.position;
        float side = _rb.position.x >= targetPosition.x ? 1f : -1f;
        float horizontalDistance = Mathf.Abs(_rb.position.x - targetPosition.x);

        // 사격 위치(높이 _hoverHeight, 현재 z)에서 Transform 간 거리가 (AttackRange - 여유) 이내가 되는 최대 수평 거리
        float rangeInside = Mathf.Max(0f, _monster.Stats.AttackRange - _attackRangeMargin);
        float depth = transform.position.z - targetPosition.z;
        float maxHorizontal = Mathf.Sqrt(Mathf.Max(0f, rangeInside * rangeInside - _hoverHeight * _hoverHeight - depth * depth));
        // 사거리가 최소 거리보다 짧게 설정된 경우에도 최소 거리를 우선한다
        maxHorizontal = Mathf.Max(maxHorizontal, _preferredDistance);

        float desiredHorizontal = Mathf.Clamp(horizontalDistance, _preferredDistance, maxHorizontal);

        return new Vector3(
            targetPosition.x + side * desiredHorizontal,
            targetPosition.y + _hoverHeight,
            _rb.position.z
        );
    }

    private static Vector3 GetAimPoint(Transform target)
    {
        return target.TryGetComponent(out Collider targetCollider)
            ? targetCollider.bounds.center
            : target.position;
    }

    private void UpdateAttackState()
    {
        if (!IsAttacking)
            return;

        if (Time.time - _attackStartTime < ATTACK_START_GRACE_SECONDS)
            return;

        if (_animHandler.IsPlayingAttack())
            return;

        IsAttacking = false;
    }
}
