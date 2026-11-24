using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : MonoBehaviour, IInvulnerable
{
    [SerializeField] private float _dashForce = 100f;
    [SerializeField] private float _dashDuration = 1f;
    [SerializeField] private float _reChargeDuration = 5f;
    [SerializeField] private int _maxDashCount = 1;
    [SerializeField] private Transform _modelRoot;
    [Tooltip("대시 중(IsDashing) 피해를 받지 않는다. Health가 IInvulnerable로 확인한다")]
    [SerializeField] private bool _invulnerableWhileDashing = true;
    
    private PlayerMovement _playerMovement;
    private PlayerAim _playerAim;
    private Rigidbody _rigidbody;
    
    private float _dashDirection;
    private bool _isDashRequested;
    private bool _isDashing;
    private int _currentDashCount;
    // 사용한 대시가 충전될 시각(사용 순서대로). 충전 코루틴과 같은 순서로 빠진다.
    private readonly Queue<float> _restoreTimes = new Queue<float>();
    
    private bool CanDash => !_isDashing && _currentDashCount > 0;
    public bool IsDashing => _isDashing;
    public bool IsInvulnerable => _invulnerableWhileDashing && _isDashing;
    public int CurrentDashCount => _currentDashCount;
    public int MaxDashCount => _maxDashCount;

    // 다음 대시 1회가 충전되기까지의 진행도(0~1). 충전 대기 중인 대시가 없으면 1.
    public float RechargeProgress
    {
        get
        {
            if (_restoreTimes.Count == 0 || _reChargeDuration <= 0f) return 1f;

            return Mathf.Clamp01(1f - (_restoreTimes.Peek() - Time.time) / _reChargeDuration);
        }
    }
    
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
            _rigidbody.AddForce(Vector2.right * (_dashDirection * dashAmount), ForceMode.Impulse);
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
        
        if (_restoreTimes.Count > 0)
            _restoreTimes.Dequeue();

        RestoreDash();
    }

    private bool TryStartDash()
    {
        if (!CanDash) return false;
        
        _currentDashCount -= 1;
        _restoreTimes.Enqueue(Time.time + _reChargeDuration);
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
        _restoreTimes.Clear();
    }
    
    private void CacheComponents()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerAim = GetComponent<PlayerAim>();
        _rigidbody = GetComponent<Rigidbody>();
    }
}
