using UnityEngine;

public class PlayerAnimHandler : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string dashAnimName;
    [SerializeField] private string moveXParam;

    private PlayerDash _playerDash;

    private void Awake() => CacheComponents();
    
    private void OnEnable()
    {
        _playerDash.DashStarted += OnDashStarted;
    }

    private void OnDisable()
    {
        _playerDash.DashStarted -= OnDashStarted;
    }
        
    public void SetMoveSpeed(float speed)
    {
        _animator.SetFloat(moveXParam, speed);
    }
    
    public void OnDashStarted()
    {
        _animator.Play(dashAnimName);
    }

    private void CacheComponents()
    {
        _playerDash = GetComponent<PlayerDash>();
    }
}
