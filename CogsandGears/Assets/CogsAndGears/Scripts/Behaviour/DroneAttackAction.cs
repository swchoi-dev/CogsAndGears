using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

// 원거리 공격 1회를 실행한다. 쿨다운·발사는 DroneController가 담당하고,
// 이 노드는 공격 시작 요청과 Attack 애니메이션 종료까지의 대기만 처리한다.
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Drone Attack", story: "Shoot at [Target]", category: "Enemy", id: "732a4ea1d6f84dfbbdf7b4922e44e514")]
public partial class DroneAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    private DroneController _drone;

    protected override Status OnStart()
    {
        _drone = GameObject.GetComponent<DroneController>();

        if (_drone == null || Target.Value == null)
            return Status.Failure;

        // 쿨다운 중이면 실패시켜 다음 분기(이동/대기)로 넘긴다.
        return _drone.TryStartAttack(Target.Value.transform) ? Status.Running : Status.Failure;
    }

    protected override Status OnUpdate()
    {
        return _drone.IsAttacking ? Status.Running : Status.Success;
    }
}
