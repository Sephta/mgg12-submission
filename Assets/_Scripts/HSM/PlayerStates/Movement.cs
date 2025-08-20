using stal.HSM.Core;
using stal.HSM.Drivers;
using UnityEngine;

namespace stal.HSM.PlayerStates
{
  public class Movement : State
  {
    private readonly PlayerMovementDataSO _playerMovementDataSO;
    private readonly PlayerContext _playerContext;

    public Movement(HierarchicalStateMachine stateMachine, State Parent, PlayerMovementDataSO playerMovementDataSO, PlayerContext playerContext) : base(stateMachine, Parent)
    {
      _playerMovementDataSO = playerMovementDataSO;
      _playerContext = playerContext;
    }

    protected override void OnUpdate(float deltaTime)
    {
      _playerContext.targetSpeed = _playerMovementDataSO.PlayerDirectionInput.x * _playerMovementDataSO.RunVelocityMaximum;

      // Perform actions based on updates
      MovePlayer();
      PerformJump();
      HandleGravity();
      ClampPlayerMovement();

    }

    private void MovePlayer()
    {
      float accelRate;

      if (_playerMovementDataSO.IsGrounded)
      {
        if (Mathf.Abs(_playerContext.targetSpeed) > 0.01f)
        {
          accelRate = _playerMovementDataSO.RunAccelerationAmount;
        }
        else
        {
          accelRate = _playerMovementDataSO.RunDecelerationAmount;

        }
      }
      else
      {
        if (Mathf.Abs(_playerContext.targetSpeed) > 0.01f)
        {
          accelRate = _playerMovementDataSO.RunAccelerationAmount * _playerMovementDataSO.AccelerationAirMultiplier;
        }
        else
        {
          accelRate = _playerMovementDataSO.RunDecelerationAmount * _playerMovementDataSO.DecelerationAirMultiplier;

        }
      }

      // Conserve momentum
      if (Mathf.Abs(_playerContext.rigidbody2D.linearVelocityX) > Mathf.Abs(_playerContext.targetSpeed) && Mathf.Sign(_playerContext.rigidbody2D.linearVelocityX) == Mathf.Sign(_playerContext.targetSpeed) && _playerContext.targetSpeed > 0.01f && !_playerMovementDataSO.IsGrounded)
      {
        accelRate = 0;
      }

      float delta = _playerContext.targetSpeed - _playerContext.rigidbody2D.linearVelocityX;

      float force = delta * accelRate;

      // Multiplying by Vector2.right is a quick way to convert the calculation into a vector
      _playerContext.rigidbody2D.AddForce(force * Time.fixedDeltaTime * Vector2.right, ForceMode2D.Force);

    }

    private void PerformJump()
    {
      if (!_playerContext.jumpEndEarly && !_playerMovementDataSO.IsGrounded && !_playerContext.isJumping && _playerContext.rigidbody2D.linearVelocityY > 0) _playerContext.jumpEndEarly = true;

      if (_playerContext.jumpBufferWindow > 0 && _playerContext.coyoteTime > 0 && _playerContext.jumpCount > 0)
      {
        _playerContext.jumpCount = Mathf.Clamp(_playerContext.jumpCount - 1, 0, _playerMovementDataSO.JumpMaximum);
        _playerContext.jumpBufferWindow = 0f;
        _playerContext.coyoteTime = 0f;

        ExecuteJump();
      }
    }

    private void ExecuteJump()
    {
      _playerContext.jumpEndEarly = false;
      _playerContext.rigidbody2D.linearVelocityY = _playerMovementDataSO.JumpingPower;
    }

    private void HandleGravity()
    {
      // If we are falling then we have a different gravity multiplier
      if (_playerContext.rigidbody2D.linearVelocityY < 0 && !_playerMovementDataSO.IsGrounded)
      {
        UpdateGravityScale(_playerMovementDataSO.FallingGravityMultiplier);
      }
      else
      {
        ResetGravityScale();
      }

      if (_playerContext.jumpEndEarly) UpdateGravityScale(_playerMovementDataSO.FallingGravityMultiplier * _playerMovementDataSO.ShortJumpGravityMultiplier);

      if (_playerContext.isJumping && (Mathf.Abs(_playerContext.rigidbody2D.linearVelocityY) < _playerMovementDataSO.JumpHangTimeThreshold))
      {
        UpdateGravityScale(_playerMovementDataSO.JumpHangTimeGravityMultiplier);
      }
    }

    private void ClampPlayerMovement()
    {
      // Clamps velocity by the amount configured in _playerMovementDataSO
      if (_playerMovementDataSO.DoClampHorizontalVelocity)
      {
        _playerContext.rigidbody2D.linearVelocityX = Mathf.Clamp(_playerContext.rigidbody2D.linearVelocityX, -_playerMovementDataSO.VelocityHorizontalClamp, _playerMovementDataSO.VelocityHorizontalClamp);
      }
      if (_playerMovementDataSO.DoClampVerticalVelocity)
      {
        _playerContext.rigidbody2D.linearVelocityY = Mathf.Clamp(_playerContext.rigidbody2D.linearVelocityY, -_playerMovementDataSO.VelocityVerticalClamp, _playerMovementDataSO.VelocityVerticalClamp);
      }
    }

    private void ResetGravityScale() => _playerContext.rigidbody2D.gravityScale = _playerMovementDataSO.GravityScale;
    private void UpdateGravityScale(float scale) => _playerContext.rigidbody2D.gravityScale = _playerMovementDataSO.GravityScale * scale;
  }

}
