using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Rendering;

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

  // Define animation states here. These should be the names of each node 
  // in the Animator graph that the aseprite importer generates.
  private enum AnimationStates
  {
    IDLE,
    RUN,
    JUMP,
    FALLING,
    AIM,
    ENVIRON01,
    COMBAT01,
    COMBAT02,
    COMABT03
  }

  // Dictionaries are not serializable in the inspector by default in Unity. To get around this
  // we use Unity's "SerializedDictionary" wrapper class. This class itself requires that we 
  // extend it in a new class before use.
  [Serializable]
  public class StringIntDictionary : SerializedDictionary<string, int> { }

  // Marked read only because they should be visible in the inspector but only editable in code (see above enumeration^).
  [ReadOnly]
  public StringIntDictionary _animationStates;

  [Button("Reset and Populate Animation Dictionary Based on Enumeration")]
  private void PopulateAnimationDictionary()
  {
    // Wipe them away
    _animationStates.Clear();

    foreach (AnimationStates animationState in (AnimationStates[])Enum.GetValues(typeof(AnimationStates)))
    {
      _animationStates.Add(animationState.ToString(), Animator.StringToHash(animationState.ToString().ToLower()));
    }
  }

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_animator == null) _animator = GetComponent<Animator>();
    if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();


    if (_animationStates.Count == 0)
    {
      PopulateAnimationDictionary();
    }

  }

  private void Update()
  {
    FlipSpriteBasedOnPlayerInput();

    _isMoving = IsMoving();

    int transitionDuration = 0;
    int animationLayer = 0;
    _animator.CrossFade(AnimationSelector(), transitionDuration, animationLayer);
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private int AnimationSelector()
  {
    if (_playerMovementDataSO.IsGrounded)
    {
      if (_playerMovementDataSO.IsTakingAim)
      {
        return _animationStates[nameof(AnimationStates.AIM)];
      }
      else if (_playerMovementDataSO.IsAttacking)
      {
        return _animationStates[nameof(AnimationStates.COMBAT01)];
      }
      else
      {
        return _isMoving ? _animationStates[nameof(AnimationStates.RUN)] : _animationStates[nameof(AnimationStates.IDLE)];
      }
    }
    else
    {
      if (IsFalling()) return _animationStates[nameof(AnimationStates.FALLING)];
      return _animationStates[nameof(AnimationStates.JUMP)];
    }
  }

  private bool IsFalling()
  {
    return _playerMovementDataSO.PlayerVelocity.y < 0 && !_playerMovementDataSO.IsGrounded;
  }

  private bool IsMoving()
  {
    return _playerMovementDataSO.PlayerMoveDirection.x != 0;
  }

  private void FlipSpriteBasedOnPlayerInput()
  {
    if (_playerMovementDataSO.IsAttacking) return;

    if (_playerMovementDataSO.PlayerMoveDirection.x != 0)
    {
      _spriteRenderer.flipX = _playerMovementDataSO.PlayerMoveDirection.x < 0;
    }
  }

}
