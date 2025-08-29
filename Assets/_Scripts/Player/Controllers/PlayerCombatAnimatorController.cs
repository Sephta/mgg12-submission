using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class PlayerCombatAnimatorController : MonoBehaviour
{
  [Header("Component References"), Space(10f)]
  [SerializeField] private CombatAnimatorComponentReferences _componentRefs = new();

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
  [SerializeField, Expandable] private PlayerAnimationStatesSO _animationStates;

  [Space(10f)]
  [Header("Debug")]
  [SerializeField, ReadOnly] private int _currentAttackAnimationIndex = 0;
  [SerializeField, ReadOnly] private bool _isHoldingAttackButton = false;
  [SerializeField, ReadOnly] private bool _lookForInputToBuffer = false;
  [SerializeField, ReadOnly] private bool _attackBuffer = false;
  private readonly float _attackMoveForceBase = 1;

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

    if (_componentRefs.animator == null) _componentRefs.animator = GetComponent<Animator>();
    if (_componentRefs.spriteRenderer == null) _componentRefs.spriteRenderer = GetComponent<SpriteRenderer>();

  }

  // private void Start() {}

  private void OnEnable()
  {
    _currentAttackAnimationIndex = 0;
    _isHoldingAttackButton = false;
    _lookForInputToBuffer = false;
    _attackBuffer = false;

    ExitAttackState();
    if (_componentRefs.playerHitZone != null)
    {
      _componentRefs.playerHitZone.enabled = false;
    }

    OnPlayerArmFinishedCycling();
    SetAttackAnimationSpeedForCurrentAnimator();

    _playerEventData.Attack.OnEventRaised += OnAttack;
    _playerEventData.PlayerArmFinishedCycling.OnEventRaised += OnPlayerArmFinishedCycling;
  }

  private void OnDisable()
  {
    _playerEventData.Attack.OnEventRaised -= OnAttack;
    _playerEventData.PlayerArmFinishedCycling.OnEventRaised -= OnPlayerArmFinishedCycling;
  }

  private void Update()
  {
    // If the player is in the attacking state we should handle attack animations.
    if (_playerAttributesData.IsAttacking && _playerAbilityData.CurrentlyEquippedArm.CombatAbility != null)
    {
      CombatAbilitySO combatAbilityData = _playerAbilityData.CurrentlyEquippedArm.CombatAbility;

      if (_lookForInputToBuffer && _isHoldingAttackButton)
      {
        _attackBuffer = true;
      }

      List<AnimationClip> attackClips = combatAbilityData.AttackAnimationClips;

      if (attackClips.Count > 0)
      {
        string stateNameInAttackChain = attackClips[_currentAttackAnimationIndex].name.ToUpper();
        AnimatorStateInfo stateInfo = _componentRefs.animator.GetCurrentAnimatorStateInfo(_animationLayer);
        if (!(stateInfo.shortNameHash == _animationStates.StateNameToHash[stateNameInAttackChain]))
        {
          _componentRefs.animator.CrossFade(_animationStates.StateNameToHash[stateNameInAttackChain], _transitionDuration, _animationLayer);

          if (_componentRefs.playerRigidBody != null)
          {
            // If we have the "IgnoreMoveDirection" toggle enabled we want to avoid using the player's movement input direction
            // as the direction of our attack force. Instead we apply the force in the direction we're attacking.
            Vector3 directionOfAttack = combatAbilityData.IgnoreMoveDirection ? (_componentRefs.playerHitZone.transform.position - transform.position).normalized : _playerAttributesData.PlayerMoveDirection;
            Vector2 forceToApply = combatAbilityData.AttackMovementForce * _attackMoveForceBase * directionOfAttack.x * Vector2.right;
            _componentRefs.playerRigidBody.AddForce(forceToApply, ForceMode2D.Impulse);
          }
        }
      }
    }
  }

  // private void FixedUpdate() {}

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

  private void OnPlayerArmFinishedCycling()
  {
    if (_playerAbilityData.CurrentlyEquippedArm != null)
    {
      _componentRefs.animator.runtimeAnimatorController = _playerAbilityData.CurrentlyEquippedArm.RuntimeAnimatorController;
      SetAttackAnimationSpeedForCurrentAnimator();
    }
  }

  private void EnablePlayerHitzone()
  {
    if (_componentRefs.playerHitZone == null) return;

    if (_playerAttributesData.IsAttacking) _componentRefs.playerHitZone.enabled = true;
  }

  private void DisablePlayerHitzone()
  {
    if (_componentRefs.playerHitZone == null) return;

    _componentRefs.playerHitZone.enabled = false;
  }

  private void HandleAttackInputBuffer()
  {
    _lookForInputToBuffer = true;
  }

  private void ChainAttackOrFinishCombo()
  {
    DisablePlayerHitzone();

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
    _playerEventData.AttackChainCompleted.RaiseEvent();
  }

  private void SetAttackAnimationSpeedForCurrentAnimator()
  {
    if (_componentRefs.animator == null) return;
    if (_playerAbilityData.CurrentlyEquippedArm.CombatAbility == null) return;

    _componentRefs.animator.SetFloat("speed", _playerAbilityData.CurrentlyEquippedArm.CombatAbility.Speed);
  }
}
