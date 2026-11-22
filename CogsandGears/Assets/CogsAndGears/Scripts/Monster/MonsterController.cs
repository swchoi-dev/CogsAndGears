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
    
    private Transform _target;
    private Health _health;
    private Quaternion _defaultRotation;

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        _health.OnDeath += Die;
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
    }
    
}
