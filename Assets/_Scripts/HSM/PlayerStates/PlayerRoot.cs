using stal.HSM.Core;
using stal.HSM.Drivers;
using UnityEngine;

namespace stal.HSM.PlayerStates
{
  public class PlayerRoot : State
  {
    public readonly Movement Movement;
    public readonly Attack Attack;
    public readonly Nero Nero;

    private readonly PlayerMovementDataSO _playerMovementDataSO;
    private readonly PlayerContext _playerContext;

    public PlayerRoot(HierarchicalStateMachine stateMachine, PlayerMovementDataSO playerMovementDataSO, PlayerContext playerContext) : base(stateMachine, null)
    {
      Movement = new(stateMachine, this, playerMovementDataSO, playerContext);
      Attack = new(stateMachine, this, playerMovementDataSO, playerContext);
      Nero = new(stateMachine, this, playerMovementDataSO, playerContext);

      _playerMovementDataSO = playerMovementDataSO;
      _playerContext = playerContext;
    }

    protected override State GetInitialState() => Movement;

    protected override State GetTransition()
    {
      if (_playerContext.isTakingAim && _playerMovementDataSO.IsGrounded) return Nero;

      if (_playerContext.isAtacking && !_playerContext.isTakingAim && _playerMovementDataSO.IsGrounded) return Attack;

      return null;
    }

    protected override void OnUpdate(float deltaTime)
    {
      // We need to decelerate the player back to zero outside of the Movement state because
      // otherwise we slide infinitely with no friction. The deceleration is our friction bringing
      // our linear velocity back to zero.
      if (ActiveChild != Movement)
      {
        float delta = 0 - _playerContext.rigidbody2D.linearVelocityX;

        float force = delta * _playerMovementDataSO.RunDecelerationAmount;

        // Multiplying by Vector2.right is a quick way to convert the calculation into a vector
        _playerContext.rigidbody2D.AddForce(force * Time.fixedDeltaTime * Vector2.right, ForceMode2D.Force);
      }
    }

  }
}
