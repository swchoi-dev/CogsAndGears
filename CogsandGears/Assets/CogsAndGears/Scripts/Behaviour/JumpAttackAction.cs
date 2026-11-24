using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

// 점프 공격 1회를 실행한다. 쿨다운·도약·착지 판정은 MonsterController가 담당하고,
// 이 노드는 공격 시작 요청과 공격 종료까지의 대기만 처리한다.
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Jump Attack", story: "Jump Attack", category: "Enemy", id: "2d0b1cfb86be4032806b2273fbb7c212")]
public partial class JumpAttackAction : Action
{
    private MonsterController _monster;

    protected override Status OnStart()
    {
        _monster = GameObject.GetComponent<MonsterController>();

        if (_monster == null)
            return Status.Failure;

        // 쿨다운 중이거나 이미 공격 중이면 실패시켜 다음 분기(추적/대기)로 넘긴다.
        return _monster.TryStartJumpAttack() ? Status.Running : Status.Failure;
    }

    protected override Status OnUpdate()
    {
        return _monster.IsJumpAttacking ? Status.Running : Status.Success;
    }
}
