using UnityEngine;

[CreateAssetMenu(
    fileName = "StatDataSO",
    menuName = "CogsAndGears/Stat Data"
)]
public class StatDataSO : ScriptableObject
{
    [Header("Common Stats")]
    [SerializeField] private int _maxHealth;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _turnSpeed;
    
    [Header("Monster Stats")]
    [SerializeField] private int _damage;
    [SerializeField] private float _attackInterval;
    [SerializeField] private float _attackRange;
    
    public int MaxHealth => _maxHealth;
    public float MoveSpeed => _moveSpeed;
    public float TurnSpeed => _turnSpeed;
    
    public int Damage => _damage;
    public float AttackInterval => _attackInterval;
    public float AttackRange => _attackRange;
}
