using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class MonsterController : MonoBehaviour
{
    [SerializeField] private StatDataSO _stats;
    [SerializeField] private BehaviorGraphAgent _agent;
    private Health _health;

    private void Awake()
    {
        CacheComponents();
        InitBlackBoard();
    }

    private void OnEnable()
    {
        _health.OnDeath += Die;
    }

    private void OnDisable()
    {
        _health.OnDeath -= Die;
    }
    
    private void Die()
    {
        Destroy(gameObject);
    }

    private void InitBlackBoard()
    {
        _agent.SetVariableValue("AttackRange", _stats.AttackRange);
        _agent.SetVariableValue("Speed", _stats.MoveSpeed);
        _agent.SetVariableValue("AttackInterval", _stats.AttackInterval);
    }
    
    private void CacheComponents()
    {
        _health = GetComponent<Health>();
    }
    
}
