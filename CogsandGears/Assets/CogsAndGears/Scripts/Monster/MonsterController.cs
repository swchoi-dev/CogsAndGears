using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class MonsterController : MonoBehaviour
{
    private const float DEFAULT_MODEL_ROTATION_ANGLE = 45f;
    
    [SerializeField] private StatDataSO _stats;
    [SerializeField] private BehaviorGraphAgent _agent;
    [SerializeField] private Transform _modelRoot;
    [SerializeField] private float _turnSpeed = 540f;
    [SerializeField] private bool _defaultFacingRight = true;
    
    [Header("Jump Attack")]
    [SerializeField] private float _jumpDuration = 0.8f;
    [SerializeField] private float _maxJumpDistance = 5f;
    
    private Transform _target;
    private Health _health;
    
    private Quaternion _defaultRotation;
    
    private Rigidbody _rb;
    
    public bool IsJumpAttacking { get; private set; }

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        _health.OnDeath += Die;
    }
    
    private void FixedUpdate()
    {
        if (!IsJumpAttacking) return;

        Debug.Log($"Jump Velocity: {_rb.linearVelocity}");
    }
    
    private void LateUpdate()
    {
        UpdateFacing();
    }

    private void OnDisable()
    {
        _health.OnDeath -= Die;
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

    public void JumpTowardTarget()
    {
        if (_target == null || _rb == null)
            return;

        float duration = Mathf.Max(0.1f, _jumpDuration);

        Vector3 start = _rb.position;
        Vector3 target = _target.position;

        float deltaX = Mathf.Clamp(
            target.x - start.x,
            -_maxJumpDistance,
            _maxJumpDistance
        );

        float velocityX = deltaX / duration;

        float velocityY =
            (target.y - start.y
                      - 0.5f * Physics.gravity.y * duration * duration)
            / duration;

        Vector3 currentVelocity = _rb.linearVelocity;

        Vector3 velocityChange = new Vector3(
            velocityX - currentVelocity.x,
            velocityY - currentVelocity.y,
            0f
        );

        IsJumpAttacking = true;
        _rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
    
    public void EndJumpAttack()
    {
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
    }
    
}
