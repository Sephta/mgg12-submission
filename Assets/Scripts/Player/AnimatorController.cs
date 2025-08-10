using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class AnimatorController : MonoBehaviour
{
  [Header("Data References")]
  [SerializeField] private Animator _animator;
  [SerializeField] private SpriteRenderer _spriteRenderer;

  [Header("Movement Settings")]
  [SerializeField, Expandable] private PlayerMovementDataSO _playerMovementDataSO;

  [Header("Debug")]
  [SerializeField, ReadOnly] private bool _isMoving;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_animator == null) _animator = GetComponent<Animator>();
    if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
  }

  private void Update()
  {
    if (_playerMovementDataSO.PlayerDirectionInput.x != 0)
    {
      _spriteRenderer.flipX = _playerMovementDataSO.PlayerDirectionInput.x < 0;
    }

    _isMoving = IsMoving();
    _animator.Play(AnimationSelector(), 0);
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private string AnimationSelector()
  {
    if (_playerMovementDataSO.IsGrounded)
    {
      return _isMoving ? "run" : "idle";
    }
    else
    {
      return "jump";
    }
  }

  private bool IsMoving()
  {
    return _playerMovementDataSO.PlayerDirectionInput.x != 0;
  }

}
