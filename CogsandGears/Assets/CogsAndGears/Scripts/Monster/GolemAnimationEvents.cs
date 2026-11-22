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
    
    public void OnJumpLanded()
    {
        _monster.EndJumpAttack();
    }
}