using UnityEngine;

public class PlayerAnimHandler : MonoBehaviour
{
    // 이동 Blend Tree(Idle·Run·RunBackward) 전환이 튀지 않도록 파라미터를 감쇠한다
    private const float MOVE_SPEED_DAMP_TIME = 0.1f;

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
        _animator.SetFloat(moveXParam, speed, MOVE_SPEED_DAMP_TIME, Time.deltaTime);
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
