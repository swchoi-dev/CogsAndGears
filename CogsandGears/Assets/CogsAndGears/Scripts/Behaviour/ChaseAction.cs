using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase", story: "Chase [Target] at [Speed]", category: "Enemy", id: "70258839bb1548405ff7b4b8e373a966")]
public partial class ChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Speed;
    [SerializeReference] public BlackboardVariable<float> AttackRange;

    private Rigidbody _rb;

    protected override Status OnStart()
    {
        _rb = GameObject.GetComponent<Rigidbody>();

        return _rb != null ? Status.Running : Status.Failure;
    }

    protected override Status OnUpdate()
    {
        if (Target.Value == null)
            return Status.Failure;

        float deltaX =
            Target.Value.transform.position.x - GameObject.transform.position.x;
        
        if (Mathf.Abs(deltaX) <= AttackRange.Value)
        {
            return Status.Success;
        }

        Vector3 velocity = _rb.linearVelocity;
        velocity.x = Mathf.Sign(deltaX) * Speed.Value;
        _rb.linearVelocity = velocity;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_rb == null) return;

        Vector3 velocity = _rb.linearVelocity;
        velocity.x = 0f;
        _rb.linearVelocity = velocity;
    }
}

