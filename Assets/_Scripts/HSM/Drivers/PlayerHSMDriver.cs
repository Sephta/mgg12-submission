using System;
using System.Linq;
using NaughtyAttributes;
using stal.HSM.Contexts;
using stal.HSM.Core;
using stal.HSM.PlayerStates;
using UnityEngine;
using UnityEngine.InputSystem;

namespace stal.HSM.Drivers
{
  [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
  public class PlayerHSMDriver : MonoBehaviour
  {
    [Space(10f)]
    [Header("State Machine Context")]
    [Space(10)]

    [SerializeField] private PlayerContext _playerContext = new();

    [Space(15)]
    [Header("State Machine Scratchpad")]
    [Space(10)]

    [Required("Must provide a HSMScratchpadSO asset.")]
    [SerializeField, Expandable] private HSMScratchpadSO _scratchpad;
    private PlayerMovementDataSO _playerMovementData;
    private PlayerAttributesDataSO _playerAttributesData;
    private PlayerAbilityDataSO _playerAbilityData;
    private PlayerEventDataSO _playerEventData;

    // State Machine Specific Fields
    private HierarchicalStateMachine _stateMachine;
    private State _rootState;

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

      _playerMovementData = _scratchpad.GetScratchpadData<PlayerMovementDataSO>();
      if (_playerMovementData == null)
      {
        Debug.LogError(name + " does not have a PlayerMovementDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
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

      if (_playerContext.transform == null) _playerContext.transform = transform;
      if (_playerContext.rigidbody2D == null) _playerContext.rigidbody2D = GetComponent<Rigidbody2D>();
      if (_playerContext.boxCollider2D == null) _playerContext.boxCollider2D = GetComponent<BoxCollider2D>();

      // if null, grab main camera. If still null (could not find main camera) then disable gameobject.
      if (_playerContext.mainCamera == null) _playerContext.mainCamera = Camera.main;
      if (_playerContext.mainCamera == null)
      {
        Debug.LogError(name + " could not find main camera. Deactivating object to avoid null object errors.");
        gameObject.SetActive(false);
      }

      if (_playerContext.bramble == null)
      {
        Debug.LogError(name + " does not have a prefab selected for spawning bramble. Deactiving object to avoid null object errors.");
        gameObject.SetActive(false);
      }

      _rootState = new PlayerRoot(null, _playerContext, _scratchpad);
      HierarchicalStateMachineBuilder stateMachineBuilder = new(_rootState);
      _stateMachine = stateMachineBuilder.BuildStateMachine();
    }

    private void OnEnable()
    {
      // Register Input Events
      _playerEventData.Move.OnEventRaised += OnMove;
      _playerEventData.Jump.OnEventRaised += OnJump;
      _playerEventData.TakeAim.OnEventRaised += OnTakeAim;
      _playerEventData.ConfirmAim.OnEventRaised += OnConfirmAim;
      _playerEventData.Look.OnEventRaised += OnLook;
      _playerEventData.SwapArmLeft.OnEventRaised += OnSwapArmLeft;
      _playerEventData.SwampArmRight.OnEventRaised += OnSwapArmRight;

      _playerAttributesData.ResetPlayerAttributesData();
    }

    private void OnDisable()
    {
      //Un-Register Input Events
      _playerEventData.Move.OnEventRaised -= OnMove;
      _playerEventData.Jump.OnEventRaised -= OnJump;
      _playerEventData.TakeAim.OnEventRaised -= OnTakeAim;
      _playerEventData.ConfirmAim.OnEventRaised -= OnConfirmAim;
      _playerEventData.Look.OnEventRaised -= OnLook;
      _playerEventData.SwapArmLeft.OnEventRaised -= OnSwapArmLeft;
      _playerEventData.SwampArmRight.OnEventRaised -= OnSwapArmRight;

      _playerAttributesData.ResetPlayerAttributesData();
    }

    private void Start()
    {
      _playerContext.jumpCount = _playerMovementData.JumpMaximum;
    }

    private void Update()
    {
      UpdateMousePositionData();
      _playerAttributesData.UpdatePlayerVelocity(_playerContext.rigidbody2D.linearVelocity);
      _playerAttributesData.UpdateIsGrounded(IsGrounded());

      // Update timers
      _playerContext.jumpBufferWindow = Mathf.Clamp(_playerContext.jumpBufferWindow - Time.deltaTime, 0f, _playerMovementData.JumpInputBuffer);
      _playerContext.coyoteTime = Mathf.Clamp(_playerContext.coyoteTime - Time.deltaTime, 0f, _playerMovementData.CoyoteTime);

      // Reset timers 
      if (_playerAttributesData.IsGrounded) _playerContext.coyoteTime = _playerMovementData.CoyoteTime;
      if (!_playerContext.wasGroundedLastFrame && _playerAttributesData.IsGrounded) _playerContext.jumpCount = _playerMovementData.JumpMaximum;

      // Tick the state machine every frame
      _stateMachine.Tick(Time.deltaTime);

      UpdateLoopBookKeeping();

      // Draw Debug Gizmos
      DrawDebugGizmos();

      Vector2 rayDirection = Vector2.zero;
      if (_playerAttributesData.PlayerMoveDirection != Vector2.zero)
      {
        if (Mathf.Abs(_playerAttributesData.PlayerMoveDirection.x) > Mathf.Abs(_playerAttributesData.PlayerMoveDirection.y))
        {
          rayDirection.x = Mathf.Sign(_playerAttributesData.PlayerMoveDirection.x) >= 0 ? 1f : -1f;
        }
        else
        {
          rayDirection.y = Mathf.Sign(_playerAttributesData.PlayerMoveDirection.y) >= 0 ? 1f : -1f;
        }
      }

      _playerContext.needleRayDirection = rayDirection;
    }

    /* ---------------------------------------------------------------- */
    /*                               PUBLIC                             */
    /* ---------------------------------------------------------------- */

    // For used in the Player Input component.
    private void OnMove(InputAction.CallbackContext context) => _playerAttributesData.UpdatePlayerDirectionInput(context.ReadValue<Vector2>());

    // For use in the Player Input component.
    private void OnJump(InputAction.CallbackContext context)
    {
      if (context.started) _playerAttributesData.UpdateIsJumping(true);
      if (context.canceled) _playerAttributesData.UpdateIsJumping(false);
      if (context.performed)
      {
        _playerContext.jumpBufferWindow = _playerMovementData.JumpInputBuffer;
      }
    }

    private void OnTakeAim(InputAction.CallbackContext context)
    {
      if (context.started) _playerAttributesData.UpdateIsTakingAim(true);
      if (context.canceled) _playerAttributesData.UpdateIsTakingAim(false);
    }

    private void OnConfirmAim(InputAction.CallbackContext context)
    {
      if (context.started) _playerAttributesData.UpdateIsConfirmingAim(true);
      if (context.canceled) _playerAttributesData.UpdateIsConfirmingAim(false);
    }

    private void OnLook(InputAction.CallbackContext context)
    {
      if (context.control.name == "position")
      {
        // If the control being read is position the vector 2 will represent a
        // screenspace coordinate. (i.e. x,y could be between 0,0 and 1280,720
        // for a 720p resolution screen.)
        _playerAttributesData.UpdatePlayerMousePosition(context.ReadValue<Vector2>());
      }
      else if (context.control.name == "rightStick")
      {
        // If the control being read is "rightStrick" then the vector 2 will be value between
        // -1 and 1 on both axis, where y is the axis of moving the right analog stick up and down
        // and x is the axis of moving the right analog stick left and right.
        _playerAttributesData.UpdatePlayerAimDirection(context.ReadValue<Vector2>().normalized);
      }
    }

    private void OnSwapArmLeft(InputAction.CallbackContext context)
    {
      if (context.started && !_playerAttributesData.IsAttacking)
      {
        _playerAbilityData.CycleArmLeft();
      }
    }

    private void OnSwapArmRight(InputAction.CallbackContext context)
    {
      if (context.started && !_playerAttributesData.IsAttacking)
      {
        _playerAbilityData.CycleArmRight();
      }
    }

    public static string PlayerStatePathToString(State state)
    {
      return string.Join(">", state.PathToRoot().Reverse().Select(foo => foo.GetType().Name));
    }

    /* ---------------------------------------------------------------- */
    /*                               PRIVATE                            */
    /* ---------------------------------------------------------------- */

    private bool IsGrounded()
    {
      // using three distinct rays, the middle of which casts from player's origin position, 
      // while the left and right cast from sids of the player's collider

      Vector3 leftRayPosition = new(_playerContext.transform.position.x - _playerContext.boxCollider2D.bounds.extents.x, _playerContext.transform.position.y, _playerContext.transform.position.z);
      Vector3 middleRayPosition = _playerContext.transform.position;
      Vector3 rightRayPosition = new(_playerContext.transform.position.x + _playerContext.boxCollider2D.bounds.extents.x, _playerContext.transform.position.y, _playerContext.transform.position.z);

      RaycastHit2D leftRay = Physics2D.Raycast(
        leftRayPosition,
        Vector2.down,
        _playerContext.boxCollider2D.bounds.extents.y * _playerMovementData.GroundingRayCastDistance,
        _playerMovementData.LayersConsideredForGroundingPlayer
      );

      RaycastHit2D middleRay = Physics2D.Raycast(
        middleRayPosition,
        Vector2.down,
        _playerContext.boxCollider2D.bounds.extents.y * _playerMovementData.GroundingRayCastDistance,
        _playerMovementData.LayersConsideredForGroundingPlayer
      );

      RaycastHit2D rightRay = Physics2D.Raycast(
        rightRayPosition,
        Vector2.down,
        _playerContext.boxCollider2D.bounds.extents.y * _playerMovementData.GroundingRayCastDistance,
        _playerMovementData.LayersConsideredForGroundingPlayer
      );

      if (_playerContext.drawDebugGizmos)
      {
        // For debugging (only appears in the Scene window, not the game window)
        Debug.DrawRay(leftRayPosition, _playerContext.boxCollider2D.bounds.extents.y * _playerMovementData.GroundingRayCastDistance * Vector3.down, Color.red, 1f);
        Debug.DrawRay(middleRayPosition, _playerContext.boxCollider2D.bounds.extents.y * _playerMovementData.GroundingRayCastDistance * Vector3.down, Color.red, 1f);
        Debug.DrawRay(rightRayPosition, _playerContext.boxCollider2D.bounds.extents.y * _playerMovementData.GroundingRayCastDistance * Vector3.down, Color.red, 1f);
      }

      // player is grounded if any of the following rays make contact with an object with on the specified LayerMask stored in _playerMovementData
      return leftRay || middleRay || rightRay;
    }

    private void UpdateMousePositionData()
    {
      if (_playerAttributesData.PlayerMousePosition != Vector2.zero)
      {
        _playerContext.mouseScreenToWorldPos = _playerContext.mainCamera.ScreenToWorldPoint(_playerAttributesData.PlayerMousePosition);
        Vector2 player2DPosition = new(_playerContext.transform.position.x, _playerContext.transform.position.y);
        _playerAttributesData.UpdatePlayerAimDirection((_playerContext.mouseScreenToWorldPos - player2DPosition).normalized);
      }
    }

    private void DrawDebugGizmos()
    {
      if (_playerContext.drawDebugGizmos)
      {
        Debug.DrawRay(_playerContext.transform.position + (Vector3.up * 0.5f), _playerAttributesData.PlayerAimDirection * _playerMovementData.AbilityAimRaycastDistance, Color.darkGreen, 0.1f);
      }
    }

    private void UpdateLoopBookKeeping()
    {
      // cache grounded state at the end of this frame since next frame we might not be grounded.
      _playerContext.wasGroundedLastFrame = IsGrounded();

      _playerContext.statePath = PlayerStatePathToString(_stateMachine.Root.Leaf());

      if (_playerContext.statePath != _playerContext.previousStatePath)
      {
        Debug.Log("Path Update: " + _playerContext.statePath);
        _playerContext.previousStatePath = _playerContext.statePath;
      }
    }
  }
}
