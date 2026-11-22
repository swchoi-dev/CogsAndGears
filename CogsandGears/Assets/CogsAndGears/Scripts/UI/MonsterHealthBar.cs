using UnityEngine;

// 몬스터 머리 위 World Space HP 바. 부모의 Health를 구독해 게이지만 갱신한다.
// 매 프레임 추적 대상 위 _heightOffset 위치로 이동하고, 모델 회전과 무관하게 카메라 정면을 유지한다.
// 추적 대상: _followTarget 지정값 → Humanoid면 Head 본(점프 애니메이션 추적) → 몬스터 루트 순.
public class MonsterHealthBar : MonoBehaviour
{
    [SerializeField] private GaugeUI _gauge;
    [Tooltip("비워두면 Humanoid는 Head 본, 그 외는 몬스터 루트를 따라간다")]
    [SerializeField] private Transform _followTarget;
    [Tooltip("추적 대상 위치에서 위로 띄울 월드 높이")]
    [SerializeField] private float _heightOffset = 1f;

    private Health _health;
    private Transform _cameraTransform;

    private void Awake()
    {
        _health = GetComponentInParent<Health>();

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

    private void OnHpChanged(float currentHp) => Refresh();

    // 몬스터 오브젝트 Destroy 전에 먼저 숨긴다
    private void OnDeath() => gameObject.SetActive(false);

    private void Refresh()
    {
        _gauge.SetValue(_health.CurrentHp, _health.MaxHp);
    }
}
