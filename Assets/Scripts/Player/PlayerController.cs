using System;
using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
  [Header("Data References")]
  [SerializeField] private Rigidbody2D _rigidBody2D;
  [SerializeField] private CapsuleCollider2D _capsuleCollider2D;

  [Header("Movement Settings")]
  [SerializeField, Expandable] private PlayerMovementDataSO _playerMovementDataSO;

  [Header("Debug")]
  [SerializeField, ReadOnly] private Vector2 _inputDirection;
  [SerializeField, ReadOnly] private float _jumpBufferWindow;
  [SerializeField, ReadOnly] private float _coyoteTime;
  [SerializeField, ReadOnly] private int _jumpCount;
  [SerializeField, ReadOnly] private float _dragToApply;
  [SerializeField, ReadOnly] private bool _isJumping;
  [SerializeField, ReadOnly] private bool _wasGroundedLastFrame;

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
    if (_capsuleCollider2D == null) _capsuleCollider2D = GetComponent<CapsuleCollider2D>();
  }

  private void Start()
  {
    _rigidBody2D.gravityScale = _playerMovementDataSO.GravityScale;
    _jumpCount = _playerMovementDataSO.MaxNumberOfJumps;
  }

  private void Update()
  {
    _playerMovementDataSO.UpdateIsGrounded(IsGrounded());
    _playerMovementDataSO.UpdatePlayerVelocity(_rigidBody2D.linearVelocity);

    if (!_playerMovementDataSO.IsGrounded && _wasGroundedLastFrame)
    {
      _coyoteTime = _playerMovementDataSO.CoyoteTime;
    }

    if (!_wasGroundedLastFrame && _playerMovementDataSO.IsGrounded)
    {
      _jumpCount = _playerMovementDataSO.MaxNumberOfJumps;
    }

    _jumpBufferWindow = Mathf.Clamp(_jumpBufferWindow - Time.deltaTime, 0f, _playerMovementDataSO.JumpInputBuffer);
    _coyoteTime = Mathf.Clamp(_coyoteTime - Time.deltaTime, 0f, _playerMovementDataSO.CoyoteTime);

    MovePlayer();
    PerformJump();
    JumpHangTime();
    ClampPlayerMovement();

    _wasGroundedLastFrame = IsGrounded();
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  // For used in the Player Input component.
  public void OnMove(InputAction.CallbackContext context)
  {
    _inputDirection = context.ReadValue<Vector2>();
    _playerMovementDataSO.UpdatePlayerDirectionInput(context.ReadValue<Vector2>());
  }

  // For use in the Player Input component.
  public void OnJump(InputAction.CallbackContext context)
  {
    if (context.started) _isJumping = true;
    if (context.canceled) _isJumping = false;
    if (context.performed)
    {
      _jumpBufferWindow = _playerMovementDataSO.JumpInputBuffer;
      // Debug.Log("Is grounded: " + _playerMovementDataSO.IsGrounded + " jumpCount: " + _jumpCount);
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private bool IsGrounded()
  {
    return _rigidBody2D.IsTouching(_playerMovementDataSO.GroundingContactFilter);
  }

  private void MovePlayer()
  {
    _dragToApply = _playerMovementDataSO.IsGrounded ? _playerMovementDataSO.GroundDrag : _playerMovementDataSO.AirDrag;

    if (_playerMovementDataSO.PlayerDirectionInput.x != 0)
    {
      _rigidBody2D.linearVelocityX = _playerMovementDataSO.PlayerDirectionInput.x * _playerMovementDataSO.MovementSpeed * Time.fixedDeltaTime;
    }
    else
    {
      _rigidBody2D.linearVelocityX -= _rigidBody2D.linearVelocityX * _dragToApply * Time.fixedDeltaTime;
    }

    // If we are falling then we have a different gravity multiplier
    if (_rigidBody2D.linearVelocityY < 0 && !_playerMovementDataSO.IsGrounded)
    {
      _rigidBody2D.gravityScale = _playerMovementDataSO.GravityScale * _playerMovementDataSO.GravityMultiplierWhenFalling;
    }
    else
    {
      _rigidBody2D.gravityScale = _playerMovementDataSO.GravityScale;
    }
  }

  private void PerformJump()
  {
    if (_jumpBufferWindow > 0 && (_coyoteTime > 0 || _playerMovementDataSO.IsGrounded) && _jumpCount > 0)
    {
      _jumpCount = Mathf.Clamp(_jumpCount - 1, 0, _playerMovementDataSO.MaxNumberOfJumps);
      _jumpBufferWindow = 0f;

      float finalJumpForce = _playerMovementDataSO.JumpingPower;
      if (_rigidBody2D.linearVelocityY < 0) finalJumpForce -= _rigidBody2D.linearVelocityY;

      _rigidBody2D.AddForce(finalJumpForce * Vector2.up, ForceMode2D.Impulse);
      // Debug.Log("JUMP PERFORMED A, new jumpCount: " + _jumpCount);
      Debug.Log("JUMP PERFORMED");
    }
  }

  private void JumpHangTime()
  {
    if (Mathf.Abs(_rigidBody2D.linearVelocityY) < _playerMovementDataSO.JumpHangTimeThreshold)
    {
      _rigidBody2D.gravityScale = _playerMovementDataSO.GravityScale * _playerMovementDataSO.JumpHangTimeGravityMultiplier;
    }
  }

  private void ClampPlayerMovement()
  {
    // Clamps vertical velocity by the amount configured in _playerMovementDataSO
    _rigidBody2D.linearVelocityY = Mathf.Clamp(_rigidBody2D.linearVelocityY, -_playerMovementDataSO.MaxFallingSpeed, _playerMovementDataSO.MaxFallingSpeed);
  }

}
