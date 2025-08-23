using stal.HSM.Contexts;
using stal.HSM.Core;
using UnityEngine.InputSystem;

namespace stal.HSM.PlayerStates
{
  public abstract class NeroSubstate : State
  {
    protected NeroSubstate(HierarchicalStateMachine stateMachine, State parent = null) : base(stateMachine, parent) { }
  }

  public class Nero : State
  {
    // Nero Substates
    public readonly NeroNeutral Neutral;
    public readonly NeroNeedle Needle;
    public readonly NeroClaw Claw;
    public readonly NeroGun Gun;

    private readonly PlayerContext _playerContext;
    private readonly PlayerAttributesDataSO _playerAttributesDataSO;
    private readonly PlayerAbilityDataSO _playerAbilityDataSO;
    private readonly PlayerEventDataSO _playerEventDataSO;

    public Nero(HierarchicalStateMachine stateMachine, State parent, PlayerContext playerContext, HSMScratchpadSO scratchpad) : base(stateMachine, parent)
    {
      _playerContext = playerContext;
      _playerAttributesDataSO = scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
      _playerAbilityDataSO = scratchpad.GetScratchpadData<PlayerAbilityDataSO>();
      _playerEventDataSO = scratchpad.GetScratchpadData<PlayerEventDataSO>();

      // Child States
      Neutral = new(stateMachine, this, playerContext, scratchpad);
      Needle = new(stateMachine, this, playerContext, scratchpad);
      Claw = new(stateMachine, this, playerContext, scratchpad);
      Gun = new(stateMachine, this, playerContext, scratchpad);
    }

    // In the future we should set the initial state to whatever the currently equipped seed arm is
    protected override State GetInitialState()
    {
      return _playerAbilityDataSO.CurrentlyEquippedArmType switch
      {
        NeroArmType.Neutral => Neutral,
        NeroArmType.Needle => Needle,
        NeroArmType.Claw => Claw,
        NeroArmType.Gun => Gun,
        _ => null,
      };
    }

    protected override State GetTransition() => _playerAttributesDataSO.IsTakingAim ? null : ((PlayerRoot)Parent).Movement;

    protected override void OnUpdate(float deltaTime) { }
  }
}
