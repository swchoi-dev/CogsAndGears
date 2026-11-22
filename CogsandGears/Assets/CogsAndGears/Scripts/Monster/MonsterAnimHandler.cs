using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MonsterAnimHandler : MonoBehaviour
{
    private static readonly int IsMoving =
        Animator.StringToHash("IsMoving");

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
}