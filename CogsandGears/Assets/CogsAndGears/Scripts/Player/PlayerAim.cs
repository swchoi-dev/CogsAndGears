using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerAim : MonoBehaviour
{
    private const float MIN_AIM_DISTANCE = 0.0001f;
    private const float MIN_FACING_THRESHOLD = 0.1f;
    private const float SIDE_FACING_ANGLE = 90f;
    // IK Target이 팔 길이 끝에 닿아 팔꿈치가 완전히 펴지지 않도록 남기는 비율
    private const float MAX_ARM_EXTENSION = 0.98f;

    [SerializeField] private Camera _camera;
    
    [SerializeField] private Transform _modelRoot;
    [SerializeField] private Transform _aimOrigin;
    
    [SerializeField] private Transform _upperBodyTarget;

    [SerializeField] private float _turnSpeed = 540f;
    [SerializeField] private float _bodyRotateRange = 50f;
    [Tooltip("수평 기준 최대 상하 조준 각도. 자세와 발사 방향 모두 이 각도로 제한한다")]
    [SerializeField] private float _maxAimAngle = 45f;
    [Tooltip("완전 측면(0°)에서 카메라 쪽으로 돌리는 기본 회전 각도. 이전 3/4뷰는 45°")]
    [SerializeField] private float _defaultModelRotationAngle = 0f;

    [Header("Arm Rig")]
    [Tooltip("오른팔·왼팔 상완 본(어깨 관절). 가슴 중심과 팔 길이 계산에 사용")]
    [SerializeField] private Transform _rightShoulder;
    [SerializeField] private Transform _leftShoulder;
    [SerializeField] private Transform _rightHand;
    [Tooltip("오른손에 장착된 무기 소켓. 원점이 RightHandGrip이며 +X = 총열, +Y = 총 위")]
    [SerializeField] private Transform _weaponSocket;
    [SerializeField] private Transform _leftHandGrip;
    [SerializeField] private Transform _rightHandTarget;
    [SerializeField] private Transform _rightElbowHint;
    [SerializeField] private Transform _leftElbowHint;
    [Tooltip("가슴 중심에서 조준 방향으로 오른손 파지점(RightHandGrip)까지의 거리")]
    [SerializeField] private float _armReach = 0.32f;
    [Tooltip("팔꿈치 Hint를 팔 중간에서 총 아래쪽·바깥쪽으로 띄우는 거리")]
    [SerializeField] private float _elbowHintDistance = 0.3f;
    [Tooltip("Dash 중에는 애니메이션 자세를 쓰도록 weight를 0으로 내리는 팔 IK")]
    [SerializeField] private TwoBoneIKConstraint[] _armIKConstraints;

    private PlayerDash _playerDash;
    // 오른손 기준 소켓(RightHandGrip)의 회전·위치. 시작 시 한 번 읽어 IK Target 계산에 사용한다(무기를 매 프레임 참조하지 않음)
    private Quaternion _handToSocketRotation;
    private Vector3 _handToSocketOffset;
    private float _rightArmLength;
    private float[] _armIKWeights;

    public Vector2 AimDirection { get; private set; } = Vector2.right;
    // 마우스 포인터의 월드(XY) 위치. 투사체는 총구에서 이 지점을 향해 발사된다.
    public Vector2 AimPoint { get; private set; }
    public float FacingDirection { get; private set; } = 1f;

    private void Awake() => CacheComponents();

    private void OnEnable()
    {
        _playerDash.DashStarted += OnDashStarted;
        _playerDash.DashFinished += OnDashFinished;
    }

    private void OnDisable()
    {
        _playerDash.DashStarted -= OnDashStarted;
        _playerDash.DashFinished -= OnDashFinished;
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

    // 평가 순서: 상체(범위 제한) → 오른팔 IK(조준 방향의 총 자세) → 오른손의 Rifle → LeftHandGrip → 왼팔 IK
    private void UpdateAnimRig()
    {
        UpdateUpperBody();
        UpdateArms();
    }
    
    private void UpdateAim()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        Vector2 mouseWorld = _camera.ScreenToWorldPoint(mouseScreen);
        AimPoint = mouseWorld;

        Vector2 direction = mouseWorld - (Vector2)_aimOrigin.position;

        if (direction.sqrMagnitude < MIN_AIM_DISTANCE)
            return;

        AimDirection = ClampAimDirection(direction);
    }

    // 상하 조준 각도를 _maxAimAngle 이내로 제한한 단위 방향. 수직에 가까우면 현재 바라보는 쪽을 유지한다.
    public Vector2 ClampAimDirection(Vector2 direction)
    {
        float sideX = Mathf.Abs(direction.x) > MIN_FACING_THRESHOLD ? Mathf.Sign(direction.x) : FacingDirection;
        float angle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;
        angle = Mathf.Clamp(angle, -_maxAimAngle, _maxAimAngle) * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(angle) * sideX, Mathf.Sin(angle));
    }

    private void UpdateFacing()
    {
        if (Mathf.Abs(AimDirection.x) > MIN_FACING_THRESHOLD)
        {
            FacingDirection = Mathf.Sign(AimDirection.x);
        }
        
        // 수직 조준 근처에서 좌우가 반복 전환되지 않도록 임계값이 적용된 FacingDirection으로 회전한다
        FacingUtility.RotateTowardsX(
            _modelRoot,
            FacingDirection,
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

    // 총이 조준 방향을 향하는 자세를 정하고, 그 자세가 되도록 오른손 IK Target을 둔다. 왼손은 Rig에서 LeftHandGrip을 따른다.
    private void UpdateArms()
    {
        Vector3 aim = new Vector3(AimDirection.x, AimDirection.y, 0f);
        // 총 위쪽: 조준 방향에 수직인 XY 방향. 좌우 어느 쪽을 봐도 총이 뒤집히지 않도록 FacingDirection을 곱한다
        Vector3 gunUp = new Vector3(-AimDirection.y, AimDirection.x, 0f) * FacingDirection;
        Quaternion gunRotation = Quaternion.LookRotation(Vector3.Cross(aim, gunUp), gunUp);

        Vector3 chest = (_rightShoulder.position + _leftShoulder.position) * 0.5f;
        Vector3 gripPosition = chest + aim * _armReach;

        Quaternion handRotation = gunRotation * Quaternion.Inverse(_handToSocketRotation);
        Vector3 handPosition = gripPosition - handRotation * _handToSocketOffset;

        Vector3 shoulderToHand = handPosition - _rightShoulder.position;
        float maxReach = _rightArmLength * MAX_ARM_EXTENSION;
        if (shoulderToHand.magnitude > maxReach)
            handPosition = _rightShoulder.position + shoulderToHand.normalized * maxReach;

        _rightHandTarget.SetPositionAndRotation(handPosition, handRotation);

        // 팔꿈치는 총 아래쪽(-gunUp)과 각 팔의 바깥쪽으로 굽힌다
        Vector3 outward = _modelRoot.right;
        _rightElbowHint.position = (_rightShoulder.position + handPosition) * 0.5f
                                   + (-gunUp + outward) * _elbowHintDistance;
        _leftElbowHint.position = (_leftShoulder.position + _leftHandGrip.position) * 0.5f
                                  + (-gunUp - outward) * _elbowHintDistance;
    }

    private void OnDashStarted() => SetArmIKWeights(false);

    private void OnDashFinished() => SetArmIKWeights(true);

    private void SetArmIKWeights(bool enabled)
    {
        for (int i = 0; i < _armIKConstraints.Length; i++)
            _armIKConstraints[i].weight = enabled ? _armIKWeights[i] : 0f;
    }

    // 모델 정면(+Z)이 오른쪽(+X)을 보는 90°를 측면 기준으로, 좌우 모두 카메라 쪽으로 _defaultModelRotationAngle만큼 돌린다
    private Quaternion GetRightFacingRotation() =>
        Quaternion.Euler(0f, SIDE_FACING_ANGLE + _defaultModelRotationAngle, 0f);

    private Quaternion GetLeftFacingRotation() =>
        Quaternion.Euler(0f, -SIDE_FACING_ANGLE - _defaultModelRotationAngle, 0f);

    private void Init()
    {
        _modelRoot.localRotation = GetRightFacingRotation();

        _handToSocketRotation = Quaternion.Inverse(_rightHand.rotation) * _weaponSocket.rotation;
        _handToSocketOffset = Quaternion.Inverse(_rightHand.rotation) * (_weaponSocket.position - _rightHand.position);

        Transform rightForeArm = _rightHand.parent;
        _rightArmLength = Vector3.Distance(_rightShoulder.position, rightForeArm.position)
                          + Vector3.Distance(rightForeArm.position, _rightHand.position);
    }

    private void CacheComponents()
    {
        _playerDash = GetComponent<PlayerDash>();

        _armIKWeights = new float[_armIKConstraints.Length];
        for (int i = 0; i < _armIKConstraints.Length; i++)
            _armIKWeights[i] = _armIKConstraints[i].weight;
    }
}
