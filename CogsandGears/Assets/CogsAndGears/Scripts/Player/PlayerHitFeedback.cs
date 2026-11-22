using UnityEngine;

// 플레이어가 실제로 피해를 받으면(Health.OnDamaged) 카메라 흔들림과 Hit Stop → Slow Motion → 복귀 연출을 재생한다.
// - 흔들림: CameraShake의 Animation 클립을 Unscaled Time으로 직접 샘플링해 시간 연출 중에도 같은 속도로 재생하고,
//   끝나면 CameraShake의 원래 로컬 위치·회전으로 되돌린다. (CameraRoot 추적과 분리된 하위 계층만 움직인다)
// - 시간: 실제 시간 기준으로 HitStop(_hitStopTimeScale) → Slow(_slowTimeScale) → Recover(보간) 후 원래 값으로 복원한다.
//   fixedDeltaTime은 timeScale에 비례해 함께 바꿔 물리 갱신 빈도를 유지한다.
// - 충돌 방지: 정지(timeScale 0, 인벤토리) 중에는 시작하지 않고, 연출 도중 다른 시스템이 timeScale을 바꾸면
//   즉시 연출을 중단하고 그 값을 유지한다(fixedDeltaTime만 복원).
public class PlayerHitFeedback : MonoBehaviour
{
    private enum TimePhase
    {
        None,
        HitStop,
        Slow,
        Recover,
    }

    [SerializeField] private Health _playerHealth;

    [Header("Camera Shake")]
    [Tooltip("CameraShake 오브젝트의 Animation (Shake 클립)")]
    [SerializeField] private Animation _cameraShake;

    [Header("Hit Stop")]
    [Range(0.01f, 1f)]
    [SerializeField] private float _hitStopTimeScale = 0.05f;
    [Tooltip("실제 시간(초)")]
    [SerializeField] private float _hitStopDuration = 0.06f;

    [Header("Slow Motion")]
    [Range(0.05f, 1f)]
    [SerializeField] private float _slowTimeScale = 0.3f;
    [Tooltip("실제 시간(초)")]
    [SerializeField] private float _slowDuration = 0.18f;
    [Tooltip("원래 속도로 보간하는 시간(실제 시간, 초). 0이면 즉시 복귀")]
    [SerializeField] private float _recoverDuration = 0.05f;

    [Header("Retrigger")]
    [Tooltip("시간 연출 시작 후 다시 시작할 수 있을 때까지의 실제 시간(초). 연출 중 피격은 흔들림만 다시 재생한다")]
    [SerializeField] private float _retriggerCooldown = 0.35f;

    private Transform _shakeTransform;
    private AnimationClip _shakeClip;
    private Vector3 _shakeRestPosition;
    private Quaternion _shakeRestRotation;
    private float _shakeElapsed = -1f;

    private TimePhase _phase = TimePhase.None;
    private float _phaseStartRealtime;
    private float _nextTimeEffectRealtime;
    private float _appliedTimeScale;
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
        EndTimeEffect();
    }

    private void Update()
    {
        UpdateShake(Time.unscaledDeltaTime);
        UpdateTimeEffect();
    }

    private void OnPlayerDamaged(float damage)
    {
        // 흔들림은 피격마다 처음부터, 시간 연출은 중첩하지 않는다
        StartShake();
        TryStartTimeEffect();
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

    private void TryStartTimeEffect()
    {
        if (_phase != TimePhase.None) return;
        if (Time.unscaledTime < _nextTimeEffectRealtime) return;

        // 정지 중(인벤토리 등)이거나 이미 Hit Stop보다 느린 상태면 시간 배율을 건드리지 않는다
        if (Time.timeScale <= 0f || Time.timeScale <= _hitStopTimeScale) return;

        _savedTimeScale = Time.timeScale;
        _savedFixedDeltaTime = Time.fixedDeltaTime;
        _nextTimeEffectRealtime = Time.unscaledTime + _retriggerCooldown;

        EnterPhase(TimePhase.HitStop);
        ApplyTimeScale(_hitStopTimeScale);
    }

    private void UpdateTimeEffect()
    {
        if (_phase == TimePhase.None) return;

        // 연출 도중 다른 시스템이 timeScale을 바꿨다면 그 값을 존중하고 연출을 중단한다
        if (!Mathf.Approximately(Time.timeScale, _appliedTimeScale))
        {
            EndTimeEffect();
            return;
        }

        float elapsed = Time.unscaledTime - _phaseStartRealtime;

        switch (_phase)
        {
            case TimePhase.HitStop:
                if (elapsed < _hitStopDuration) return;

                EnterPhase(TimePhase.Slow);
                ApplyTimeScale(_slowTimeScale);
                break;

            case TimePhase.Slow:
                if (elapsed < _slowDuration) return;

                if (_recoverDuration <= 0f)
                {
                    EndTimeEffect();
                    return;
                }

                EnterPhase(TimePhase.Recover);
                break;

            case TimePhase.Recover:
                float t = Mathf.Clamp01(elapsed / _recoverDuration);
                if (t >= 1f)
                {
                    EndTimeEffect();
                    return;
                }

                ApplyTimeScale(Mathf.Lerp(_slowTimeScale, _savedTimeScale, t));
                break;
        }
    }

    private void EnterPhase(TimePhase phase)
    {
        _phase = phase;
        _phaseStartRealtime = Time.unscaledTime;
    }

    private void ApplyTimeScale(float timeScale)
    {
        _appliedTimeScale = timeScale;
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = _savedFixedDeltaTime * timeScale;
    }

    // fixedDeltaTime은 항상 원래 값으로, timeScale은 이 연출이 마지막으로 설정한 값일 때만 원래 값으로 되돌린다
    private void EndTimeEffect()
    {
        if (_phase == TimePhase.None) return;

        _phase = TimePhase.None;
        Time.fixedDeltaTime = _savedFixedDeltaTime;

        if (Mathf.Approximately(Time.timeScale, _appliedTimeScale))
            Time.timeScale = _savedTimeScale;
    }
}
