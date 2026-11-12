using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string dashAnimName;
    [SerializeField] private string moveXParam;
    [SerializeField] private float _runThreshold = 2f;
    [SerializeField] private float _acceleration = 2.5f;
    [SerializeField] private float _deceleration = 5f;

    private PlayerMovement _playerMovement;
    private float _currentSpeed;

    private void Awake() => CacheComponents();
    
    private void OnEnable()
    {
        _playerMovement.DashStarted += OnDashStarted;
    }

    private void OnDisable()
    {
        _playerMovement.DashStarted -= OnDashStarted;
    }
        
    public void UpdateMovement(float moveX)
    {
        float targetSpeed = Mathf.Abs(moveX) * _runThreshold;
        
        float acceleration = targetSpeed > _currentSpeed
            ? _acceleration
            : _deceleration;

        _currentSpeed = Mathf.MoveTowards(
            _currentSpeed,
            targetSpeed,
            acceleration * Time.deltaTime
        );

        float signedSpeed = _currentSpeed * moveX;

        _animator.SetFloat(moveXParam, signedSpeed);
    }
    
    public void OnDashStarted()
    {
        _animator.Play(dashAnimName);
    }

    private void CacheComponents()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }
}
