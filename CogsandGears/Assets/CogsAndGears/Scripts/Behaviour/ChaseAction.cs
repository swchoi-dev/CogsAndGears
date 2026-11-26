using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase", story: "Chase [Target] at [Speed]", category: "Enemy", id: "70258839bb1548405ff7b4b8e373a966")]
public partial class ChaseAction : Action
{
    private const float ARRIVE_DISTANCE_X = 0.1f;

    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Speed;
    [SerializeReference] public BlackboardVariable<float> AttackRange;

    private Rigidbody _rb;
    private MonsterController _monster;

    protected override Status OnStart()
    {
        _rb = GameObject.GetComponent<Rigidbody>();
        _monster = GameObject.GetComponent<MonsterController>();

        return _rb != null ? Status.Running : Status.Failure;
    }

    protected override Status OnUpdate()
    {
        if (Target.Value == null)
            return Status.Failure;

        // 점프 공격 중에는 Rigidbody 속도를 건드리지 않는다
        if (IsJumpAttacking())
            return Status.Failure;

        float deltaX =
            Target.Value.transform.position.x - GameObject.transform.position.x;

        // 그래프의 공격 조건(Distance, Transform 간 3D 거리)과 같은 기준으로 도착을 판단한다.
        // 수평 거리만 쓰면 사거리 경계에서 높이 차 때문에 공격 조건을 만족하지 못한 채 멈춘다.
        if (Vector3.Distance(Target.Value.transform.position, GameObject.transform.position) <= AttackRange.Value)
        {
            return Status.Success;
        }

        // 수평으로는 도착했지만 높이 차로 사거리 밖이면 제자리에서 기다린다(좌우 왕복 방지)
        Vector3 velocity = _rb.linearVelocity;
        velocity.x = Mathf.Abs(deltaX) > ARRIVE_DISTANCE_X ? Mathf.Sign(deltaX) * Speed.Value : 0f;
        _rb.linearVelocity = velocity;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_rb == null) return;

        if (IsJumpAttacking()) return;

        Vector3 velocity = _rb.linearVelocity;
        velocity.x = 0f;
        _rb.linearVelocity = velocity;
    }

    private bool IsJumpAttacking()
    {
        return _monster != null && _monster.IsJumpAttacking;
    }
}

