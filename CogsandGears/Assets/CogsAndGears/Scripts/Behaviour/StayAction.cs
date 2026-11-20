using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Stay", story: "Wait for [value] seconds", category: "Action", id: "e5df7aa1dec4595d2d690a48a17ba5e4")]
public partial class StayAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Value;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

