using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
  [Header("Data References")]
  [SerializeField] private Rigidbody2D _rigidBody2D;
  [SerializeField] private SpriteRenderer _spriteRenderer;

  [Header("Movement Settings")]
  [SerializeField] private PlayerMovementDataSO _playerMovementDataSO;

  [Header("Debug")]
  [SerializeField, ReadOnly] private float _horizontal;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_playerMovementDataSO == null)
    {
      Debug.LogError("PlayerController does not have defined PlayerMovementDataSO.");
    }

    if (_spriteRenderer == null) _spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
    if (_rigidBody2D == null) _rigidBody2D = gameObject.GetComponent<Rigidbody2D>();

    _rigidBody2D.gravityScale = _playerMovementDataSO.GravityMultipler;
  }

  private void FixedUpdate()
  {
    _rigidBody2D.linearVelocity = new Vector2(
      _horizontal * _playerMovementDataSO.MovementSpeed,
      _rigidBody2D.linearVelocity.y
    );

    _playerMovementDataSO.UpdatePlayerVelocity(_rigidBody2D.linearVelocity);
    _playerMovementDataSO.UpdateIsGrounded(IsGrounded());
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  public void Move(InputAction.CallbackContext context)
  {
    _horizontal = context.ReadValue<Vector2>().x;
  }

  public void Jump(InputAction.CallbackContext context)
  {
    if (context.performed && IsGrounded())
    {
      _rigidBody2D.linearVelocity = new Vector2(
        _rigidBody2D.linearVelocity.x,
        _playerMovementDataSO.JumpingPower
      );
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private bool IsGrounded()
  {
    return Physics2D.OverlapCapsule(transform.position, new Vector2(0.5f, 1f), CapsuleDirection2D.Horizontal, 0, _playerMovementDataSO.GroundLayerMask);
  }

}
