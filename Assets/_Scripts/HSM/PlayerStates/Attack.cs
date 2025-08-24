using stal.HSM.Contexts;
using stal.HSM.Core;
using UnityEngine;

namespace stal.HSM.PlayerStates
{
  public class Attack : State
  {
    private readonly PlayerAttributesDataSO _playerAttributesDataSO;
    private readonly PlayerMovementDataSO _playerMovementDataSO;
    private readonly PlayerEventDataSO _playerEventDataSO;
    private readonly PlayerContext _playerContext;

    private readonly float _attackOneTime = 1f;
    private float _attackTimer;

    public Attack(HierarchicalStateMachine stateMachine, State parent, PlayerContext playerContext, HSMScratchpadSO scratchpad) : base(stateMachine, parent)
    {
      _playerAttributesDataSO = scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
      _playerMovementDataSO = scratchpad.GetScratchpadData<PlayerMovementDataSO>();
      _playerEventDataSO = scratchpad.GetScratchpadData<PlayerEventDataSO>();
      _playerContext = playerContext;
    }

    protected override void OnEnter()
    {
      _attackTimer = _attackOneTime;
      _playerEventDataSO.AttackChainCompleted.OnEventRaised += RequestTransitionOutOfAttackState;
    }

    protected override void OnExit()
    {
      _playerEventDataSO.AttackChainCompleted.OnEventRaised -= RequestTransitionOutOfAttackState;
    }

    protected override void OnUpdate(float deltaTime)
    {
      _attackTimer = Mathf.Clamp(_attackTimer - deltaTime, 0f, _attackOneTime);
    }

    private void RequestTransitionOutOfAttackState()
    {
      StateMachine.Sequencer.RequestTransition(this, ((PlayerRoot)Parent).Movement);
    }
  }
}