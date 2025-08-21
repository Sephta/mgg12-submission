using stal.HSM.Core;
using stal.HSM.Drivers;

namespace stal.HSM.PlayerStates
{
  public class Nero : State
  {
    // Nero Substates
    public readonly NeroNeutral Neutral;
    public readonly NeroNeedle Needle;
    public readonly NeroClaw Claw;
    public readonly NeroGun Gun;

    private readonly PlayerContext _playerContext;

    public Nero(HierarchicalStateMachine stateMachine, State Parent, PlayerAttributesDataSO playerAttributesDataSO, PlayerMovementDataSO playerMovementDataSO, PlayerContext playerContext) : base(stateMachine, Parent)
    {
      _playerContext = playerContext;

      Neutral = new(stateMachine, this, playerAttributesDataSO, playerMovementDataSO, playerContext);
      Needle = new(stateMachine, this, playerAttributesDataSO, playerMovementDataSO, playerContext);
      Claw = new(stateMachine, this, playerAttributesDataSO, playerMovementDataSO, playerContext);
      Gun = new(stateMachine, this, playerAttributesDataSO, playerMovementDataSO, playerContext);
    }

    // In the future we should set the initial state to whatever the currently equipped seed arm is
    protected override State GetInitialState() => Neutral;

    protected override State GetTransition() => _playerContext.isTakingAim ? null : ((PlayerRoot)Parent).Movement;

    protected override void OnUpdate(float deltaTime) { }
  }
}
