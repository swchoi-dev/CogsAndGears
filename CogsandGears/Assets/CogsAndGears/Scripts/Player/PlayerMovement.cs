using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _dashAction;
    [SerializeField] private Transform _modelRoot;
    
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _runThreshold = 2f;
    [SerializeField] private float _acceleration = 2.5f;
    [SerializeField] private float _deceleration = 5f;
    
    private Rigidbody _rigidbody;
    private PlayerAim _playerAim;
    private PlayerAnimHandler _playerAnim;
    private PlayerDash _playerDash;
    
    private Vector2 _moveInput;
    
    private float _currentSpeed;
    
    public float MoveSpeed => _moveSpeed;
    
    private void Awake() => CacheComponents();

    private void Update()
    {
        float signedMoveX = PlayerInputHandler.Instance.MoveX * _playerAim.FacingDirection;
        
        float targetSpeed = Mathf.Abs(signedMoveX) * _runThreshold;
        
        float acceleration = targetSpeed > _currentSpeed
            ? _acceleration
            : _deceleration;

        _currentSpeed = Mathf.MoveTowards(
            _currentSpeed,
            targetSpeed,
            acceleration * Time.deltaTime
        );

        float signedSpeed = _currentSpeed * signedMoveX;
        _playerAnim.SetMoveSpeed(signedSpeed);
    }
    
    private void FixedUpdate()
    {
        if (_playerDash.IsDashing) return;
        
        _rigidbody.linearVelocity = new Vector2(PlayerInputHandler.Instance.MoveX * _moveSpeed, _rigidbody.linearVelocity.y);
    }
    
    private void CacheComponents()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerAim = GetComponent<PlayerAim>();
        _playerAnim = GetComponent<PlayerAnimHandler>();
        _playerDash = GetComponent<PlayerDash>();
    }
}
