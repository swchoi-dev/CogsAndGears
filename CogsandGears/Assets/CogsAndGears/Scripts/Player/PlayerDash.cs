using System;
using System.Collections;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [SerializeField] private float _dashForce = 100f;
    [SerializeField] private float _dashDuration = 1f;
    [SerializeField] private float _reChargeDuration = 5f;
    [SerializeField] private int _maxDashCount = 1;
    [SerializeField] private Transform _modelRoot;
    
    private PlayerMovement _playerMovement;
    private PlayerAim _playerAim;
    private Rigidbody2D _rigidbody2D;
    
    private float _dashDirection;
    private bool _isDashRequested;
    private bool _isDashing;
    private int _currentDashCount;
    
    private bool CanDash => !_isDashing && _currentDashCount > 0;
    public bool IsDashing => _isDashing;
    
    public event Action DashStarted;
    public event Action DashFinished;

    private void Awake() => CacheComponents();
    
    private void Start() => InitDash();
    
    private void Update()
    {
        UpdateDashDirection(PlayerInputHandler.Instance.MoveX);
        
        if (!PlayerInputHandler.Instance.IsDashPressed) return;
        
        if (!TryStartDash()) return;
        
        DashStarted?.Invoke();
    }
    
    private void FixedUpdate()
    {
        if (_isDashRequested)
        {
            float dashAmount = PlayerInputHandler.Instance.MoveX == 0 ? _dashForce + _playerMovement.MoveSpeed : _dashForce;
            _rigidbody2D.AddForce(Vector2.right * (_dashDirection * dashAmount), ForceMode2D.Impulse);
            _isDashRequested = false;
        }
    }

    private void UpdateDashDirection(float moveX)
    {
        _dashDirection = (Mathf.Abs(moveX) > 0.01f) ? Mathf.Sign(moveX) : _playerAim.FacingDirection;
    }

    private void RestoreDash()
    {
        _currentDashCount = Mathf.Clamp(_currentDashCount + 1, 0, _maxDashCount);
    }
    
    private IEnumerator DashCoroutine()
    {
        yield return new WaitForSeconds(_dashDuration);

        _isDashing = false;
        DashFinished?.Invoke();
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
        
        _modelRoot.localRotation = Quaternion.Euler(
            0f,
            _dashDirection > 0f ? 90f : -90f,
            0f
        );
        
        StartCoroutine(DashCoroutine());
        StartCoroutine(ReChargeCoroutine());

        return true;
    }
    
    private void InitDash()
    {
        _dashDirection = 1f;
        _isDashRequested = false;
        _isDashing = false;
        _currentDashCount = _maxDashCount;
    }
    
    private void CacheComponents()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerAim = GetComponent<PlayerAim>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }
}
