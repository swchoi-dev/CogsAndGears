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
    private const float DEFAULT_MODEL_ROTATION_ANGLE = 45f;
    
    [SerializeField] private Camera _camera;
    
    [SerializeField] private Transform _modelRoot;
    [SerializeField] private Transform _aimOrigin;
    
    [SerializeField] private Transform _upperBodyTarget;
    [SerializeField] private Transform _armTarget;
    [SerializeField] private Transform _handTarget;
    
    [SerializeField] private float _turnSpeed = 540f;
    [SerializeField] private float _bodyRotateRange = 50f;
    [SerializeField] private float _armReach = 0.6f;
    
    [SerializeField] private TwoBoneIKConstraint _handIKConstraint;
    
    private PlayerDash _playerDash;
    private Quaternion _initHandRotation;
    
    public Vector2 AimDirection { get; private set; } = Vector2.right;
    public float FacingDirection { get; private set; } = 1f;

    private void Awake() => CacheComponents();

    private void OnEnable()
    {
        _playerDash.DashFinished += OnDashFinished;
        _playerDash.DashStarted += OnDashStarted;
    }

    private void OnDisable()
    {
        _playerDash.DashFinished -= OnDashFinished;
        _playerDash.DashStarted -= OnDashStarted;
    }
    
    private void Start() => Init();
    
    private void Update()
    {
        if (_playerDash.IsDashing) return;
        if (GameFlowManager.Instance.FlowState != GameFlowState.PLAYING) return;
        
        UpdateAim();
        UpdateFacing();
        UpdateAnimRig();
    }

    private void UpdateAnimRig()
    {
        UpdateUpperBody();
        UpdateRightArm();
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
            FacingDirection > 0f ?  DEFAULT_MODEL_ROTATION_ANGLE + 90f : DEFAULT_MODEL_ROTATION_ANGLE + 180f,
            0f
        );
        
        _modelRoot.rotation = Quaternion.RotateTowards(
            _modelRoot.rotation,
            targetRotation,
            _turnSpeed * Time.deltaTime
        );
    }

    private void UpdateUpperBody()
    {
        float angle = Mathf.Atan2(
            AimDirection.y,
            Mathf.Abs(AimDirection.x)
        ) * Mathf.Rad2Deg;

        angle = Mathf.Clamp(angle, _bodyRotateRange * -1, _bodyRotateRange);
        
        Quaternion targetRotation = Quaternion.Euler(-angle, 0f, 0f);

        _upperBodyTarget.localRotation = Quaternion.RotateTowards(
            _upperBodyTarget.localRotation,
            targetRotation,
            _turnSpeed * Time.deltaTime
        );
    }

    private void UpdateRightArm()
    {
        Vector3 targetPosition = _armTarget.position 
            + new Vector3(AimDirection.x, AimDirection.y, 0f) * _armReach;
        
        _handTarget.position = targetPosition;
        
        float handRotateAngle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
        float facingAngle = FacingDirection > 0f ? 0f : -180f;

        _handTarget.rotation = _initHandRotation
                                    * Quaternion.AngleAxis(handRotateAngle, Vector3.forward)
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
        _modelRoot.localRotation = Quaternion.AngleAxis(DEFAULT_MODEL_ROTATION_ANGLE, Vector3.up);
        _initHandRotation = _handTarget.localRotation;
    }
    
    private void CacheComponents()
    {
        _playerDash = GetComponent<PlayerDash>();
    }
}
