using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

// 공중 몬스터가 플레이어 기준 사격 위치(일정 거리·높이)로 XY 평면 이동한다.
// 이미 사격 위치 근처면 Failure를 반환해 다음 분기(대기)로 넘긴다.
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Fly", story: "Fly to fire position of [Target] at [Speed]", category: "Enemy", id: "d318d6e599f846dbb44e510c9dae345f")]
public partial class FlyAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Speed;

    private DroneController _drone;

    protected override Status OnStart()
    {
        _drone = GameObject.GetComponent<DroneController>();

        if (_drone == null || Target.Value == null || _drone.IsAttacking)
            return Status.Failure;

        return _drone.NeedsReposition(Target.Value.transform) ? Status.Running : Status.Failure;
    }

    protected override Status OnUpdate()
    {
        if (Target.Value == null || _drone.IsAttacking)
            return Status.Failure;

        return _drone.FlyToFirePosition(Target.Value.transform, Speed.Value)
            ? Status.Running
            : Status.Success;
    }

    protected override void OnEnd()
    {
        if (_drone == null || _drone.IsAttacking)
            return;

        _drone.StopFlying();
    }
}
