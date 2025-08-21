using stal.HSM.Core;
using stal.HSM.Drivers;
using UnityEngine;

namespace stal.HSM.PlayerStates
{
  public class NeroGun : State
  {
    private readonly PlayerMovementDataSO _playerMovementDataSO;
    private readonly PlayerContext _playerContext;

    public NeroGun(HierarchicalStateMachine stateMachine, State Parent, PlayerAttributesDataSO playerAttributesDataSO, PlayerMovementDataSO playerMovementDataSO, PlayerContext playerContext) : base(stateMachine, Parent)
    {
      _playerMovementDataSO = playerMovementDataSO;
      _playerContext = playerContext;
    }

    protected override void OnUpdate(float deltaTime) { }
  }
}