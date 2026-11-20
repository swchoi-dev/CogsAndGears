using UnityEngine;

[RequireComponent(typeof(Health))]
public class MonsterController : MonoBehaviour
{
    private Health _health;
    
    private void Awake() => CacheComponents();

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
    
    private void CacheComponents()
    {
        _health = GetComponent<Health>();
    }
    
}
