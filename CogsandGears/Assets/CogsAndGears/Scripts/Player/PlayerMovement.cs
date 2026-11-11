using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _dashAction;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _dashForce = 100f;
    [SerializeField] private float _dashDuration = 1f;
    [SerializeField] private float _reChargeDuration = 5f;
    [SerializeField] private int _maxDashCount = 1;
    
    private Rigidbody2D _rigidbody;
    private Vector2 _moveInput;
    
    private float _facingDirection;
    private float _dashDirection;
    private bool _isDashRequested;
    private bool _isDashing;
    private int _currentDashCount;

    private bool CanDash => !_isDashing && _currentDashCount > 0; 
    
    private void Awake() => CacheComponents();

    private void Start() => InitMovement();
    
    private void FixedUpdate()
    {
        if (_isDashRequested)
        {
            Debug.Log(
                $"Dash BEFORE | Force: {_dashForce}, " +
                $"Mass: {_rigidbody.mass}, " +
                $"Velocity: {_rigidbody.linearVelocity}"
            );
            
            _rigidbody.AddForce(Vector2.right * (_dashDirection * _dashForce), ForceMode2D.Impulse);
            _isDashRequested = false;
        }
        
        if (_isDashing) return;
        
        _rigidbody.linearVelocity = new Vector2(_moveInput.x * _moveSpeed, _rigidbody.linearVelocity.y);
    }
    
    private void OnEnable()
    {
        _moveAction.action.Enable();
        _dashAction.action.Enable();
        _moveAction.action.performed += OnMove;
        _moveAction.action.canceled += OnMove;
        _dashAction.action.performed += OnDash;
    }

    private void OnDisable()
    {
        _moveAction.action.performed -= OnMove;
        _moveAction.action.canceled -= OnMove;
        _dashAction.action.performed -= OnDash;
        _moveAction.action.Disable();
        _dashAction.action.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();

        if (Mathf.Abs(_moveInput.x) > 0.01f)
        {
            _facingDirection = Mathf.Sign(_moveInput.x);
        }
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (!CanDash) return;

        Debug.Log($"Dash START: {Time.time:F2}, Count: {_currentDashCount}");

        
        _currentDashCount -= 1;
        
        _dashDirection = _facingDirection;
        _isDashRequested = true;
        _isDashing = true;
        
        StartCoroutine(DashCoroutine());
        StartCoroutine(ReChargeCoroutine());
    }

    private void RestoreDash()
    {
        _currentDashCount = Mathf.Clamp(_currentDashCount + 1, 0, _maxDashCount);
        
        Debug.Log($"Dash RESTORED: {Time.time:F2}, Count: {_currentDashCount}");
    }
    
    private IEnumerator DashCoroutine()
    {
        yield return new WaitForSeconds(_dashDuration);

        _isDashing = false;
    }

    private IEnumerator ReChargeCoroutine()
    {
        yield return new WaitForSeconds(_reChargeDuration);
        
        RestoreDash();
    }

    private void InitMovement()
    {
        _facingDirection = 1f;
        _dashDirection = _facingDirection;
        _isDashRequested = false;
        _isDashing = false;
        _currentDashCount = _maxDashCount;
    }
    
    private void CacheComponents()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }
}
