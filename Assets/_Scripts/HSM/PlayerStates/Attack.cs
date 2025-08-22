using stal.HSM.Contexts;
using stal.HSM.Core;
using UnityEngine;

namespace stal.HSM.PlayerStates
{
  public class Attack : State
  {
    private readonly PlayerAttributesDataSO _playerAttributesDataSO;
    private readonly PlayerMovementDataSO _playerMovementDataSO;
    private readonly PlayerContext _playerContext;

    private readonly float _attackOneTime = 1f;
    private float _attackTimer;

    public Attack(HierarchicalStateMachine stateMachine, State parent, PlayerContext playerContext, HSMScratchpadSO scratchpad) : base(stateMachine, parent)
    {
      _playerAttributesDataSO = scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
      _playerMovementDataSO = scratchpad.GetScratchpadData<PlayerMovementDataSO>();
      _playerContext = playerContext;
    }

    protected override State GetTransition() => _playerAttributesDataSO.IsAttacking ? null : ((PlayerRoot)Parent).Movement;

    // protected override State GetTransition()
    // {
    //   if (_attackTimer == 0f) return ((PlayerRoot)Parent).Movement;

    //   return null;
    // }

    protected override void OnEnter()
    {
      _attackTimer = _attackOneTime;
    }

    protected override void OnUpdate(float deltaTime)
    {
      _attackTimer = Mathf.Clamp(_attackTimer - deltaTime, 0f, _attackOneTime);
    }
  }
}