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

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Transform selfTransform = GameObject.transform;
        Transform targetTransform = Target.Value.transform;
        Vector3 toTarget = targetTransform.position - selfTransform.position;

        selfTransform.LookAt(targetTransform);
        selfTransform.position += toTarget.normalized * Speed.Value * Time.deltaTime;

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

