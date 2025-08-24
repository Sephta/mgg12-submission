using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class AnimatorController : MonoBehaviour
{
  [Header("Component References"), Space(10f)]

  [SerializeField] private Animator _animator;
  [SerializeField] private SpriteRenderer _spriteRenderer;
  [SerializeField] private AnimationClip _attackClip;

  [Header("Player Data"), Space(10f)]

  [Required("Must provide a HSMScratchpadSO asset.")]
  [SerializeField] private HSMScratchpadSO _scratchpad;

  [Space(15f)]
  [InfoBox("If the fields bellow are left empty they will be populated from the Scratchpad data at runtime on Awake().")]
  [Space(5f)]

  [SerializeField, Expandable] private PlayerAttributesDataSO _playerAttributesData;
  [SerializeField, Expandable] private PlayerAbilityDataSO _playerAbilityData;
  [SerializeField, Expandable] private PlayerEventDataSO _playerEventData;

  [Header("Animation Data"), Space(10f)]

  [SerializeField] private int _transitionDuration = 0;
  [SerializeField] private int _animationLayer = 0;

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
    COMBAT03
  }

  // Dictionaries are not serializable in the inspector by default in Unity. To get around this
  // we use Unity's "SerializedDictionary" wrapper class. This class itself requires that we 
  // extend it in a new class before use.
  [Serializable]
  public class StringIntDictionary : SerializedDictionary<string, int> { }
  [Serializable]
  public class IntStringDictionary : SerializedDictionary<int, string> { }

  // Marked read only because they should be visible in the inspector but only editable in code.
  // We will map an enumeration defined above to an existing state name hash from the animator.
  // This will make it so that we do not need to rely on the string name of the animation state
  // to play the animation. We can just use the int hash stored in this dictionary.
  [ReadOnly] public StringIntDictionary _animationStates = new();

  // Its probably useful to store an easy way to get the animator state name using the int hash.
  // This container is populated using the button defined bellow (accessible from the inspector)
  private IntStringDictionary _stateHashToName = new();

  [Header("Debug")]
  [SerializeField, ReadOnly] private int _currentAttackAnimationIndex = 0;
  [SerializeField, ReadOnly] private bool _isHoldingAttackButton = false;
  [SerializeField, ReadOnly] private bool _lookForInputToBuffer = false;
  [SerializeField, ReadOnly] private bool _attackBuffer = false;

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

    _playerEventData = _scratchpad.GetScratchpadData<PlayerEventDataSO>();
    if (_playerEventData == null)
    {
      Debug.LogError(name + " does not have a PlayerEventDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_animator == null) _animator = GetComponent<Animator>();
    if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

    // Build/ReBuild the dictionaries on awake.
    PopulateAnimationDictionary();

  }

  private void Start()
  {
    AddAnimationEventsToPlayerArmAttacks();
  }

  private void OnEnable()
  {
    _playerEventData.Attack.OnEventRaised += OnAttack;
  }

  private void OnDisable()
  {
    _playerEventData.Attack.OnEventRaised += OnAttack;
  }

  private void Update()
  {
    FlipSpriteBasedOnPlayerAttributesData();

    // Check which animation we should be playing next.
    int nextAnimationToPlay = AnimationSelector();

    // If the player is in the attacking state we should handle attack animations.
    if (_playerAttributesData.IsAttacking && _playerAbilityData.CurrentlyEquippedArm.CombatAbility != null)
    {
      Debug.Log("looking for input to buffer: " + _lookForInputToBuffer + " , is holding attack: " + _isHoldingAttackButton);
      if (_lookForInputToBuffer && _isHoldingAttackButton)
      {
        _attackBuffer = true;
      }

      if (_playerAbilityData.CurrentlyEquippedArm.CombatAbility.AttackAnimationClips.Count > 0)
      {
        string stateNameInAttackChain = _playerAbilityData.CurrentlyEquippedArm.CombatAbility.AttackAnimationClips[_currentAttackAnimationIndex].name.ToUpper();
        _animator.CrossFade(_animationStates[stateNameInAttackChain], _transitionDuration, _animationLayer);
      }
    }
    else
    {
      // If we're not in the attacking state just handle animations as we would normally.
      _animator.CrossFade(nextAnimationToPlay, _transitionDuration, _animationLayer);
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private void OnAttack(InputAction.CallbackContext context)
  {
    if (_playerAttributesData.IsAttacking)
    {
      if (context.started) _isHoldingAttackButton = true;
      if (context.canceled) _isHoldingAttackButton = false;
    }
  }

  private void HandleAttackInputBuffer()
  {
    Debug.Log("HandleAttackInputBuffer");
    _lookForInputToBuffer = true;
  }

  private void ChainAttackOrFinishCombo()
  {
    Debug.Log("ChainAttackOrFinishCombo, Attack Buffer: " + _attackBuffer);
    _lookForInputToBuffer = false;

    // exit attack state if we have no combat ability with current arm.
    if (_playerAbilityData.CurrentlyEquippedArm.CombatAbility == null)
    {
      ExitAttackState();
    }

    // if we're at the last attack in our combo chain, complete the chain.
    if (_currentAttackAnimationIndex == _playerAbilityData.CurrentlyEquippedArm.CombatAbility.AttackAnimationClips.Count - 1 || !_attackBuffer)
    {
      ExitAttackState();
    }
    else
    {
      if (_attackBuffer)
      {
        _attackBuffer = false;
        _currentAttackAnimationIndex++;
        if (_currentAttackAnimationIndex > _playerAbilityData.CurrentlyEquippedArm.CombatAbility.AttackAnimationClips.Count - 1)
        {
          _currentAttackAnimationIndex = _playerAbilityData.CurrentlyEquippedArm.CombatAbility.AttackAnimationClips.Count - 1;
        }
      }
    }
  }

  private void ExitAttackState()
  {
    _attackBuffer = false;
    _currentAttackAnimationIndex = 0;
    if (_playerAttributesData.IsAttacking) _playerAttributesData.UpdateIsAttacking(false);

    _playerEventData.AttackChainCompleted.RaiseEvent();
  }

  private void AddAnimationEventsToPlayerArmAttacks()
  {
    foreach (NeroArmDataSO armData in _playerAbilityData.ArmData)
    {
      if (armData.CombatAbility != null && armData.CombatAbility.AttackAnimationClips.Count > 0)
      {
        List<AnimationClip> attackAnimationClips = armData.CombatAbility.AttackAnimationClips;
        foreach (AnimationClip clip in attackAnimationClips)
        {
          float timeDurringClipToRaiseInputBufferEvent = clip.length - armData.CombatAbility.AttackChainingInputBuffer;

          AnimationEvent startInputBuffer = new()
          {
            time = timeDurringClipToRaiseInputBufferEvent,
            functionName = nameof(HandleAttackInputBuffer)
          };

          clip.AddEvent(startInputBuffer);

          AnimationEvent endOfAttackEvent = new()
          {
            time = clip.length,
            functionName = nameof(ChainAttackOrFinishCombo)
          };

          clip.AddEvent(endOfAttackEvent);
        }
      }
    }
  }

  private void PopulateAnimationDictionary()
  {
    // Wipe them away
    _animationStates.Clear();
    _stateHashToName.Clear();

    // Build/Re-Build them
    foreach (AnimationStates animationState in (AnimationStates[])Enum.GetValues(typeof(AnimationStates)))
    {
      int hashedStateName = Animator.StringToHash(animationState.ToString().ToLower());
      _animationStates.Add(animationState.ToString(), hashedStateName);
      _stateHashToName.Add(hashedStateName, animationState.ToString().ToLower());
    }
  }

  private int AnimationSelector()
  {
    if (_playerAttributesData.IsGrounded)
    {
      if (_playerAttributesData.IsTakingAim)
      {
        return _animationStates[nameof(AnimationStates.AIM)];
      }

      return IsMoving() ? _animationStates[nameof(AnimationStates.RUN)] : _animationStates[nameof(AnimationStates.IDLE)];
    }
    else
    {
      if (IsFalling()) return _animationStates[nameof(AnimationStates.FALLING)];
      return _animationStates[nameof(AnimationStates.JUMP)];
    }
  }

  private bool IsMoving() => _playerAttributesData.PlayerMoveDirection.x != 0;

  private bool IsFalling() => _playerAttributesData.PlayerVelocity.y < 0 && !_playerAttributesData.IsGrounded;

  private void FlipSpriteBasedOnPlayerAttributesData()
  {
    if (_playerAttributesData.IsAttacking) return;

    if (_playerAttributesData.IsTakingAim)
    {
      if (_playerAttributesData.PlayerAimDirection.x != 0)
        _spriteRenderer.flipX = _playerAttributesData.PlayerAimDirection.x < 0;
    }
    else if (_playerAttributesData.PlayerMoveDirection.x != 0)
    {
      _spriteRenderer.flipX = _playerAttributesData.PlayerMoveDirection.x < 0;
    }
  }

}
