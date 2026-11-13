using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _dashAction;
    [SerializeField] private PlayerAnimation _playerAnimation;
    [SerializeField] private Transform _visualRoot;
    
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _dashForce = 100f;
    [SerializeField] private float _dashDuration = 1f;
    [SerializeField] private float _reChargeDuration = 5f;
    [SerializeField] private int _maxDashCount = 1;
    
    private PlayerAim _playerAim;
    private Rigidbody2D _rigidbody;
    private Vector2 _moveInput;
    
    private float _dashDirection;
    private bool _isDashRequested;
    private bool _isDashing;
    private int _currentDashCount;
    
    private bool CanDash => !_isDashing && _currentDashCount > 0;
    
    public event Action DashStarted;
    public bool IsDashing => _isDashing;
    
    private void Awake() => CacheComponents();

    private void Start() => InitMovement();

    private void Update()
    {
        float signedMoveX = _moveInput.x * _playerAim.FacingDirection;
        _playerAnimation.UpdateMovement(signedMoveX);
    }
    
    private void FixedUpdate()
    {
        if (_isDashRequested)
        {
            float dashAmount = _moveInput.x == 0 ? _dashForce + _moveSpeed : _dashForce;
            _rigidbody.AddForce(Vector2.right * (_dashDirection * dashAmount), ForceMode2D.Impulse);
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
            _dashDirection = Mathf.Sign(_moveInput.x);
        }
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (!TryStartDash()) return;
        DashStarted?.Invoke();
    }

    private void RestoreDash()
    {
        _currentDashCount = Mathf.Clamp(_currentDashCount + 1, 0, _maxDashCount);
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

    private bool TryStartDash()
    {
        if (!CanDash) return false;
        
        _currentDashCount -= 1;
        _isDashRequested = true;
        _isDashing = true;
        
        _visualRoot.localRotation = Quaternion.Euler(
            0f,
            _dashDirection > 0f ? 90f : -90f,
            0f
        );
        
        StartCoroutine(DashCoroutine());
        StartCoroutine(ReChargeCoroutine());

        return true;
    }
    
    
    private void InitMovement()
    {
        _dashDirection = 1f;
        _isDashRequested = false;
        _isDashing = false;
        _currentDashCount = _maxDashCount;
    }
    
    private void CacheComponents()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerAim = GetComponent<PlayerAim>();
    }
}
