using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerAim : MonoBehaviour
{
    private const float MIN_AIM_DISTANCE = 0.0001f;
    private const float MIN_FACING_THRESHOLD = 0.1f;
    private const float SIDE_FACING_ANGLE = 90f;
    
    [SerializeField] private Camera _camera;
    
    [SerializeField] private Transform _modelRoot;
    [SerializeField] private Transform _aimOrigin;
    
    [SerializeField] private Transform _upperBodyTarget;

    [SerializeField] private float _turnSpeed = 540f;
    [SerializeField] private float _bodyRotateRange = 50f;
    [Tooltip("완전 측면(0°)에서 카메라 쪽으로 돌리는 기본 회전 각도. 이전 3/4뷰는 45°")]
    [SerializeField] private float _defaultModelRotationAngle = 0f;

    private PlayerDash _playerDash;

    public Vector2 AimDirection { get; private set; } = Vector2.right;
    // 마우스 포인터의 월드(XY) 위치. 투사체는 총구에서 이 지점을 향해 발사된다.
    public Vector2 AimPoint { get; private set; }
    public float FacingDirection { get; private set; } = 1f;

    private void Awake() => CacheComponents();

    private void Start() => Init();
    
    private void Update()
    {
        if (_playerDash.IsDashing) return;
        if (GameFlowManager.Instance.FlowState != GameFlowState.PLAYING) return;
        
        UpdateAim();
        UpdateFacing();
        UpdateAnimRig();
    }

    // 팔·손 자세는 애니메이션이 담당하고, Rig는 상체 조준만 갱신한다
    private void UpdateAnimRig()
    {
        UpdateUpperBody();
    }
    
    private void UpdateAim()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        Vector2 mouseWorld = _camera.ScreenToWorldPoint(mouseScreen);
        AimPoint = mouseWorld;

        Vector2 direction = mouseWorld - (Vector2)_aimOrigin.position;

        if (direction.sqrMagnitude < MIN_AIM_DISTANCE)
            return;

        AimDirection = direction.normalized;
    }

    private void UpdateFacing()
    {
        if (Mathf.Abs(AimDirection.x) > MIN_FACING_THRESHOLD)
        {
            FacingDirection = Mathf.Sign(AimDirection.x);
        }
        
        FacingUtility.RotateTowardsX(
            _modelRoot,
            AimDirection.x,
            GetRightFacingRotation(),
            GetLeftFacingRotation(),
            _turnSpeed,
            false
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

    // 모델 정면(+Z)이 오른쪽(+X)을 보는 90°를 측면 기준으로, 좌우 모두 카메라 쪽으로 _defaultModelRotationAngle만큼 돌린다
    private Quaternion GetRightFacingRotation() =>
        Quaternion.Euler(0f, SIDE_FACING_ANGLE + _defaultModelRotationAngle, 0f);

    private Quaternion GetLeftFacingRotation() =>
        Quaternion.Euler(0f, -SIDE_FACING_ANGLE - _defaultModelRotationAngle, 0f);

    private void Init()
    {
        _modelRoot.localRotation = GetRightFacingRotation();
    }
    
    private void CacheComponents()
    {
        _playerDash = GetComponent<PlayerDash>();
    }
}
