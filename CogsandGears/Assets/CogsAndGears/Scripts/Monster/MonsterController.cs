using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class MonsterController : MonoBehaviour
{
    // 도약 직후 아직 지면과 닿아 있는 접촉을 착지로 오판하지 않기 위한 최소 체공 시간
    private const float MIN_AIRBORNE_SECONDS = 0.1f;
    // 이 값보다 빠르게 상승 중이면 착지로 보지 않는다
    private const float LANDING_MAX_VERTICAL_SPEED = 0.1f;
    // 트리거 후 Animator가 JumpAttack 상태로 진입할 때까지 기다리는 시간
    private const float ATTACK_START_GRACE_SECONDS = 0.5f;
    // 예정 체공 시간을 이만큼 넘겨도 착지하지 못하면 피해 없이 점프를 종료한다
    private const float AIRBORNE_TIMEOUT_EXTRA_SECONDS = 1f;
    // 정점은 도약·착지 지점 중 높은 쪽보다 최소 이만큼 위에 둔다
    private const float MIN_JUMP_APEX_MARGIN = 0.1f;
    
    [SerializeField] private StatDataSO _stats;
    [SerializeField] private BehaviorGraphAgent _agent;
    [SerializeField] private Transform _modelRoot;
    [SerializeField] private float _turnSpeed = 540f;
    [Tooltip("플레이어가 오른쪽에 있을 때 모델의 월드 Y 회전각. 모델의 전방 축에 맞춰 몬스터별로 설정한다.")]
    [SerializeField] private float _rightFacingAngle = 135f;
    [Tooltip("플레이어가 왼쪽에 있을 때 모델의 월드 Y 회전각")]
    [SerializeField] private float _leftFacingAngle = 225f;
    [SerializeField] private bool _defaultFacingRight = true;
    
    [Header("Jump Attack")]
    [Tooltip("도약부터 착지까지의 체공 시간(초). JumpAttack 클립의 도약→착지 이벤트 간격과 맞춘다.")]
    [SerializeField] private float _jumpDuration = 1.3f;
    [Tooltip("도약 지점(발) 기준 정점 높이. 체공 시간(_jumpDuration)은 유지하고 점프 중 유효 중력을 바꿔 맞춘다")]
    [SerializeField] private float _jumpHeight = 1.8f;
    [SerializeField] private float _maxJumpDistance = 5f;
    [Tooltip("착지 지점(발 위치)으로부터 XY 평면에서 피해를 주는 반경")]
    [SerializeField] private float _landingAttackRadius = 2f;
    [Tooltip("접촉 법선의 Y값이 이 값 이상이어야 지면 착지로 판정한다 (벽 충돌 제외)")]
    [SerializeField] private float _groundNormalMinY = 0.7f;

    [Header("Jump Attack Foot Grounding")]
    [Tooltip("JumpAttack 중 지면에 있을 때(준비·착지 자세) 발이 바닥에 닿도록 ModelRoot의 보이는 높이만 보정한다")]
    [SerializeField] private bool _groundFeetDuringJumpAttack = true;
    [Tooltip("Idle 자세에서 발 본(발목)이 Collider 바닥보다 높은 거리. 모델마다 다르다")]
    [SerializeField] private float _footBoneHeight = 0.06f;
    [Tooltip("보정량이 목표값을 따라가는 속도(유닛/초). 도약·복귀 시 튀지 않도록 부드럽게 바꾼다")]
    [SerializeField] private float _footOffsetSpeed = 10f;
    [SerializeField] private float _maxFootOffset = 1f;
    [Tooltip("착지 예상 지점 경고. 폭은 착지 피해 반경(_landingAttackRadius × 2)")]
    [SerializeField] private AttackTelegraph _telegraphPrefab;
    
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
    private AttackTelegraph _landingTelegraph;
    // 점프 중 Physics.gravity에 더할 추가 수직 가속도(정점 높이·체공 시간을 맞추기 위한 유효 중력 보정)
    private float _jumpExtraGravity;
    // JumpAttack 시작 시 확정한 바라볼 방향(X 부호). 공격이 끝날 때까지 플레이어를 따라 돌지 않는다
    private float _lockedFacingX;
    private Vector3 _modelRootBaseLocalPosition;
    private float _footOffset;
    // 착지 프레임에는 보정을 즉시 적용해 접지 순간 발이 바로 땅에 닿게 한다
    private bool _snapFootOffset;
    private Transform _leftFoot;
    private Transform _rightFoot;
    
    // 공격 트리거부터 JumpAttack 애니메이션 종료까지 true. 이 동안 ChaseAction은 Rigidbody를 제어하지 않는다.
    public bool IsJumpAttacking { get; private set; }
    public StatDataSO Stats => _stats;

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
    
    private void FixedUpdate()
    {
        if (_isAirborne && _jumpExtraGravity != 0f)
            _rb.AddForce(Vector3.up * _jumpExtraGravity, ForceMode.Acceleration);
    }

    private void LateUpdate()
    {
        UpdateFacing();
        UpdateFootGrounding();
    }

    private void OnDisable()
    {
        _health.OnDeath -= Die;
        // 사망(Destroy)·비활성화 시 착지 경고를 남기지 않는다
        HideLandingTelegraph();
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

        // 점프 공격 중(준비~착지~종료)에는 시작 시 확정한 방향을 유지한다
        float directionX = IsJumpAttacking
            ? _lockedFacingX
            : _target.position.x - transform.position.x;

        Quaternion rightRotation = Quaternion.Euler(0f, _rightFacingAngle, 0f);
        Quaternion leftRotation = Quaternion.Euler(0f, _leftFacingAngle, 0f);

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
        _lockedFacingX = _target.position.x - transform.position.x;
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
        float targetFootY = GetTargetFootY();
        float deltaY = targetFootY - footY;

        // 수평: 등속. 수직: 정점 높이 apex와 체공 시간 duration을 동시에 만족하는 유효 중력 a와 초기 속도 vy를 구한다.
        //   apex = vy^2 / (2a),  deltaY = vy*T - 0.5*a*T^2  →  sqrt(a) = (sqrt(2*apex) + sqrt(2*(apex - deltaY))) / T
        // 실제 중력과의 차이는 체공 중 FixedUpdate에서 추가 가속도로 더한다.
        float velocityX = deltaX / duration;
        float apex = Mathf.Max(_jumpHeight, Mathf.Max(0f, deltaY) + MIN_JUMP_APEX_MARGIN);
        float sqrtGravity = (Mathf.Sqrt(2f * apex) + Mathf.Sqrt(2f * (apex - deltaY))) / duration;
        float effectiveGravity = sqrtGravity * sqrtGravity;
        float velocityY = Mathf.Sqrt(2f * effectiveGravity * apex);
        _jumpExtraGravity = -effectiveGravity - Physics.gravity.y;

        Vector3 currentVelocity = _rb.linearVelocity;
        Vector3 velocityChange = new Vector3(
            velocityX - currentVelocity.x,
            velocityY - currentVelocity.y,
            0f
        );

        _isAirborne = true;
        _takeoffTime = Time.time;
        _rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // 점프 계산과 같은 목표값(deltaX, targetFootY)으로 착지 예상 지점을 표시한다
        ShowLandingTelegraph(new Vector3(_rb.position.x + deltaX, targetFootY, _rb.position.z));
    }

    private void ShowLandingTelegraph(Vector3 landingPoint)
    {
        HideLandingTelegraph();

        if (_telegraphPrefab == null) return;

        _landingTelegraph = Instantiate(_telegraphPrefab, transform.parent);
        _landingTelegraph.Show(landingPoint, _landingAttackRadius * 2f);
    }

    private void HideLandingTelegraph()
    {
        if (_landingTelegraph == null) return;

        _landingTelegraph.Hide();
        _landingTelegraph = null;
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
        _jumpExtraGravity = 0f;
        _snapFootOffset = true;
        // 착지·타임아웃(applyDamage=false) 모두 경고를 제거한다
        HideLandingTelegraph();
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

    // JumpAttack 중 지면에 있는 동안 가장 낮은 발이 Collider 바닥(지면) + _footBoneHeight에 오도록 ModelRoot를 위아래로 옮긴다.
    // 준비·착지 자세는 원래 몸 전체가 내려앉는 동작인데, 클립의 Root Y를 버리면서(물리 점프와 이중 적용 방지)
    // 몸이 내려가지 않아 발이 뜨는 것을 보정한다. 공중·평상시에는 보정을 0으로 되돌린다. Collider·Rigidbody는 건드리지 않는다.
    private void UpdateFootGrounding()
    {
        if (!_groundFeetDuringJumpAttack || _leftFoot == null || _rightFoot == null)
            return;

        float targetOffset = 0f;

        if (IsJumpAttacking && !_isAirborne)
        {
            float lowestFoot = Mathf.Min(_leftFoot.position.y, _rightFoot.position.y);
            float desiredFoot = _collider.bounds.min.y + _footBoneHeight;
            targetOffset = Mathf.Clamp(_footOffset + (desiredFoot - lowestFoot), -_maxFootOffset, _maxFootOffset);
        }

        _footOffset = _snapFootOffset
            ? targetOffset
            : Mathf.MoveTowards(_footOffset, targetOffset, _footOffsetSpeed * Time.deltaTime);
        _snapFootOffset = false;
        _modelRoot.localPosition = _modelRootBaseLocalPosition + Vector3.up * _footOffset;
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

        if (_modelRoot != null)
            _modelRootBaseLocalPosition = _modelRoot.localPosition;

        // 발 접지 보정은 Humanoid 모델(Golem)에서만 동작한다
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null && animator.isHuman)
        {
            _leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            _rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }
    }
    
}
