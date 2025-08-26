using System;
using System.Collections.Generic;
using NaughtyAttributes;
using stal.Helpers.Animation;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class PlayerAnimatorController : MonoBehaviour
{
  [Header("Component References"), Space(10f)]

  [SerializeField] private AnimatorComponentReferences _componentRefs = new();

  [Header("Player Data"), Space(10f)]

  [Required("Must provide a HSMScratchpadSO asset.")]
  [SerializeField] private HSMScratchpadSO _scratchpad;

  [Space(15f)]
  [InfoBox("If the fields bellow are left empty they will be populated from the Scratchpad data at runtime on Awake().")]
  [Space(5f)]

  [SerializeField, Expandable] private PlayerAttributesDataSO _playerAttributesData;
  [SerializeField, Expandable] private PlayerAbilityDataSO _playerAbilityData;

  [Header("Animation Data"), Space(10f)]

  [SerializeField] private int _transitionDuration = 0;
  [SerializeField] private int _animationLayer = 0;
  [SerializeField, Expandable] private PlayerAnimationStatesSO _animationStates;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_scratchpad == null)
    {
      Debug.LogError(name + " does not have a HSMScratchpadSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _playerAttributesData = _scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
    if (_playerAttributesData == null)
    {
      Debug.LogError(name + " does not have a PlayerAttributesDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _playerAbilityData = _scratchpad.GetScratchpadData<PlayerAbilityDataSO>();
    if (_playerAbilityData == null)
    {
      Debug.LogError(name + " does not have a PlayerAbilityDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_playerAbilityData.ArmData.Count <= 0)
    {
      Debug.LogError(name + " contains empty PlayerAbilityDataSO.ArmData. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_componentRefs.animator == null) _componentRefs.animator = GetComponent<Animator>();
    if (_componentRefs.spriteRenderer == null) _componentRefs.spriteRenderer = GetComponent<SpriteRenderer>();
  }

  // private void Start() {}

  private void Update()
  {
    if (!_playerAttributesData.IsAttacking)
    {
      // If we're not in the attacking state just handle animations as we would normally.
      _componentRefs.animator.CrossFade(AnimationSelector(), _transitionDuration, _animationLayer);
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private int AnimationSelector()
  {
    if (_playerAttributesData.IsNeedling) return _animationStates.StateNameToHash[nameof(AnimationStates.ENVIRON01)];
    if (_playerAttributesData.IsLatchedOntoWall) return _animationStates.StateNameToHash[nameof(AnimationStates.WALL_CLING)];

    if (_playerAttributesData.IsGrounded)
    {
      if (_playerAttributesData.IsTakingAim && _playerAbilityData.CurrentlyEquippedArmType == NeroArmType.Neutral)
      {
        Debug.Log("State name and hash: <" + nameof(AnimationStates.AIM) + " : " + _animationStates.StateNameToHash[nameof(AnimationStates.AIM)] + ">");
        return _animationStates.StateNameToHash[nameof(AnimationStates.AIM)];
      }

      return IsMoving() ? _animationStates.StateNameToHash[nameof(AnimationStates.RUN)] : _animationStates.StateNameToHash[nameof(AnimationStates.IDLE)];
    }
    else
    {
      if (IsFalling()) return _animationStates.StateNameToHash[nameof(AnimationStates.FALLING)];
      return _animationStates.StateNameToHash[nameof(AnimationStates.JUMP)];
    }
  }

  private bool IsMoving() => _playerAttributesData.PlayerMoveDirection.x != 0;

  private bool IsFalling() => _playerAttributesData.PlayerVelocity.y < 0 && !_playerAttributesData.IsGrounded;

}
