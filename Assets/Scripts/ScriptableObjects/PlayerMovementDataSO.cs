using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementDataSO", menuName = "Scriptable Objects/PlayerMovementData")]
public class PlayerMovementDataSO : ScriptableObject
{
  /* ---------------------------------------------------------------- */
  /*                          Configurable Stats                      */
  /* ---------------------------------------------------------------- */
  [Header("Movement Data")]
  [SerializeField, Range(0f, 10f), Tooltip("Determines how fast the player will move.")]
  private float _maxRunVelocity;

  [SerializeField, Range(0f, 10f), Tooltip("Determines how fast the player reach max speed.")]
  private float _acceleration;

  [SerializeField, Range(0f, 10f), Tooltip("Determines how fast the player slows down while on the gournd.")]
  private float _decelerationGround;

  [SerializeField, Range(0f, 10f), Tooltip("Determines how fast the player slows down while in the air.")]
  private float _decelerationAir;

  [SerializeField, Range(0f, 50f), Tooltip("Limits the velocity at which the player can move. Is measured in units/second. In Unity 1 unit = 1 meter.")]
  private float _velocityHorizontalClamp;

  [SerializeField, Range(9.8f, 50f), Tooltip("Limits the velocity at which the player can fall. Is measured in units/second. In Unity 1 unit = 1 meter.")]
  private float _velocityVerticalClamp;

  [SerializeField, Range(1f, 10f), Tooltip("Affects influcence of gravity on the player as they're falling.")]
  private float _fallingGravityMultiplier;

  [Space(5)]
  [Header("Jump Data")]
  [SerializeField, Tooltip("Maximum number of jumps the player can perform before touching the ground again.")]
  private int _maxNumberOfJumps;
  [SerializeField, Range(1f, 10f), Tooltip("Height of the jump in units.")]
  private float _jumpHeight;

  [SerializeField, Range(0.25f, 1.5f), Tooltip("How long it should take to reach the apex of the jump.")]
  private float _timeToApex;

  [SerializeField, Range(0f, 0.5f), Tooltip("When the player's linear velocity on the y axis is within this threshold the gravity scale is changed based on Jump Hang Time Gravity Multiplier.")]
  private float _jumpHangTimeThreshold;

  [SerializeField, Range(0f, 1f), Tooltip("The influence of hang time on gravity.")]
  private float _jumpHangTimeGravityMultiplier;

  [SerializeField, Range(0f, 1f), Tooltip("")]
  private float _jumpInputBuffer;

  [Space(5)]
  [Header("Grounding Data")]
  [SerializeField, Tooltip("The layer to use when checking if the player is grounded.")]
  private LayerMask _groundLayerMask;
  [SerializeField, Range(0f, 1f), Tooltip("Controls how far the raycast goes to check if player is grounded.")]
  private float _groundingRaycastDistance;
  [SerializeField, Tooltip("Useful for checking if the player is touching a surface in the specified layer.")]
  private ContactFilter2D _surfaceContactFilter;

  [Space(5)]
  [Header("Mechanical Data")]
  [SerializeField, Range(0f, 1f), Tooltip("Window of time the player has after leaving the ground to jump again.")]
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
  public float MaxRunVelocity => _maxRunVelocity;
  public float Acceleration => _acceleration;
  public float DecelerationGround => _decelerationGround;
  public float DecelerationAir => _decelerationAir;
  public float VelocityHorizontalClamp => _velocityHorizontalClamp;
  public float VelocityVerticalClamp => _velocityVerticalClamp;
  public float FallingGravityMultiplier => _fallingGravityMultiplier;
  public int MaxNumberOfJumps => _maxNumberOfJumps;
  public float JumpHeight => _jumpHeight;
  public float TimeToApex => _timeToApex;
  public float JumpHangTimeThreshold => _jumpHangTimeThreshold;
  public float JumpHangTimeGravityMultiplier => _jumpHangTimeGravityMultiplier;
  public float JumpInputBuffer => _jumpInputBuffer;
  public LayerMask GroundLayerMask => _groundLayerMask;
  public float GroundingRayCastDistance => _groundingRaycastDistance;
  public ContactFilter2D SurfaceContactFilter => _surfaceContactFilter;
  public float CoyoteTime => _coyoteTime;

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
    // float gravityStrength = 2f * _jumpHeight / Mathf.Pow(_timeToApex, 2);

    // _jumpingPower = gravityStrength * _timeToApex;
    // _gravityScale = Mathf.Abs(gravityStrength / Physics2D.gravity.y);

    float gravityStrength = -2 * _jumpHeight / Mathf.Pow(_timeToApex, 2);

    _jumpingPower = 2 * _jumpHeight / _timeToApex;
    _gravityScale = gravityStrength / Physics2D.gravity.y;
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
