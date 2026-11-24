using UnityEngine;

// 몬스터 머리 위 World Space HP 바. 부모의 Health를 구독해 게이지만 갱신한다.
// 평소에는 숨기고, HP가 이전 값보다 줄었을 때(피격)만 _visibleDuration초 동안 표시한다. 추가 피격 시 시간을 다시 센다.
// 매 프레임 추적 대상 위 _heightOffset 위치로 이동하고, 모델 회전과 무관하게 카메라 정면을 유지한다.
// 추적 대상: _followTarget 지정값 → Humanoid면 Head 본(점프 애니메이션 추적) → 몬스터 루트 순.
public class MonsterHealthBar : MonoBehaviour
{
    [SerializeField] private GaugeUI _gauge;
    [Tooltip("비워두면 Humanoid는 Head 본, 그 외는 몬스터 루트를 따라간다")]
    [SerializeField] private Transform _followTarget;
    [Tooltip("추적 대상 위치에서 위로 띄울 월드 높이")]
    [SerializeField] private float _heightOffset = 1f;
    [Tooltip("피격 후 HP 바를 표시하는 시간(초)")]
    [SerializeField] private float _visibleDuration = 2f;

    private Health _health;
    private Transform _cameraTransform;
    private Canvas _canvas;
    private float _lastHp;
    private float _hideTime;

    private void Awake()
    {
        _health = GetComponentInParent<Health>();
        // GameObject를 끄면 추적·이벤트가 멈추므로 표시는 Canvas만 켜고 끈다
        _canvas = GetComponent<Canvas>();

        if (_followTarget == null)
            _followTarget = FindDefaultTarget();

        if (Camera.main != null)
            _cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        if (_health == null) return;

        _health.OnHpChanged += OnHpChanged;
        _health.OnDeath += OnDeath;

        // 활성화 시에는 실제 HP만 동기화하고 표시하지 않는다(Health.Start 초기화 이벤트도 증가로 처리돼 표시되지 않는다)
        _lastHp = _health.CurrentHp;
        SetVisible(false);
        Refresh();
    }

    private void OnDisable()
    {
        if (_health == null) return;

        _health.OnHpChanged -= OnHpChanged;
        _health.OnDeath -= OnDeath;
    }

    private void LateUpdate()
    {
        if (_canvas.enabled && Time.time >= _hideTime)
            SetVisible(false);

        if (_followTarget != null)
            transform.position = _followTarget.position + Vector3.up * _heightOffset;

        if (_cameraTransform != null)
            transform.rotation = _cameraTransform.rotation;
    }

    private Transform FindDefaultTarget()
    {
        Animator animator = transform.parent != null ? transform.parent.GetComponentInChildren<Animator>() : null;

        if (animator != null && animator.isHuman)
        {
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            if (head != null) return head;
        }

        return transform.parent;
    }

    private void OnHpChanged(float currentHp)
    {
        // 감소(피격)일 때만 표시 시간을 처음부터 다시 센다. 회복·초기화는 게이지만 갱신한다.
        if (currentHp < _lastHp)
        {
            _hideTime = Time.time + _visibleDuration;
            SetVisible(true);
        }

        _lastHp = currentHp;
        Refresh();
    }

    // 몬스터 오브젝트 Destroy 전에 즉시 숨긴다
    private void OnDeath()
    {
        SetVisible(false);
        gameObject.SetActive(false);
    }

    private void SetVisible(bool visible)
    {
        _canvas.enabled = visible;
    }

    private void Refresh()
    {
        _gauge.SetValue(_health.CurrentHp, _health.MaxHp);
    }
}
