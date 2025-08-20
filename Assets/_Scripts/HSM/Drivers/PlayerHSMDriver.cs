using System;
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

    private HierarchicalStateMachine _stateMachine;
    private State _rootState;

    /* ---------------------------------------------------------------- */
    /*                           Unity Functions                        */
    /* ---------------------------------------------------------------- */

    private void Awake()
    {
      if (_playerMovementDataSO == null)
      {
        Debug.LogError(name + " does not have defined " + _playerMovementDataSO.GetType().Name);
        gameObject.SetActive(false);
      }

      if (_playerContext.transform == null) _playerContext.transform = transform;
      if (_playerContext.rigidbody2D == null) _playerContext.rigidbody2D = GetComponent<Rigidbody2D>();
      if (_playerContext.boxCollider2D == null) _playerContext.boxCollider2D = GetComponent<BoxCollider2D>();

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
      _playerContext.inputDirection = _playerMovementDataSO.PlayerDirectionInput;
      _playerMovementDataSO.UpdateIsGrounded(IsGrounded());
      _playerMovementDataSO.UpdatePlayerVelocity(_playerContext.rigidbody2D.linearVelocity);

      // Update timers
      _playerContext.jumpBufferWindow = Mathf.Clamp(_playerContext.jumpBufferWindow - Time.deltaTime, 0f, _playerMovementDataSO.JumpInputBuffer);
      _playerContext.coyoteTime = Mathf.Clamp(_playerContext.coyoteTime - Time.deltaTime, 0f, _playerMovementDataSO.CoyoteTime);

      // Reset timers 
      if (_playerMovementDataSO.IsGrounded) _playerContext.coyoteTime = _playerMovementDataSO.CoyoteTime;
      if (!_playerContext.wasGroundedLastFrame && _playerMovementDataSO.IsGrounded) _playerContext.jumpCount = _playerMovementDataSO.JumpMaximum;

      _stateMachine.Tick(Time.deltaTime);

      _playerContext.wasGroundedLastFrame = IsGrounded();

      // cache grounded state at the end of this frame since next frame we might not be grounded.
      _playerContext.inputDirectionLastFrame = _playerMovementDataSO.PlayerDirectionInput;

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

    [Header("Input Flags")]

    [ReadOnly] public Vector2 inputDirection;
    [ReadOnly] public Vector2 inputDirectionLastFrame;
    [ReadOnly] public bool isJumping;
    [ReadOnly] public bool isAtacking;
    [ReadOnly] public bool isTakingAim;

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
