// 피해를 받지 않는 상태를 Health에 알린다. Health와 같은 GameObject에 붙은 컴포넌트가 구현한다(예: PlayerDash).
public interface IInvulnerable
{
    bool IsInvulnerable { get; }
}
