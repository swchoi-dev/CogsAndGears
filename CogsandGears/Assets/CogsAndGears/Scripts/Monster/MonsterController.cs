using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class MonsterController : MonoBehaviour
{
    private const float DEFAULT_MODEL_ROTATION_ANGLE = 45f;
    // 도약 직후 아직 지면과 닿아 있는 접촉을 착지로 오판하지 않기 위한 최소 체공 시간
    private const float MIN_AIRBORNE_SECONDS = 0.1f;
    // 이 값보다 빠르게 상승 중이면 착지로 보지 않는다
    private const float LANDING_MAX_VERTICAL_SPEED = 0.1f;
    // 트리거 후 Animator가 JumpAttack 상태로 진입할 때까지 기다리는 시간
    private const float ATTACK_START_GRACE_SECONDS = 0.5f;
    // 예정 체공 시간을 이만큼 넘겨도 착지하지 못하면 피해 없이 점프를 종료한다
    private const float AIRBORNE_TIMEOUT_EXTRA_SECONDS = 1f;
    
    [SerializeField] private StatDataSO _stats;
    [SerializeField] private BehaviorGraphAgent _agent;
    [SerializeField] private Transform _modelRoot;
    [SerializeField] private float _turnSpeed = 540f;
    [SerializeField] private bool _defaultFacingRight = true;
    
    [Header("Jump Attack")]
    [Tooltip("도약부터 착지까지의 체공 시간(초). JumpAttack 클립의 도약→착지 이벤트 간격과 맞춘다.")]
    [SerializeField] private float _jumpDuration = 1.3f;
    [SerializeField] private float _maxJumpDistance = 5f;
    [Tooltip("착지 지점(발 위치)으로부터 XY 평면에서 피해를 주는 반경")]
    [SerializeField] private float _landingAttackRadius = 2f;
    [Tooltip("접촉 법선의 Y값이 이 값 이상이어야 지면 착지로 판정한다 (벽 충돌 제외)")]
    [SerializeField] private float _groundNormalMinY = 0.7f;
    
    private Transform _target;
    private Health _health;
    
    private Quaternion _defaultRotation;
    
    private Rigidbody _rb;
    private Collider _collider;
    private MonsterAnimHandler _animHandler;

    private bool _hasTakenOff;
    private bool _isAirborne;
    private float _attackStartTime;
    private float _takeoffTime;
    private float _nextAttackTime;
    
    // 공격 트리거부터 JumpAttack 애니메이션 종료까지 true. 이 동안 ChaseAction은 Rigidbody를 제어하지 않는다.
    public bool IsJumpAttacking { get; private set; }

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        _health.OnDeath += Die;
    }
    
    private void Update()
    {
        UpdateJumpAttackState();
    }
    
    private void LateUpdate()
    {
        UpdateFacing();
    }

    private void OnDisable()
    {
        _health.OnDeath -= Die;
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckLanding(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        CheckLanding(collision);
    }
    
    private void UpdateFacing()
    {
        if (_target == null || _modelRoot == null)
            return;

        float directionX = _target.position.x - transform.position.x;

        Quaternion rightRotation = Quaternion.Euler(
            0f, DEFAULT_MODEL_ROTATION_ANGLE + 90f, 0f
        );

        Quaternion leftRotation = Quaternion.Euler(
            0f, DEFAULT_MODEL_ROTATION_ANGLE + 180f, 0f
        );

        FacingUtility.RotateTowardsX(
            _modelRoot,
            directionX,
            rightRotation,
            leftRotation,
            _turnSpeed,
            false
        );
    }
    
    private void Die()
    {
        Destroy(gameObject);
    }

    // Behavior Graph(JumpAttackAction)에서 호출한다. 쿨다운 중이거나 이미 공격 중이면 false.
    public bool TryStartJumpAttack()
    {
        if (IsJumpAttacking || _animHandler == null || _target == null)
            return false;

        if (Time.time < _nextAttackTime)
            return false;

        IsJumpAttacking = true;
        _hasTakenOff = false;
        _isAirborne = false;
        _attackStartTime = Time.time;

        _animHandler.PlayJumpAttack();
        return true;
    }

    // JumpAttack 클립의 도약 프레임 Animation Event(OnJumpTakeoff)에서 호출한다.
    public void JumpTowardTarget()
    {
        // 공격 중이 아니거나 이미 도약했으면 중복 이벤트로 보고 무시한다
        if (!IsJumpAttacking || _hasTakenOff)
            return;

        _hasTakenOff = true;

        if (_target == null || _rb == null || _collider == null)
            return;

        float duration = Mathf.Max(0.1f, _jumpDuration);

        // 도약 순간의 플레이어 위치를 목표로 고정한다. 높이는 Transform 중심이 아닌 발(Collider 하단) 기준.
        float footY = _collider.bounds.min.y;
        float deltaX = Mathf.Clamp(
            _target.position.x - _rb.position.x,
            -_maxJumpDistance,
            _maxJumpDistance
        );
        float deltaY = GetTargetFootY() - footY;

        // 수평: 등속 / 수직: y = vy*t + 0.5*g*t^2 를 t = duration 에서 deltaY 가 되도록 역산
        float velocityX = deltaX / duration;
        float velocityY = (deltaY - 0.5f * Physics.gravity.y * duration * duration) / duration;

        Vector3 currentVelocity = _rb.linearVelocity;
        Vector3 velocityChange = new Vector3(
            velocityX - currentVelocity.x,
            velocityY - currentVelocity.y,
            0f
        );

        _isAirborne = true;
        _takeoffTime = Time.time;
        _rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private float GetTargetFootY()
    {
        return _target.TryGetComponent(out Collider targetCollider)
            ? targetCollider.bounds.min.y
            : _target.position.y;
    }

    private void CheckLanding(Collision collision)
    {
        if (!_isAirborne)
            return;

        if (Time.time - _takeoffTime < MIN_AIRBORNE_SECONDS)
            return;

        if (_rb.linearVelocity.y > LANDING_MAX_VERTICAL_SPEED)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y < _groundNormalMinY)
                continue;

            Land(true);
            return;
        }
    }

    private void Land(bool applyDamage)
    {
        _isAirborne = false;
        _nextAttackTime = Time.time + _stats.AttackInterval;

        Vector3 velocity = _rb.linearVelocity;
        velocity.x = 0f;
        _rb.linearVelocity = velocity;

        if (applyDamage)
            ApplyLandingDamage();
    }

    // 착지 순간의 실제 플레이어 위치로 판정한다 (도약 시 캡처한 목표 위치 아님)
    private void ApplyLandingDamage()
    {
        if (_target == null)
            return;

        Vector2 landingPoint = new Vector2(_rb.position.x, _collider.bounds.min.y);
        Vector2 targetPoint = _target.position;

        if (_target.TryGetComponent(out Collider targetCollider))
        {
            Vector3 probe = new Vector3(landingPoint.x, landingPoint.y, targetCollider.bounds.center.z);
            targetPoint = targetCollider.ClosestPoint(probe);
        }

        if (Vector2.Distance(landingPoint, targetPoint) > _landingAttackRadius)
            return;

        if (_target.TryGetComponent(out IDamageable damageable))
            damageable.TakeDamage(_stats.Damage);
    }

    private void UpdateJumpAttackState()
    {
        if (!IsJumpAttacking)
            return;

        if (_isAirborne)
        {
            if (Time.time - _takeoffTime > _jumpDuration + AIRBORNE_TIMEOUT_EXTRA_SECONDS)
                Land(false);

            return;
        }

        if (Time.time - _attackStartTime < ATTACK_START_GRACE_SECONDS)
            return;

        if (_animHandler.IsPlayingJumpAttack())
            return;

        // 착지하지 못한 채(도약 실패 등) 애니메이션이 끝난 경우에도 쿨다운을 적용한다
        if (_nextAttackTime < _attackStartTime)
            _nextAttackTime = Time.time + _stats.AttackInterval;

        IsJumpAttacking = false;
    }
    
    public void Initialize(Transform target)
    {
        if (target == null || _agent == null || _stats == null)
        {
            Debug.LogError($"[{name}] Monster 초기화 참조 누락");
            return;
        }
        
        _defaultRotation = _modelRoot.localRotation;
        _target = target;

        _agent.SetVariableValue("Target", target.gameObject);
        _agent.SetVariableValue("AttackRange", _stats.AttackRange);
        _agent.SetVariableValue("Speed", _stats.MoveSpeed);
        _agent.SetVariableValue("AttackInterval", _stats.AttackInterval);
        
    }
    
    private void CacheComponents()
    {
        _health = GetComponent<Health>();
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _animHandler = GetComponent<MonsterAnimHandler>();
    }
    
}
