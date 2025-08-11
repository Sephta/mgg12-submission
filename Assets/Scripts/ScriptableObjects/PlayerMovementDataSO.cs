using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementDataSO", menuName = "Scriptable Objects/PlayerMovementData")]
public class PlayerMovementDataSO : ScriptableObject
{
  /* ---------------------------------------------------------------- */
  /*                           Movement Data                          */
  /* ---------------------------------------------------------------- */
  [field: Header("Movement Data")]

  [field: SerializeField, Range(0f, 10f), Tooltip("Determines how fast the player will move.")]
  public float MaxRunVelocity { get; private set; }

  [field: SerializeField, Range(0f, 10f), Tooltip("Determines how fast the player reach max speed while grounded.")]
  public float AccelerationGround { get; private set; }

  [field: SerializeField, Range(0f, 20f), Tooltip("Determines how fast the player reach max speed while airborne.")]
  public float AccelerationAir { get; private set; }

  [field: SerializeField, Range(0f, 10f), Tooltip("Determines how fast the player slows down while grounded.")]
  public float DecelerationGround { get; private set; }

  [field: SerializeField, Range(0f, 10f), Tooltip("Determines how fast the player slows down while airborne.")]
  public float DecelerationAir { get; private set; }

  [field: SerializeField, Tooltip("Determines whether turning around should be an intstantaneous change in velocity.")]
  public bool IntstantaneousTurns { get; private set; }

  [field: SerializeField, Range(0f, 50f), Tooltip("Limits the velocity at which the player can move. Is measured in units/second. In Unity 1 unit = 1 meter.")]
  public float VelocityHorizontalClamp { get; private set; }

  [field: SerializeField, Range(9.8f, 50f), Tooltip("Limits the velocity at which the player can fall. Is measured in units/second. In Unity 1 unit = 1 meter.")]
  public float VelocityVerticalClamp { get; private set; }

  [field: SerializeField, Range(1f, 10f), Tooltip("Affects influcence of gravity on the player as they're falling.")]
  public float FallingGravityMultiplier { get; private set; }

  /* ---------------------------------------------------------------- */
  /*                          Jumping Data                            */
  /* ---------------------------------------------------------------- */

  [field: Space(5)]
  [field: Header("Jump Data")]

  [field: SerializeField, Tooltip("Maximum number of jumps the player can perform before touching the ground again.")]
  public int MaxNumberOfJumps { get; private set; }

  [field: SerializeField, Range(1f, 10f), Tooltip("Max height of the jump.")]
  public float JumpHeight { get; private set; }

  [field: SerializeField, Range(0.25f, 1.5f), Tooltip("How long it should take to reach the apex of the jump.")]
  public float JumpTimeToApex { get; private set; }

  [field: SerializeField, Range(0f, 0.5f), Tooltip("When the player's linear velocity on the y axis is within this threshold the gravity scale is changed based on Jump Hang Time Gravity Multiplier.")]
  public float JumpHangTimeThreshold { get; private set; }

  [field: SerializeField, Range(0f, 1f), Tooltip("The influence of hang time on gravity.")]
  public float JumpHangTimeGravityMultiplier { get; private set; }

  [field: SerializeField, Range(1f, 2f), Tooltip("Gravity mulitplier when performing a shorter jump.")]
  public float ShortJumpGravityMultiplier { get; private set; }

  [field: SerializeField, Range(0f, 1f), Tooltip("")]
  public float JumpInputBuffer { get; private set; }

  /* ---------------------------------------------------------------- */
  /*                          Grounding Data                          */
  /* ---------------------------------------------------------------- */

  [field: Space(5)]
  [field: Header("Grounding Data")]

  [field: SerializeField, Tooltip("The layer to use when checking if the player is grounded.")]
  public LayerMask GroundLayerMask { get; private set; }

  [field: SerializeField, Range(0f, 1f), Tooltip("Controls how far the raycast goes to check if player is grounded.")]
  public float GroundingRayCastDistance { get; private set; }

  [Tooltip("Useful for checking if the player is touching a surface in the specified layer.")]
  public ContactFilter2D SurfaceContactFilter;


  /* ---------------------------------------------------------------- */
  /*                          Mechanical Data                         */
  /* ---------------------------------------------------------------- */

  [field: Space(5)]
  [field: Header("Mechanical Data")]

  [field: SerializeField, Range(0f, 1f), Tooltip("Window of time the player has after leaving the ground to jump again.")]
  public float CoyoteTime { get; private set; }


  /* ---------------------------------------------------------------- */
  /*                          Derived Data                            */
  /* ---------------------------------------------------------------- */

  [field: Space(5)]
  [field: Header("Derived Data")]

  [field: SerializeField, ReadOnly, Tooltip("Equation: Abs(2 * jumpHeight / timeToApex^2) * timeToApex")]
  public float JumpingPower { get; private set; }

  [field: SerializeField, ReadOnly, Tooltip("")]
  public float GravityScale { get; private set; }

  /* ---------------------------------------------------------------- */
  /*                       Runtime Player Stats                       */
  /* ---------------------------------------------------------------- */

  [field: Space(5)]
  [field: Header("Runtime Data")]

  [field: SerializeField, ReadOnly]
  public Vector2 PlayerVelocity { get; private set; }

  [field: SerializeField, ReadOnly]
  public Vector2 PlayerDirectionInput { get; private set; }

  [field: SerializeField, ReadOnly]
  public bool IsGrounded { get; private set; }

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    ResetRuntimeData();
  }

  private void Reset()
  {
    ResetRuntimeData();
  }

  private void OnValidate()
  {
    float gravityStrength = -2 * JumpHeight / Mathf.Pow(JumpTimeToApex, 2);

    JumpingPower = 2 * JumpHeight / JumpTimeToApex;
    GravityScale = gravityStrength / Physics2D.gravity.y;
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  // Public Setters for Runtime Data
  public void UpdatePlayerVelocity(Vector2 state) => PlayerVelocity = state;
  public void UpdatePlayerDirectionInput(Vector2 state) => PlayerDirectionInput = state;
  public void UpdateIsGrounded(bool state) => IsGrounded = state;

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  [Button("Reset Runtime Data")]
  private void ResetRuntimeData()
  {
    PlayerVelocity = Vector2.zero;
    PlayerDirectionInput = Vector2.zero;
    IsGrounded = false;
  }

}
