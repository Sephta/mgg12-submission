using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using stal.HSM.Core;
using stal.HSM.PlayerStates;
using UnityEngine;
using UnityEngine.InputSystem;

namespace stal.HSM.Drivers
{
  [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
  public class PlayerHSMDriver : MonoBehaviour
  {
    [Header("Context")]
    [SerializeField] private PlayerContext _playerContext = new();

    [Space(10f)]
    [Header("HSM Driver Fields")]
    // [SerializeField, Expandable] private HSMScratchpadSO _scratchPad;
    [SerializeField, Expandable] private PlayerMovementDataSO _playerMovementDataSO;

    [Space(10f)]
    [Header("Debug")]
    [SerializeField, ReadOnly] private string _statePath;
    private string _previousStatePath;
    [SerializeField, ReadOnly] private Vector2 _mouseScreenToWorldPos;
    [SerializeField, ReadOnly] private Vector2 _aimDirection;

    private HierarchicalStateMachine _stateMachine;
    private State _rootState;

    /* ---------------------------------------------------------------- */
    /*                           Unity Functions                        */
    /* ---------------------------------------------------------------- */

    private void Awake()
    {
      if (_playerMovementDataSO == null)
      {
        Debug.LogError(name + " does not have defined " + _playerMovementDataSO.GetType().Name + ".  Deactivating object to avoid null object errors.");
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

      _rootState = new PlayerRoot(null, _playerMovementDataSO, _playerContext);
      HierarchicalStateMachineBuilder stateMachineBuilder = new(_rootState);
      _stateMachine = stateMachineBuilder.BuildStateMachine();
    }

    private void Start()
    {
      _playerContext.jumpCount = _playerMovementDataSO.JumpMaximum;
    }

    private void Update()
    {
      if (_playerContext.mousePosition != Vector2.zero)
      {
        _mouseScreenToWorldPos = _playerContext.mainCamera.ScreenToWorldPoint(_playerContext.mousePosition);
        Vector2 player2DPosition = new(_playerContext.transform.position.x, _playerContext.transform.position.y);
        _aimDirection = (_mouseScreenToWorldPos - player2DPosition).normalized;
      }

      if (_playerContext.drawDebugGizmos)
      {
        Debug.DrawRay(_playerContext.transform.position + (Vector3.up * 0.5f), _aimDirection * _playerMovementDataSO.AbilityAimRaycastDistance, Color.darkGreen, 0.1f);
      }

      _playerContext.inputDirection = _playerMovementDataSO.PlayerMoveDirection;
      _playerMovementDataSO.UpdatePlayerAimDirection(_aimDirection);
      _playerMovementDataSO.UpdatePlayerVelocity(_playerContext.rigidbody2D.linearVelocity);
      _playerMovementDataSO.UpdateIsGrounded(IsGrounded());
      _playerMovementDataSO.UpdateIsJumping(_playerContext.isJumping);
      _playerMovementDataSO.UpdateIsAttacking(_playerContext.isAtacking);
      _playerMovementDataSO.UpdateIsTakingAim(_playerContext.isTakingAim);
      _playerMovementDataSO.UpdateIsConfirmingAim(_playerContext.isConfirmingAim);

      // Update timers
      _playerContext.jumpBufferWindow = Mathf.Clamp(_playerContext.jumpBufferWindow - Time.deltaTime, 0f, _playerMovementDataSO.JumpInputBuffer);
      _playerContext.coyoteTime = Mathf.Clamp(_playerContext.coyoteTime - Time.deltaTime, 0f, _playerMovementDataSO.CoyoteTime);

      // Reset timers 
      if (_playerMovementDataSO.IsGrounded) _playerContext.coyoteTime = _playerMovementDataSO.CoyoteTime;
      if (!_playerContext.wasGroundedLastFrame && _playerMovementDataSO.IsGrounded) _playerContext.jumpCount = _playerMovementDataSO.JumpMaximum;

      _stateMachine.Tick(Time.deltaTime);

      _playerContext.wasGroundedLastFrame = IsGrounded();

      // cache grounded state at the end of this frame since next frame we might not be grounded.
      _playerContext.inputDirectionLastFrame = _playerMovementDataSO.PlayerMoveDirection;

      _statePath = PlayerStatePathToString(_stateMachine.Root.Leaf());

      if (_statePath != _previousStatePath)
      {
        Debug.Log("Path Update: " + _statePath);
        _previousStatePath = _statePath;
      }
    }

    /* ---------------------------------------------------------------- */
    /*                               PUBLIC                             */
    /* ---------------------------------------------------------------- */

    // For used in the Player Input component.
    public void OnMove(InputAction.CallbackContext context) => _playerMovementDataSO.UpdatePlayerDirectionInput(context.ReadValue<Vector2>());

    // For use in the Player Input component.
    public void OnJump(InputAction.CallbackContext context)
    {
      if (context.started) _playerContext.isJumping = true;
      if (context.canceled) _playerContext.isJumping = false;
      if (context.performed)
      {
        _playerContext.jumpBufferWindow = _playerMovementDataSO.JumpInputBuffer;
      }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
      if (context.started) _playerContext.isAtacking = true;
      if (context.canceled) _playerContext.isAtacking = false;
    }

    public void OnTakeAim(InputAction.CallbackContext context)
    {
      if (context.started) _playerContext.isTakingAim = true;
      if (context.canceled) _playerContext.isTakingAim = false;
    }

    public void OnConfirmAim(InputAction.CallbackContext context)
    {
      if (context.started) _playerContext.isConfirmingAim = true;
      if (context.canceled) _playerContext.isConfirmingAim = false;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
      Debug.Log("Control: " + context.control.name);
      if (context.control.name == "position")
      {
        _playerContext.mousePosition = context.ReadValue<Vector2>();
      }
      else if (context.control.name == "rightStick")
      {
        Debug.Log("RIGHT STICK!");
        _aimDirection = context.ReadValue<Vector2>().normalized;
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
        _playerContext.boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance,
        _playerMovementDataSO.LayersConsideredForGroundingPlayer
      );

      RaycastHit2D middleRay = Physics2D.Raycast(
        middleRayPosition,
        Vector2.down,
        _playerContext.boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance,
        _playerMovementDataSO.LayersConsideredForGroundingPlayer
      );

      RaycastHit2D rightRay = Physics2D.Raycast(
        rightRayPosition,
        Vector2.down,
        _playerContext.boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance,
        _playerMovementDataSO.LayersConsideredForGroundingPlayer
      );

      if (_playerContext.drawDebugGizmos)
      {
        // For debugging (only appears in the Scene window, not the game window)
        Debug.DrawRay(leftRayPosition, _playerContext.boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance * Vector3.down, Color.red, 1f);
        Debug.DrawRay(middleRayPosition, _playerContext.boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance * Vector3.down, Color.red, 1f);
        Debug.DrawRay(rightRayPosition, _playerContext.boxCollider2D.bounds.extents.y * _playerMovementDataSO.GroundingRayCastDistance * Vector3.down, Color.red, 1f);
      }

      // player is grounded if any of the following rays make contact with an object with on the specified LayerMask stored in _playerMovementDataSO
      return leftRay || middleRay || rightRay;
    }
  }

  [Serializable]
  public class PlayerContext
  {

    [Header("Monobehavior Components")]

    public Transform transform;
    public Rigidbody2D rigidbody2D;
    public BoxCollider2D boxCollider2D;
    public Camera mainCamera;

    [Header("Prefabs")]
    public GameObject bramble;

    [Header("Input Flags")]

    [ReadOnly] public Vector2 inputDirection;
    [ReadOnly] public Vector2 inputDirectionLastFrame;
    [ReadOnly] public Vector2 mousePosition;
    [ReadOnly] public bool isJumping;
    [ReadOnly] public bool isAtacking;
    [ReadOnly] public bool isTakingAim;
    [ReadOnly] public bool isConfirmingAim;

    [Header("Misc.")]

    [ReadOnly] public float targetSpeed;
    [ReadOnly] public float coyoteTime;
    [ReadOnly] public float jumpBufferWindow;
    [ReadOnly] public int jumpCount;
    [ReadOnly] public bool wasGroundedLastFrame;
    [ReadOnly] public bool jumpEndEarly = false;


    [Header("Debug")]

    public bool drawDebugGizmos;
  }
}
