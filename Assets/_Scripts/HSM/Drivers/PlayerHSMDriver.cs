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
      _stateMachine.Tick(Time.deltaTime);

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

    public static string PlayerStatePathToString(State state)
    {
      return string.Join(" > ", state.PathToRoot().Reverse().Select(foo => foo.GetType().Name));
    }
  }

  [Serializable]
  public class PlayerContext
  {
    public Transform transform;
    public Rigidbody2D rigidbody2D;
    public BoxCollider2D boxCollider2D;
    [ReadOnly] public Vector2 inputDirection;
    [ReadOnly] public Vector2 inputDirectionLastFrame;
    [ReadOnly] public bool isJumping;
    [ReadOnly] public float targetSpeed;
    [ReadOnly] public float coyoteTime;
    [ReadOnly] public float jumpBufferWindow;
    [ReadOnly] public int jumpCount;
    [ReadOnly] public bool wasGroundedLastFrame;
    [ReadOnly] public bool jumpEndEarly = false;
    public bool drawDebugGizmos;
  }
}
