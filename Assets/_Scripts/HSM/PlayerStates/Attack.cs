using stal.HSM.Core;
using stal.HSM.Drivers;
using UnityEngine;

namespace stal.HSM.PlayerStates
{
  public class Attack : State
  {
    private readonly PlayerMovementDataSO _playerMovementDataSO;
    private readonly PlayerContext _playerContext;

    private readonly float _attackOneTime = 1f;
    private float _attackTimer;

    public Attack(HierarchicalStateMachine stateMachine, State Parent, PlayerMovementDataSO playerMovementDataSO, PlayerContext playerContext) : base(stateMachine, Parent)
    {
      _playerMovementDataSO = playerMovementDataSO;
      _playerContext = playerContext;
    }

    protected override State GetTransition() => _playerContext.isAtacking ? null : ((PlayerRoot)Parent).Movement;

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