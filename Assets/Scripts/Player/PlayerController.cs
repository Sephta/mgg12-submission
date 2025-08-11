using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
  [Header("Data References")]
  [SerializeField] private Rigidbody2D _rigidBody2D;
  [SerializeField] private BoxCollider2D _boxCollider2D;

  [Header("Movement Settings")]
  [SerializeField, Expandable] private PlayerMovementDataSO _playerMovementDataSO;

  [Header("Debug")]
  [SerializeField, ReadOnly] private Vector2 _inputDirection;
  [SerializeField, ReadOnly] private Vector2 _inputDirectionLastFrame;
  [SerializeField, ReadOnly] private float _targetSpeed;
  [SerializeField, ReadOnly] private int _accelerationBase;
  [SerializeField, ReadOnly] private float _jumpBufferWindow;
  [SerializeField, ReadOnly] private float _coyoteTime;
  [SerializeField, ReadOnly] private int _jumpCount;
  [SerializeField, ReadOnly] private bool _isJumping;
  [SerializeField, ReadOnly] private bool _wasGroundedLastFrame;
  [SerializeField, ReadOnly] private bool _jumpEndEarly = false;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_playerMovementDataSO == null)
    {
      Debug.LogError("PlayerController does not have defined PlayerMovementDataSO.");
    }

    if (_rigidBody2D == null) _rigidBody2D = GetComponent<Rigidbody2D>();
    if (_boxCollider2D == null) _boxCollider2D = GetComponent<BoxCollider2D>();
  }

  private void Start()
  {
    // Set default parameters
    ResetGravityScale();
    _jumpCount = _playerMovementDataSO.MaxNumberOfJumps;

    _accelerationBase = 10;
  }

  private void Update()
  {
    _inputDirection = _playerMovementDataSO.PlayerDirectionInput;

    // Update runtime movement data
    _playerMovementDataSO.UpdateIsGrounded(IsGrounded());
    _playerMovementDataSO.UpdatePlayerVelocity(_rigidBody2D.linearVelocity);

    // Update timers
    _jumpBufferWindow = Mathf.Clamp(_jumpBufferWindow - Time.deltaTime, 0f, _playerMovementDataSO.JumpInputBuffer);
    _coyoteTime = Mathf.Clamp(_coyoteTime - Time.deltaTime, 0f, _playerMovementDataSO.CoyoteTime);

    // Reset timers 
    if (_playerMovementDataSO.IsGrounded) _coyoteTime = _playerMovementDataSO.CoyoteTime;
    if (!_wasGroundedLastFrame && _playerMovementDataSO.IsGrounded) _jumpCount = _playerMovementDataSO.MaxNumberOfJumps;

    _targetSpeed = _playerMovementDataSO.PlayerDirectionInput.x * _playerMovementDataSO.MaxRunVelocity;

    // Perform actions based on updates
    MovePlayer();
    PerformJump();
    HandleGravity();
    ClampPlayerMovement();

    // cache grounded state at the end of this frame since next frame we might not be grounded.
    _wasGroundedLastFrame = IsGrounded();
    _inputDirectionLastFrame = _playerMovementDataSO.PlayerDirectionInput;
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  // For used in the Player Input component.
  public void OnMove(InputAction.CallbackContext context) => _playerMovementDataSO.UpdatePlayerDirectionInput(context.ReadValue<Vector2>());

  // For use in the Player Input component.
  public void OnJump(InputAction.CallbackContext context)
  {
    if (context.started) _isJumping = true;
    if (context.canceled) _isJumping = false;
    if (context.performed)
    {
      _jumpBufferWindow = _playerMovementDataSO.JumpInputBuffer;
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private bool IsGrounded()
  {
    // using three distinct rays, the middle of which casts from player's origin position, 
    // while the left and right cast from sids of the player's collider

    Vector3 leftRayPosition = new(transform.position.x - _boxCollider2D.bounds.extents.x, transform.position.y, transform.position.z);
    Vector3 middleRayPosition = transform.position;
    Vector3 rightRayPosition = new(transform.position.x + _boxCollider2D.bounds.extents.x, transform.position.y, transform.position.z);

    RaycastHit2D leftRay = Physics2D.Raycast(
      leftRayPosition,
      Vector2.down,
      _boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance,
      _playerMovementDataSO.GroundLayerMask
    );

    RaycastHit2D middleRay = Physics2D.Raycast(
      middleRayPosition,
      Vector2.down,
      _boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance,
      _playerMovementDataSO.GroundLayerMask
    );

    RaycastHit2D rightRay = Physics2D.Raycast(
      rightRayPosition,
      Vector2.down,
      _boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance,
      _playerMovementDataSO.GroundLayerMask
    );

    // For debugging (only appears in the Scene window, not the game window)
    // Debug.DrawRay(leftRayPosition, _boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance * Vector3.down, Color.blue, 1f);
    // Debug.DrawRay(middleRayPosition, _boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance * Vector3.down, Color.blue, 1f);
    // Debug.DrawRay(rightRayPosition, _boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance * Vector3.down, Color.blue, 1f);

    // player is grounded if any of the following rays make contact with an object with on the specified LayerMask stored in _playerMovementDataSO
    return leftRay || middleRay || rightRay;
  }

  private void MovePlayer()
  {
    if (_playerMovementDataSO.PlayerDirectionInput.x != 0)
    {
      if (_inputDirectionLastFrame.x != _playerMovementDataSO.PlayerDirectionInput.x && _playerMovementDataSO.PlayerDirectionInput.x != 0 && _playerMovementDataSO.IntstantaneousTurns)
      {
        _rigidBody2D.linearVelocityX = -_rigidBody2D.linearVelocityX;
      }

      // diff between desired speed and current 
      float delta = _targetSpeed - _rigidBody2D.linearVelocityX;

      float acceleration = _accelerationBase * (_playerMovementDataSO.IsGrounded ? _playerMovementDataSO.AccelerationGround : _playerMovementDataSO.AccelerationAir);

      float force = delta * acceleration;

      Vector2 forceAsVector = new(force * Time.fixedDeltaTime, 0);

      _rigidBody2D.AddForce(forceAsVector, ForceMode2D.Force);
    }
    else
    {
      float deceleration = _playerMovementDataSO.IsGrounded ? _playerMovementDataSO.DecelerationGround : _playerMovementDataSO.DecelerationAir;
      _rigidBody2D.linearVelocityX -= _rigidBody2D.linearVelocityX * deceleration * Time.fixedDeltaTime;
    }
  }

  private void PerformJump()
  {
    if (!_jumpEndEarly && !_playerMovementDataSO.IsGrounded && !_isJumping && _rigidBody2D.linearVelocityY > 0) _jumpEndEarly = true;

    if (_jumpBufferWindow > 0 && _coyoteTime > 0 && _jumpCount > 0)
    {
      _jumpCount = Mathf.Clamp(_jumpCount - 1, 0, _playerMovementDataSO.MaxNumberOfJumps);
      _jumpBufferWindow = 0f;
      _coyoteTime = 0f;

      ExecuteJump();
    }
  }

  private void ExecuteJump()
  {
    _jumpEndEarly = false;
    _rigidBody2D.linearVelocityY = _playerMovementDataSO.JumpingPower;
  }

  private void HandleGravity()
  {
    // If we are falling then we have a different gravity multiplier
    if (_rigidBody2D.linearVelocityY < 0 && !_playerMovementDataSO.IsGrounded)
    {
      UpdateGravityScale(_playerMovementDataSO.FallingGravityMultiplier);
    }
    else
    {
      ResetGravityScale();
    }

    if (_jumpEndEarly) UpdateGravityScale(_playerMovementDataSO.FallingGravityMultiplier * _playerMovementDataSO.ShortJumpGravityMultiplier);

    if (Mathf.Abs(_rigidBody2D.linearVelocityY) < _playerMovementDataSO.JumpHangTimeThreshold)
    {
      UpdateGravityScale(_playerMovementDataSO.JumpHangTimeGravityMultiplier);
    }
  }

  private void ClampPlayerMovement()
  {
    // Clamps velocity by the amount configured in _playerMovementDataSO
    _rigidBody2D.linearVelocityX = Mathf.Clamp(_rigidBody2D.linearVelocityX, -_playerMovementDataSO.VelocityHorizontalClamp, _playerMovementDataSO.VelocityHorizontalClamp);
    _rigidBody2D.linearVelocityY = Mathf.Clamp(_rigidBody2D.linearVelocityY, -_playerMovementDataSO.VelocityVerticalClamp, _playerMovementDataSO.VelocityVerticalClamp);
  }


  private void ResetGravityScale() => _rigidBody2D.gravityScale = _playerMovementDataSO.GravityScale;
  private void UpdateGravityScale(float scale) => _rigidBody2D.gravityScale = _playerMovementDataSO.GravityScale * scale;

}
