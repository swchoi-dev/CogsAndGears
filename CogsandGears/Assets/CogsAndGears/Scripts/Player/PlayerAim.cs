using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerAim : MonoBehaviour
{
    private const float MIN_AIM_DISTANCE = 0.0001f;
    private const float MIN_FACING_THRESHOLD = 0.1f;
    private const float DEFAULT_HAND_ROTATION_ANGLE = 310f;
    
    [SerializeField] private Camera _camera;
    
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private Transform _aimOrigin;
    
    [SerializeField] private Transform _upperBodyTarget;
    [SerializeField] private Transform _rightArmTarget;
    [SerializeField] private Transform _rightHandTarget;
    
    [SerializeField] private float _turnSpeed = 540f;
    [SerializeField] private float _turnAngle = 50f;
    [SerializeField] private float _armReach = 0.6f;

    [SerializeField] private TwoBoneIKConstraint _handIKConstraint;
    
    private PlayerMovement _playerMovement;
    private Quaternion _initRightHandRotation;
    
    public Vector2 AimDirection { get; private set; } = Vector2.right;
    public float FacingDirection { get; private set; } = 1f;

    private void Awake() => CacheComponents();

    private void OnEnable()
    {
        _playerMovement.DashFinished += OnDashFinished;
        _playerMovement.DashStarted += OnDashStarted;
    }

    private void OnDisable()
    {
        _playerMovement.DashFinished -= OnDashFinished;
        _playerMovement.DashStarted -= OnDashStarted;
    }
    
    private void Start() => Init();
    
    private void Update()
    {
        UpdateAim();
        
        if (_playerMovement.IsDashing) return;
        
        UpdateFacing();
        UpdateUpperBodyAim();
        UpdateRightArmAim();
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
        if (Mathf.Abs(AimDirection.x) < MIN_FACING_THRESHOLD)
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

        angle = Mathf.Clamp(angle, _turnAngle * -1, _turnAngle);
        
        Quaternion targetRotation = Quaternion.Euler(-angle, 0f, 0f);

        _upperBodyTarget.localRotation = Quaternion.RotateTowards(
            _upperBodyTarget.localRotation,
            targetRotation,
            _turnSpeed * Time.deltaTime
        );
    }

    private void UpdateRightArmAim()
    {
        
        Vector3 targetPosition = _rightArmTarget.position 
            + new Vector3(AimDirection.x, AimDirection.y, 0f) * _armReach;
        
        _rightHandTarget.position = targetPosition;
        
        float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
        float facingAngle = FacingDirection > 0f ? 0f : -180f;

        _rightHandTarget.rotation = _initRightHandRotation
                                    * Quaternion.AngleAxis(angle, Vector3.forward)
                                    * Quaternion.AngleAxis(DEFAULT_HAND_ROTATION_ANGLE, Vector3.up)
                                    * Quaternion.AngleAxis(facingAngle, Vector3.up);
    }

    private void OnDashStarted()
    {
        _handIKConstraint.weight = 0f;
    }
    
    private void OnDashFinished()
    {
        _handIKConstraint.weight = 1f;
    }

    private void Init()
    {
        _visualRoot.localRotation = Quaternion.Euler(0f, 90f, 0f);
        _initRightHandRotation = _rightHandTarget.localRotation;
    }
    
    private void CacheComponents()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }
}
