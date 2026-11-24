using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : SingletonBehaviour<PlayerInputHandler>
{
    [SerializeField] private InputActionReference _moveInput;
    [SerializeField] private InputActionReference _dashInput;
    [SerializeField] private InputActionReference _fireInput;
    [SerializeField] private InputActionReference _rotateInput;
    
    private float _moveX;
    private Vector2 _moveVector2;
    private bool _isDashPressed;
    private bool _isFirePressed;
    private bool _isRotatePressed;
    // 정지·인벤토리 중 눌린 발사(예: 재개 버튼 클릭)가 재개 직후 발사로 이어지지 않도록, 한 번 뗄 때까지 무시한다
    private bool _waitFireRelease;

    // 전투 조작(이동·대시·발사)은 PLAYING에서만 허용한다. 회전(인벤토리용)은 제한하지 않는다.
    private bool CanControl =>
        GameFlowManager.Instance == null || GameFlowManager.Instance.FlowState == GameFlowState.PLAYING;

    public float MoveX => CanControl ? _moveX : 0f;
    public Vector2 MoveVector => CanControl ? _moveVector2 : Vector2.zero;
    public bool IsDashPressed => CanControl && _dashInput.action.WasPressedThisFrame();

    public bool IsFirePressed
    {
        get
        {
            if (!CanControl)
            {
                if (_isFirePressed) _waitFireRelease = true;
                return false;
            }

            return _isFirePressed && !_waitFireRelease;
        }
    }
    public bool ISRotatePressed => _rotateInput.action.WasPressedThisFrame();
    
    private void OnEnable()
    {
        _moveInput.action.Enable();
        _dashInput.action.Enable();
        _fireInput.action.Enable();
        _rotateInput.action.Enable();
        
        _moveInput.action.performed += OnMove;
        _moveInput.action.canceled += OnMove;
        
        _fireInput.action.performed += OnFire;
        _fireInput.action.canceled += OnFire;
    }

    private void OnDisable()
    {
        _moveInput.action.performed -= OnMove;
        _moveInput.action.canceled -= OnMove;
        
        _fireInput.action.performed -= OnFire;
        _fireInput.action.canceled -= OnFire;
        
        _moveInput.action.Disable();
        _dashInput.action.Disable();
        _fireInput.action.Disable();
        _rotateInput.action.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveVector2 = context.ReadValue<Vector2>();
        _moveX = _moveVector2.x;
    }
    
    private void OnFire(InputAction.CallbackContext context)
    {
        _isFirePressed = context.performed;

        if (!_isFirePressed)
            _waitFireRelease = false;
    }
}
