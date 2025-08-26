using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
  [Header("Data References")]
  [SerializeField] private Rigidbody2D _rigidBody2D;
  [SerializeField] private BoxCollider2D _boxCollider2D;

  [Header("Player Data")]
  [SerializeField, Expandable, Required] private PlayerMovementDataSO _playerMovementData;
  [SerializeField, Expandable, Required] private PlayerAttributesDataSO _playerAttributesData;

  [Header("Debug")]
  [SerializeField] private bool _drawDebugGizmos;
  [SerializeField, ReadOnly] private Vector2 _inputDirection;
  [SerializeField, ReadOnly] private Vector2 _inputDirectionLastFrame;
  [SerializeField, ReadOnly] private bool _isJumping;
  [SerializeField, ReadOnly] private float _targetSpeed;
  [SerializeField, ReadOnly] private float _coyoteTime;
  [SerializeField, ReadOnly] private float _jumpBufferWindow;
  [SerializeField, ReadOnly] private int _jumpCount;
  [SerializeField, ReadOnly] private bool _wasGroundedLastFrame;
  [SerializeField, ReadOnly] private bool _jumpEndEarly = false;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_playerMovementData == null)
    {
      Debug.LogError(name + " does not have a PlayerMovementDataSO referenced in the inspector.  Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_playerAttributesData == null)
    {
      Debug.LogError(name + " does not have a PlayerAttributesDataSO referenced in the inspector.  Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_rigidBody2D == null) _rigidBody2D = GetComponent<Rigidbody2D>();
    if (_boxCollider2D == null) _boxCollider2D = GetComponent<BoxCollider2D>();
  }

  private void Start()
  {
    // Set default parameters
    ResetGravityScale();
    _jumpCount = _playerMovementData.JumpMaximum;
  }

  private void Update()
  {
    _inputDirection = _playerAttributesData.PlayerMoveDirection;

    // Update runtime movement data
    _playerAttributesData.UpdateIsGrounded(IsGrounded());
    _playerAttributesData.UpdatePlayerVelocity(_rigidBody2D.linearVelocity);

    // Update timers
    _jumpBufferWindow = Mathf.Clamp(_jumpBufferWindow - Time.deltaTime, 0f, _playerMovementData.JumpInputBuffer);
    _coyoteTime = Mathf.Clamp(_coyoteTime - Time.deltaTime, 0f, _playerMovementData.CoyoteTime);

    // Reset timers 
    if (_playerAttributesData.IsGrounded) _coyoteTime = _playerMovementData.CoyoteTime;
    if (!_wasGroundedLastFrame && _playerAttributesData.IsGrounded) _jumpCount = _playerMovementData.JumpMaximum;

    _targetSpeed = _playerAttributesData.PlayerMoveDirection.x * _playerMovementData.RunVelocityMaximum;

    // Perform actions based on updates
    MovePlayer();
    PerformJump();
    HandleGravity();
    ClampPlayerMovement();

    // cache grounded state at the end of this frame since next frame we might not be grounded.
    _wasGroundedLastFrame = IsGrounded();
    _inputDirectionLastFrame = _playerAttributesData.PlayerMoveDirection;
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  // For used in the Player Input component.
  public void OnMove(InputAction.CallbackContext context) => _playerAttributesData.UpdatePlayerDirectionInput(context.ReadValue<Vector2>());

  // For use in the Player Input component.
  public void OnJump(InputAction.CallbackContext context)
  {
    if (context.started) _isJumping = true;
    if (context.canceled) _isJumping = false;
    if (context.performed)
    {
      _jumpBufferWindow = _playerMovementData.JumpInputBuffer;
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  [SerializeField, ReadOnly] private float _rayCastDistance;
  private bool IsGrounded()
  {
    // Exposing in the inspector. Needs to be removed but for now is fine. Was useful for testing some stuff.
    _rayCastDistance = _playerMovementData.GroundingRayCastDistance;

    // using three distinct rays, the middle of which casts from player's origin position, 
    // while the left and right cast from sids of the player's collider

    Vector3 leftRayPosition = new(transform.position.x - _boxCollider2D.bounds.extents.x, transform.position.y, transform.position.z);
    Vector3 middleRayPosition = transform.position;
    Vector3 rightRayPosition = new(transform.position.x + _boxCollider2D.bounds.extents.x, transform.position.y, transform.position.z);

    RaycastHit2D leftRay = Physics2D.Raycast(
      leftRayPosition,
      Vector2.down,
      _boxCollider2D.bounds.extents.y * _rayCastDistance,
      _playerMovementData.LayersConsideredForGroundingPlayer
    );

    RaycastHit2D middleRay = Physics2D.Raycast(
      middleRayPosition,
      Vector2.down,
      _boxCollider2D.bounds.extents.y * _rayCastDistance,
      _playerMovementData.LayersConsideredForGroundingPlayer
    );

    RaycastHit2D rightRay = Physics2D.Raycast(
      rightRayPosition,
      Vector2.down,
      _boxCollider2D.bounds.extents.y * _rayCastDistance,
      _playerMovementData.LayersConsideredForGroundingPlayer
    );

    if (_drawDebugGizmos)
    {
      // For debugging (only appears in the Scene window, not the game window)
      Debug.DrawRay(leftRayPosition, _boxCollider2D.bounds.extents.y * _rayCastDistance * Vector3.down, Color.red, 1f);
      Debug.DrawRay(middleRayPosition, _boxCollider2D.bounds.extents.y * _rayCastDistance * Vector3.down, Color.red, 1f);
      Debug.DrawRay(rightRayPosition, _boxCollider2D.bounds.extents.y * _rayCastDistance * Vector3.down, Color.red, 1f);
    }

    // player is grounded if any of the following rays make contact with an object with on the specified LayerMask stored in _playerMovementData
    return leftRay || middleRay || rightRay;
  }

  private void MovePlayer()
  {
    float accelRate;

    if (_playerAttributesData.IsGrounded)
    {
      if (Mathf.Abs(_targetSpeed) > 0.01f)
      {
        accelRate = _playerMovementData.RunAccelerationAmount;
      }
      else
      {
        accelRate = _playerMovementData.RunDecelerationAmount;

      }
    }
    else
    {
      if (Mathf.Abs(_targetSpeed) > 0.01f)
      {
        accelRate = _playerMovementData.RunAccelerationAmount * _playerMovementData.AccelerationAirMultiplier;
      }
      else
      {
        accelRate = _playerMovementData.RunDecelerationAmount * _playerMovementData.DecelerationAirMultiplier;

      }
    }

    // Conserve momentum
    if (Mathf.Abs(_rigidBody2D.linearVelocityX) > Mathf.Abs(_targetSpeed) && Mathf.Sign(_rigidBody2D.linearVelocityX) == Mathf.Sign(_targetSpeed) && _targetSpeed > 0.01f && !_playerAttributesData.IsGrounded)
    {
      accelRate = 0;
    }

    float delta = _targetSpeed - _rigidBody2D.linearVelocityX;

    float force = delta * accelRate;

    // Multiplying by Vector2.right is a quick way to convert the calculation into a vector
    _rigidBody2D.AddForce(force * Time.fixedDeltaTime * Vector2.right, ForceMode2D.Force);

  }

  private void PerformJump()
  {
    if (!_jumpEndEarly && !_playerAttributesData.IsGrounded && !_isJumping && _rigidBody2D.linearVelocityY > 0) _jumpEndEarly = true;

    if (_jumpBufferWindow > 0 && _coyoteTime > 0 && _jumpCount > 0)
    {
      _jumpCount = Mathf.Clamp(_jumpCount - 1, 0, _playerMovementData.JumpMaximum);
      _jumpBufferWindow = 0f;
      _coyoteTime = 0f;

      ExecuteJump();
    }
  }

  private void ExecuteJump()
  {
    _jumpEndEarly = false;
    _rigidBody2D.linearVelocityY = _playerMovementData.JumpingPower;
  }

  private void HandleGravity()
  {
    // If we are falling then we have a different gravity multiplier
    if (_rigidBody2D.linearVelocityY < 0 && !_playerAttributesData.IsGrounded)
    {
      UpdateGravityScale(_playerMovementData.FallingGravityMultiplier);
    }
    else
    {
      ResetGravityScale();
    }

    if (_jumpEndEarly) UpdateGravityScale(_playerMovementData.FallingGravityMultiplier * _playerMovementData.ShortJumpGravityMultiplier);

    if (_isJumping && (Mathf.Abs(_rigidBody2D.linearVelocityY) < _playerMovementData.JumpHangTimeThreshold))
    {
      UpdateGravityScale(_playerMovementData.JumpHangTimeGravityMultiplier);
    }
  }

  private void ClampPlayerMovement()
  {
    // Clamps velocity by the amount configured in _playerMovementData
    if (_playerMovementData.DoClampHorizontalVelocity)
    {
      _rigidBody2D.linearVelocityX = Mathf.Clamp(_rigidBody2D.linearVelocityX, -_playerMovementData.VelocityHorizontalClamp, _playerMovementData.VelocityHorizontalClamp);
    }
    if (_playerMovementData.DoClampVerticalVelocity)
    {
      _rigidBody2D.linearVelocityY = Mathf.Clamp(_rigidBody2D.linearVelocityY, -_playerMovementData.VelocityVerticalClamp, _playerMovementData.VelocityVerticalClamp);
    }
  }


  private void ResetGravityScale() => _rigidBody2D.gravityScale = _playerMovementData.GravityScale;
  private void UpdateGravityScale(float scale) => _rigidBody2D.gravityScale = _playerMovementData.GravityScale * scale;

}
