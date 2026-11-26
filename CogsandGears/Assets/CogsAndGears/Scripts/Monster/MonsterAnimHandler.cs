using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MonsterAnimHandler : MonoBehaviour
{
    private static readonly int IsMoving =
        Animator.StringToHash("IsMoving");
    private static readonly int JumpAttackTrigger =
        Animator.StringToHash("JumpAttack");
    private static readonly int JumpAttackState =
        Animator.StringToHash("JumpAttack");
    private static readonly int AttackTrigger =
        Animator.StringToHash("Attack");
    private static readonly int AttackState =
        Animator.StringToHash("Attack");

    [SerializeField] private Animator _animator;
    [SerializeField] private float _moveThreshold = 0.1f;
    [Tooltip("공중 몬스터처럼 Y축 이동도 이동으로 볼 때 켠다. 끄면 X축 속도만 사용한다.")]
    [SerializeField] private bool _includeVerticalSpeed;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (_animator == null) return;

        Vector3 velocity = _rb.linearVelocity;
        float speed = _includeVerticalSpeed
            ? new Vector2(velocity.x, velocity.y).magnitude
            : Mathf.Abs(velocity.x);

        // JumpAttack 중 도약 속도는 이동으로 보지 않는다. 공격이 끝나야 Run으로 전환된다.
        _animator.SetBool(IsMoving, speed > _moveThreshold && !IsPlayingJumpAttack());
    }

    public void PlayJumpAttack()
    {
        if (_animator == null) return;

        _animator.SetTrigger(JumpAttackTrigger);
    }

    // JumpAttack 상태를 재생 중이거나 해당 상태로 전환 중이면 true.
    // JumpAttack은 종료 전환 없이 상태에 머무르므로 클립을 끝까지 재생했으면(normalizedTime >= 1) 종료로 본다.
    public bool IsPlayingJumpAttack()
    {
        if (_animator == null) return false;

        if (!_animator.IsInTransition(0))
        {
            AnimatorStateInfo current = _animator.GetCurrentAnimatorStateInfo(0);
            if (current.shortNameHash == JumpAttackState && current.normalizedTime >= 1f)
                return false;
        }

        return IsPlayingState(JumpAttackState);
    }

    public void PlayAttack()
    {
        if (_animator == null) return;

        _animator.SetTrigger(AttackTrigger);
    }

    // Attack 상태를 재생 중이거나 해당 상태로 전환 중이면 true
    public bool IsPlayingAttack()
    {
        return IsPlayingState(AttackState);
    }

    private bool IsPlayingState(int stateHash)
    {
        if (_animator == null) return false;

        if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash)
            return true;

        return _animator.IsInTransition(0)
               && _animator.GetNextAnimatorStateInfo(0).shortNameHash == stateHash;
    }
}
