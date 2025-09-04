using System;
using stal.HSM.Contexts;
using stal.HSM.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace stal.HSM.PlayerStates
{
  public class Movement : State
  {
    private readonly PlayerAttributesDataSO _playerAttributesDataSO;
    private readonly PlayerMovementDataSO _playerMovementDataSO;
    private readonly PlayerAbilityDataSO _playerAbilityDataSO;
    private readonly PlayerContext _playerContext;

    public Movement(HierarchicalStateMachine stateMachine, State parent, PlayerContext playerContext, HSMScratchpadSO scratchpad) : base(stateMachine, parent)
    {
      _playerAttributesDataSO = scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
      _playerMovementDataSO = scratchpad.GetScratchpadData<PlayerMovementDataSO>();
      _playerAbilityDataSO = scratchpad.GetScratchpadData<PlayerAbilityDataSO>();
      _playerContext = playerContext;
    }

    protected override State GetTransition()
    {
      if (_playerAttributesDataSO.IsTakingAim
        && _playerAbilityDataSO.CurrentlyEquippedArmType == NeroArmType.Neutral
        && _playerAttributesDataSO.IsGrounded)
      {
        return ((PlayerRoot)Parent).Nero;
      }

      return null;
    }

    protected override void OnUpdate(float deltaTime)
    {
      _playerContext.targetSpeed = _playerAttributesDataSO.PlayerMoveDirection.x * _playerMovementDataSO.RunVelocityMaximum;

      // Perform actions based on updates
      MovePlayer();
      PerformJump();
      HandleGravity();
      ClampPlayerMovement();

    }

    private void MovePlayer()
    {
      float accelRate;

      if (_playerAttributesDataSO.IsGrounded)
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
      if (Mathf.Abs(_playerContext.rigidbody2D.linearVelocityX) > Mathf.Abs(_playerContext.targetSpeed) && Mathf.Sign(_playerContext.rigidbody2D.linearVelocityX) == Mathf.Sign(_playerContext.targetSpeed) && Mathf.Abs(_playerContext.targetSpeed) > 0.01f && !_playerAttributesDataSO.IsGrounded)
      {
        accelRate = 0;
      }

      float delta = _playerContext.targetSpeed - _playerContext.rigidbody2D.linearVelocityX;

      float force = delta * accelRate;

      // Multiplying by Vector2.right is a quick way to convert the calculation into a vector
      _playerContext.rigidbody2D.AddForce(force * Vector2.right, ForceMode2D.Force);

    }

    private void PerformJump()
    {
      if (!_playerContext.jumpEndEarly && !_playerAttributesDataSO.IsGrounded && !_playerAttributesDataSO.IsJumping && _playerContext.rigidbody2D.linearVelocityY > 0) _playerContext.jumpEndEarly = true;

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
      if (_playerContext.rigidbody2D.linearVelocityY < 0 && !_playerAttributesDataSO.IsGrounded)
      {
        UpdateGravityScale(_playerMovementDataSO.FallingGravityMultiplier);
      }
      else
      {
        ResetGravityScale();
      }

      if (_playerContext.jumpEndEarly) UpdateGravityScale(_playerMovementDataSO.FallingGravityMultiplier * _playerMovementDataSO.ShortJumpGravityMultiplier);

      if (_playerAttributesDataSO.IsJumping && (Mathf.Abs(_playerContext.rigidbody2D.linearVelocityY) < _playerMovementDataSO.JumpHangTimeThreshold))
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
