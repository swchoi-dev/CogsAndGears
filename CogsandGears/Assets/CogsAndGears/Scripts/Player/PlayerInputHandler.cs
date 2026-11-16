using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionReference _moveInput;
    [SerializeField] private InputActionReference _dashInput;

    private float _moveX;
    private Vector2 _moveVector2;
    private bool _isDashPressed;
    
    public float MoveX => _moveX;
    public Vector2 MoveVector => _moveVector2;
    public bool IsDashPressed => _isDashPressed;
    
    private void OnEnable()
    {
        _moveInput.action.Enable();
        _dashInput.action.Enable();
        
        _moveInput.action.performed += OnMove;
        _moveInput.action.canceled += OnMove;
        
        _dashInput.action.performed += OnDash;
        _dashInput.action.canceled += OnDash;
    }

    private void OnDisable()
    {
        _moveInput.action.performed -= OnMove;
        _moveInput.action.canceled -= OnMove;
        
        _dashInput.action.performed -= OnDash;
        _dashInput.action.canceled -= OnDash;
        
        _moveInput.action.Disable();
        _dashInput.action.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveVector2 = context.ReadValue<Vector2>();
        _moveX = _moveVector2.x;
    }

    private void OnDash(InputAction.CallbackContext context) => _isDashPressed = context.performed;
}
