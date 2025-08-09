using UnityEngine;

public class AnimatorController : MonoBehaviour
{
  [Header("Data References")]
  [SerializeField] private Animator _animator;
  [SerializeField] private SpriteRenderer _spriteRenderer;

  [Header("Movement Settings")]
  [SerializeField] private PlayerMovementDataSO _playerMovementDataSO;

  [Header("Debug")]
  [SerializeField, ReadOnly] private bool _isMoving;
  [SerializeField, ReadOnly] private bool _isGrounded;


  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  void Awake()
  {
    if (_animator == null) _animator = GetComponent<Animator>();
    if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
  }

  void FixedUpdate()
  {
    _isMoving = IsMoving();
    _isGrounded = IsGrounded();
    _animator.Play(AnimationSelector(), 0);

    if (_playerMovementDataSO.PlayerVelocity.x != 0)
    {
      _spriteRenderer.flipX = _playerMovementDataSO.PlayerVelocity.x < 0;
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  public void OnMove()
  {
    _animator.Play("run", 0);
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private string AnimationSelector()
  {
    if (_isGrounded && !_isMoving)
    {
      return "idle";
    }

    if (_isGrounded && _isMoving)
    {
      return "run";
    }

    if (!_isGrounded)
    {
      return "jump";
    }

    return "idle";
  }

  private bool IsGrounded()
  {
    return Physics2D.OverlapCapsule(transform.position, new Vector2(0.5f, 1f), CapsuleDirection2D.Horizontal, 0, _playerMovementDataSO.GroundLayerMask);
  }

  private bool IsMoving()
  {
    return _playerMovementDataSO.PlayerVelocity.y != 0 && _playerMovementDataSO.PlayerVelocity.x != 0;
  }

}
