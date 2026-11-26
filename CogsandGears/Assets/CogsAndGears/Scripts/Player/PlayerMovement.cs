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

    private Rigidbody _rigidbody;
    private PlayerAim _playerAim;
    private PlayerAnimHandler _playerAnim;
    private PlayerDash _playerDash;

    private Vector2 _moveInput;

    public float MoveSpeed => _moveSpeed;

    private void Awake() => CacheComponents();

    private void Update()
    {
        // 바라보는 방향 기준 이동 방향: 1 = 전진(Run), -1 = 후진(RunBackward), 0 = 정지(Idle)
        float signedMoveX = PlayerInputHandler.Instance.MoveX * _playerAim.FacingDirection;
        _playerAnim.SetMoveSpeed(signedMoveX);
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
