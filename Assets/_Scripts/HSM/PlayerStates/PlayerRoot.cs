using stal.HSM.Core;
using stal.HSM.Drivers;

namespace stal.HSM.PlayerStates
{
  public class PlayerRoot : State
  {
    public readonly Movement Movement;

    public PlayerRoot(HierarchicalStateMachine stateMachine, PlayerMovementDataSO playerMovementDataSO, PlayerContext playerContext) : base(stateMachine, null)
    {
      Movement = new(stateMachine, this, playerMovementDataSO, playerContext);
    }

    protected override State GetInitialState() => Movement;
  }
}
