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
  [SerializeField, Range(0f, 1f)] private float _groundingRaycastDistance = 0.25f;
  [SerializeField, ReadOnly] private Vector2 _inputDirection;
  [SerializeField, ReadOnly] private float _timeSinceJumpPressed;
  [SerializeField, ReadOnly] private float _timeSinceGrounded;
  [SerializeField, ReadOnly] private int _jumpCount;

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

    _timeSinceJumpPressed = Mathf.Clamp(_timeSinceJumpPressed - Time.deltaTime, 0f, _playerMovementDataSO.JumpInputBuffer);
    _timeSinceGrounded = Mathf.Clamp(_timeSinceGrounded - Time.deltaTime, 0f, _playerMovementDataSO.CoyoteTime);

    MovePlayer();
    PerformJump();
    ClampPlayerMovement();
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  public void OnMove(InputAction.CallbackContext context)
  {
    _inputDirection = context.ReadValue<Vector2>();
    _playerMovementDataSO.UpdatePlayerDirectionInput(_inputDirection);
  }

  public void OnJump(InputAction.CallbackContext context)
  {
    if (context.performed)
    {
      _timeSinceJumpPressed = _playerMovementDataSO.JumpInputBuffer;
      Debug.Log("Is grounded: " + _playerMovementDataSO.IsGrounded + " jumpCount: " + _jumpCount);
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private void MovePlayer()
  {
    _rigidBody2D.linearVelocityX = _inputDirection.x * _playerMovementDataSO.MovementSpeed * Time.fixedDeltaTime;

    // If we are falling then we have a different gravity multiplier
    if (_rigidBody2D.linearVelocityY < 0)
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
    if (_playerMovementDataSO.IsGrounded && _timeSinceJumpPressed > 0 && _jumpCount > 0)
    {
      Debug.Log("JUMP PERFORMED");
      _jumpCount = Mathf.Clamp(_jumpCount - 1, 0, _playerMovementDataSO.MaxNumberOfJumps);
      _timeSinceJumpPressed = 0;
      _rigidBody2D.AddForce(_playerMovementDataSO.JumpingPower * Vector2.up, ForceMode2D.Impulse);
    }
  }

  private void ClampPlayerMovement()
  {
    _rigidBody2D.linearVelocityY = Mathf.Clamp(_rigidBody2D.linearVelocityY, -_playerMovementDataSO.MaxFallingSpeed, _playerMovementDataSO.MaxFallingSpeed);
  }

  private bool IsGrounded()
  {
    RaycastHit2D hit = Physics2D.Raycast(
      new Vector2(transform.position.x, transform.position.y),
      Vector2.down,
      _capsuleCollider2D.bounds.extents.y + _groundingRaycastDistance,
      _playerMovementDataSO.GroundLayerMask
    );

    Debug.DrawRay(transform.position, Vector3.down, Color.blue, 1f);

    if (hit)
    {
      _jumpCount = _playerMovementDataSO.MaxNumberOfJumps;
      _timeSinceGrounded = _playerMovementDataSO.CoyoteTime;
    }

    return _timeSinceGrounded > 0;
  }

}
