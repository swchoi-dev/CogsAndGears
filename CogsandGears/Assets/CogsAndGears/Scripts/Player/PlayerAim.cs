using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerAim : MonoBehaviour
{
    private const float MIN_AIM_DISTANCE = 0.0001f;
    
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _aimOrigin;
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private Transform _upperBodyTarget;
    [SerializeField] private float _facingThreshold = 0.1f;
    [SerializeField] private float _aimDistance = 0.6f;
    [SerializeField] private float _turnSpeed = 540f;
    [SerializeField] private float _upperBodyTurnSpeed = 540f;
    [SerializeField] private float _minUpperAngle = -50f;
    [SerializeField] private float _maxUpperAngle = 50f;
    
    private PlayerMovement _playerMovement;
    
    public Vector2 AimDirection { get; private set; } = Vector2.right;
    public float FacingDirection { get; private set; } = 1f;

    private void Awake() => CacheComponents();

    private void Start() => Init();
    
    private void Update()
    {
        UpdateAim();
        UpdateFacing();
        UpdateUpperBodyAim();
    }

    private void UpdateAim()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        Vector2 mouseWorld = _camera.ScreenToWorldPoint(mouseScreen);

        Vector2 direction = mouseWorld - (Vector2)_aimOrigin.position;

        if (direction.sqrMagnitude < MIN_AIM_DISTANCE)
            return;

        AimDirection = direction.normalized;
    }

    private void UpdateFacing()
    {
        if (_playerMovement.IsDashing) return;
        
        if (Mathf.Abs(AimDirection.x) < _facingThreshold)
            return;

        FacingDirection = Mathf.Sign(AimDirection.x);

        Quaternion targetRotation = Quaternion.Euler(
            0f,
            FacingDirection > 0f ? 90f : -90f,
            0f
        );
        
        _visualRoot.rotation = Quaternion.RotateTowards(
            _visualRoot.rotation,
            targetRotation,
            _turnSpeed * Time.deltaTime
        );
    }

    private void UpdateUpperBodyAim()
    {
        float angle = Mathf.Atan2(
            AimDirection.y,
            Mathf.Abs(AimDirection.x)
        ) * Mathf.Rad2Deg;

        angle = Mathf.Clamp(angle, _minUpperAngle, _maxUpperAngle);
        
        Quaternion targetRotation = Quaternion.Euler(-angle, 0f, 0f);

        _upperBodyTarget.localRotation = Quaternion.RotateTowards(
            _upperBodyTarget.localRotation,
            targetRotation,
            _upperBodyTurnSpeed * Time.deltaTime
        );
    }

    private void Init()
    {
        _visualRoot.localRotation = Quaternion.Euler(0f, 90f, 0f);
    }
    
    private void CacheComponents()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }
}
