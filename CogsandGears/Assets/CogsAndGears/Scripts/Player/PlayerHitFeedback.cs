using UnityEngine;

// 플레이어가 실제로 피해를 받으면(Health.OnDamaged) 카메라 흔들림과 짧은 슬로우 모션을 재생한다.
// - 흔들림: 기존 CameraShake의 Animation 클립을 Unscaled Time으로 직접 샘플링해 슬로우 중에도 같은 속도로 재생하고,
//   끝나면 CameraShake의 원래 로컬 위치·회전으로 되돌린다. (CameraRoot 추적과 분리된 하위 계층만 움직인다)
// - 슬로우: timeScale·fixedDeltaTime을 낮췄다가 실제 시간 기준으로 복구한다. 정지(timeScale 0) 중에는 시작하지 않고,
//   슬로우 도중 다른 시스템이 timeScale을 바꿨다면 그 값을 덮어쓰지 않는다.
public class PlayerHitFeedback : MonoBehaviour
{
    [SerializeField] private Health _playerHealth;

    [Header("Camera Shake")]
    [Tooltip("기존 CameraShake 오브젝트의 Animation (Shake 클립)")]
    [SerializeField] private Animation _cameraShake;

    [Header("Slow Motion")]
    [Range(0.05f, 1f)]
    [SerializeField] private float _slowTimeScale = 0.35f;
    [Tooltip("슬로우 지속 시간(실제 시간, 초). 연속 피격 시 마지막 피격 기준으로 연장된다")]
    [SerializeField] private float _slowDurationRealtime = 0.12f;

    private Transform _shakeTransform;
    private AnimationClip _shakeClip;
    private Vector3 _shakeRestPosition;
    private Quaternion _shakeRestRotation;
    private float _shakeElapsed = -1f;

    private bool _isSlowing;
    private float _slowEndRealtime;
    private float _savedTimeScale;
    private float _savedFixedDeltaTime;

    private void Awake()
    {
        if (_cameraShake == null) return;

        _shakeTransform = _cameraShake.transform;
        _shakeClip = _cameraShake.clip;
        _shakeRestPosition = _shakeTransform.localPosition;
        _shakeRestRotation = _shakeTransform.localRotation;
    }

    private void OnEnable()
    {
        if (_playerHealth != null)
            _playerHealth.OnDamaged += OnPlayerDamaged;
    }

    private void OnDisable()
    {
        if (_playerHealth != null)
            _playerHealth.OnDamaged -= OnPlayerDamaged;

        // 비활성화·Scene 전환 시 연출 상태를 남기지 않는다
        StopShake();
        EndSlowMotion();
    }

    private void Update()
    {
        UpdateShake(Time.unscaledDeltaTime);

        if (_isSlowing && Time.unscaledTime >= _slowEndRealtime)
            EndSlowMotion();
    }

    private void OnPlayerDamaged(float damage)
    {
        StartShake();
        StartSlowMotion();
    }

    private void StartShake()
    {
        if (_shakeClip == null) return;

        _shakeElapsed = 0f;
        _shakeClip.SampleAnimation(_shakeTransform.gameObject, 0f);
    }

    private void UpdateShake(float deltaTime)
    {
        if (_shakeElapsed < 0f) return;

        _shakeElapsed += deltaTime;

        if (_shakeElapsed >= _shakeClip.length)
        {
            StopShake();
            return;
        }

        _shakeClip.SampleAnimation(_shakeTransform.gameObject, _shakeElapsed);
    }

    private void StopShake()
    {
        if (_shakeTransform == null) return;

        _shakeElapsed = -1f;
        _shakeTransform.localPosition = _shakeRestPosition;
        _shakeTransform.localRotation = _shakeRestRotation;
    }

    private void StartSlowMotion()
    {
        if (!_isSlowing)
        {
            // 정지 중이거나 이미 더 느린 상태면 시간 배율을 건드리지 않는다
            if (Time.timeScale <= 0f || Time.timeScale <= _slowTimeScale) return;

            _isSlowing = true;
            _savedTimeScale = Time.timeScale;
            _savedFixedDeltaTime = Time.fixedDeltaTime;
            Time.timeScale = _slowTimeScale;
            Time.fixedDeltaTime = _savedFixedDeltaTime * _slowTimeScale;
        }

        // 연속 피격은 타이머만 연장한다(저장값을 덮어쓰지 않음)
        _slowEndRealtime = Time.unscaledTime + _slowDurationRealtime;
    }

    private void EndSlowMotion()
    {
        if (!_isSlowing) return;

        _isSlowing = false;
        Time.fixedDeltaTime = _savedFixedDeltaTime;

        // 슬로우 도중 다른 시스템(인벤토리 정지 등)이 timeScale을 바꿨다면 그 값을 유지한다
        if (Mathf.Approximately(Time.timeScale, _slowTimeScale))
            Time.timeScale = _savedTimeScale;
    }
}
