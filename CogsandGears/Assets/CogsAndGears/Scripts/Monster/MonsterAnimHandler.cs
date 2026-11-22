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

    [SerializeField] private Animator _animator;
    [SerializeField] private float _moveThreshold = 0.1f;

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

        bool isMoving =
            Mathf.Abs(_rb.linearVelocity.x) > _moveThreshold;

        _animator.SetBool(IsMoving, isMoving);
    }

    public void PlayJumpAttack()
    {
        if (_animator == null) return;

        _animator.SetTrigger(JumpAttackTrigger);
    }

    // JumpAttack 상태를 재생 중이거나 해당 상태로 전환 중이면 true
    public bool IsPlayingJumpAttack()
    {
        if (_animator == null) return false;

        if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash == JumpAttackState)
            return true;

        return _animator.IsInTransition(0)
               && _animator.GetNextAnimatorStateInfo(0).shortNameHash == JumpAttackState;
    }
}