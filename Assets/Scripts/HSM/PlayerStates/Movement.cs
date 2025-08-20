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
      _playerContext.inputDirection = _playerMovementDataSO.PlayerDirectionInput;

      // Update runtime movement data
      _playerMovementDataSO.UpdateIsGrounded(IsGrounded());
      _playerMovementDataSO.UpdatePlayerVelocity(_playerContext.rigidbody2D.linearVelocity);

      // Update timers
      _playerContext.jumpBufferWindow = Mathf.Clamp(_playerContext.jumpBufferWindow - Time.deltaTime, 0f, _playerMovementDataSO.JumpInputBuffer);
      _playerContext.coyoteTime = Mathf.Clamp(_playerContext.coyoteTime - Time.deltaTime, 0f, _playerMovementDataSO.CoyoteTime);

      // Reset timers 
      if (_playerMovementDataSO.IsGrounded) _playerContext.coyoteTime = _playerMovementDataSO.CoyoteTime;
      if (!_playerContext.wasGroundedLastFrame && _playerMovementDataSO.IsGrounded) _playerContext.jumpCount = _playerMovementDataSO.JumpMaximum;

      _playerContext.targetSpeed = _playerMovementDataSO.PlayerDirectionInput.x * _playerMovementDataSO.RunVelocityMaximum;

      // Perform actions based on updates
      MovePlayer();
      PerformJump();
      HandleGravity();
      ClampPlayerMovement();

      // cache grounded state at the end of this frame since next frame we might not be grounded.
      _playerContext.wasGroundedLastFrame = IsGrounded();
      _playerContext.inputDirectionLastFrame = _playerMovementDataSO.PlayerDirectionInput;
    }

    private bool IsGrounded()
    {
      // using three distinct rays, the middle of which casts from player's origin position, 
      // while the left and right cast from sids of the player's collider

      Vector3 leftRayPosition = new(_playerContext.transform.position.x - _playerContext.boxCollider2D.bounds.extents.x, _playerContext.transform.position.y, _playerContext.transform.position.z);
      Vector3 middleRayPosition = _playerContext.transform.position;
      Vector3 rightRayPosition = new(_playerContext.transform.position.x + _playerContext.boxCollider2D.bounds.extents.x, _playerContext.transform.position.y, _playerContext.transform.position.z);

      RaycastHit2D leftRay = Physics2D.Raycast(
        leftRayPosition,
        Vector2.down,
        _playerContext.boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance,
        _playerMovementDataSO.LayersConsideredForGroundingPlayer
      );

      RaycastHit2D middleRay = Physics2D.Raycast(
        middleRayPosition,
        Vector2.down,
        _playerContext.boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance,
        _playerMovementDataSO.LayersConsideredForGroundingPlayer
      );

      RaycastHit2D rightRay = Physics2D.Raycast(
        rightRayPosition,
        Vector2.down,
        _playerContext.boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance,
        _playerMovementDataSO.LayersConsideredForGroundingPlayer
      );

      if (_playerContext.drawDebugGizmos)
      {
        // For debugging (only appears in the Scene window, not the game window)
        Debug.DrawRay(leftRayPosition, _playerContext.boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance * Vector3.down, Color.red, 1f);
        Debug.DrawRay(middleRayPosition, _playerContext.boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance * Vector3.down, Color.red, 1f);
        Debug.DrawRay(rightRayPosition, _playerContext.boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance * Vector3.down, Color.red, 1f);
      }

      // player is grounded if any of the following rays make contact with an object with on the specified LayerMask stored in _playerMovementDataSO
      return leftRay || middleRay || rightRay;
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
