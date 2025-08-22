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
    private readonly PlayerAttributesDataSO _playerAttributesDataSO;

    public Nero(HierarchicalStateMachine stateMachine, State parent, PlayerContext playerContext, HSMScratchpadSO scratchpad) : base(stateMachine, parent)
    {
      _playerAttributesDataSO = scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
      _playerContext = playerContext;

      Neutral = new(stateMachine, this, playerContext, scratchpad);
      Needle = new(stateMachine, this, playerContext, scratchpad);
      Claw = new(stateMachine, this, playerContext, scratchpad);
      Gun = new(stateMachine, this, playerContext, scratchpad);
    }

    // In the future we should set the initial state to whatever the currently equipped seed arm is
    protected override State GetInitialState() => Neutral;

    protected override State GetTransition() => _playerAttributesDataSO.IsTakingAim ? null : ((PlayerRoot)Parent).Movement;

    protected override void OnUpdate(float deltaTime) { }
  }
}
