using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementDataSO", menuName = "Scriptable Objects/PlayerMovementData")]
public class PlayerMovementDataSO : ScriptableObject
{
  /* ---------------------------------------------------------------- */
  /*                          Configurable Stats                      */
  /* ---------------------------------------------------------------- */
  [Header("Movement Data")]
  [SerializeField, Tooltip("Determines how fast the player will move.")]
  private float _movementSpeed;

  [SerializeField, Range(9.8f, 50f), Tooltip("Limits the velocity at which the player can fall. Is measured in units/second. In Unity 1 unit = 1 meter.")]
  private float _maxFallingSpeed;

  [SerializeField, Range(1f, 10f), Tooltip("Affects influcence of gravity on the player as they're falling.")]
  private float _gravityMultiplierWhenFalling;

  [Space(5)]
  [Header("Jump Data")]
  [SerializeField, Tooltip("Maximum number of jumps the player can perform before touching the ground again.")]
  private int _maxNumberOfJumps;
  [SerializeField, Range(1f, 10f), Tooltip("Height of the jump in units.")]
  private float _jumpHeight;

  [SerializeField, Range(0.25f, 1.5f), Tooltip("How long it should take to reach the apex of the jump.")]
  private float _timeToApex;

  [SerializeField, Range(0f, 1f), Tooltip("")]
  private float _jumpInputBuffer;

  [Space(5)]
  [Header("Grounding Data")]
  [SerializeField, Tooltip("Layer Mask used for grounding the player.")]
  private LayerMask _groundLayerMask;

  [SerializeField, Range(0.125f, 1f), Tooltip("")]
  private float _groundingRaycastDistance;

  [Space(5)]
  [Header("Mechanical Data")]
  [SerializeField, Range(0f, 1.5f), Tooltip("")]
  private float _coyoteTime;

  [Space(5)]
  [Header("Derived Data")]
  [SerializeField, ReadOnly, Tooltip("Equation: Abs(2 * jumpHeight / timeToApex^2) * timeToApex")]
  private float _jumpingPower;

  [SerializeField, ReadOnly, Tooltip("Equation: Abs(2 * jumpHeight / timeToApex^2) / Physics2D.gravity.y")]
  private float _gravityScale;

  /* ---------------------------------------------------------------- */
  /*                           Lambda Getters                         */
  /* ---------------------------------------------------------------- */
  public float MovementSpeed => _movementSpeed;
  public int MaxNumberOfJumps => _maxNumberOfJumps;
  public float JumpHeight => _jumpHeight;
  public float TimeToApex => _timeToApex;
  public float MaxFallingSpeed => _maxFallingSpeed;
  public float GravityMultiplierWhenFalling => _gravityMultiplierWhenFalling;
  public LayerMask GroundLayerMask => _groundLayerMask;
  public float CoyoteTime => _coyoteTime;
  public float JumpInputBuffer => _jumpInputBuffer;

  /* ---------------------------------------------------------------- */
  /*                       Runtime Player Stats                       */
  /* ---------------------------------------------------------------- */
  [Space(5)]
  [Header("Runtime Data")]

  [SerializeField, ReadOnly] private Vector2 _playerVelocity;
  public Vector2 PlayerVelocity { get { return _playerVelocity; } private set { PlayerVelocity = _playerVelocity; } }

  [SerializeField, ReadOnly] private Vector2 _playerDirectionInput;
  public Vector2 PlayerDirectionInput { get { return _playerDirectionInput; } private set { PlayerDirectionInput = _playerDirectionInput; } }

  [SerializeField, ReadOnly] private bool _isGrounded;
  public bool IsGrounded { get { return _isGrounded; } private set { IsGrounded = _isGrounded; } }

  public float JumpingPower => _jumpingPower;
  public float GravityScale => _gravityScale;


  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    ResetPlayerData();
  }

  private void Reset()
  {
    ResetPlayerData();
  }

  private void OnValidate()
  {
    // gravity strength = Abs(2 * jumpHeight / timeToApex^2)
    float gravityStrength = 2f * _jumpHeight / Mathf.Pow(_timeToApex, 2);

    _jumpingPower = gravityStrength * _timeToApex;
    _gravityScale = Mathf.Abs(gravityStrength / Physics2D.gravity.y);
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */
  public void UpdatePlayerVelocity(Vector2 state) => _playerVelocity = state;
  public void UpdatePlayerDirectionInput(Vector2 state) => _playerDirectionInput = state;
  public void UpdateIsGrounded(bool state) => _isGrounded = state;

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private void ResetPlayerData()
  {
    _playerVelocity = _playerDirectionInput = Vector2.zero;
    _isGrounded = false;
  }

}
