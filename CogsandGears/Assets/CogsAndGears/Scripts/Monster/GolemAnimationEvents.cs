using UnityEngine;

public class GolemAnimationEvents : MonoBehaviour
{
    private MonsterController _monster;

    private void Awake()
    {
        _monster = GetComponentInParent<MonsterController>();
    }

    public void OnJumpTakeoff()
    {
        _monster.JumpTowardTarget();
    }
    
    // 클립에 등록된 이벤트의 수신자. 착지 확정은 MonsterController가 실제 지면 접촉으로 판정하므로
    // 애니메이션 착지 시점에서는 아무것도 하지 않는다.
    public void OnJumpLanded()
    {
    }
}