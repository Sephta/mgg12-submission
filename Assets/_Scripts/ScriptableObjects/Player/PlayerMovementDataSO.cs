using System;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Movement Data SO", menuName = "Scriptable Objects/Player/Player Movement Data")]
public class PlayerMovementDataSO : ScriptableObject
{
  /* ---------------------------------------------------------------- */
  /*                           Movement Data                          */
  /* ---------------------------------------------------------------- */

  [field: Header("Movement Data")]

  [field: SerializeField, Range(0f, 10f), Tooltip("Determines how fast the player will move.")]
  public float RunVelocityMaximum { get; private set; }

  [field: SerializeField, Range(0f, 10f), Tooltip("Determines how fast the player reach max speed.")]
  public float Acceleration { get; private set; }

  [field: SerializeField, Range(0f, 3f), Tooltip("Amplifies the rate of acceleration while airborne.")]
  public float AccelerationAirMultiplier { get; private set; }

  [field: SerializeField, Range(0f, 10f), Tooltip("Determines how fast the player slows down.")]
  public float Deceleration { get; private set; }

  [field: SerializeField, Range(0f, 3f), Tooltip("Amplifies the rate of deceleration while airborne.")]
  public float DecelerationAirMultiplier { get; private set; }

  [field: SerializeField, Range(0f, 50f), Tooltip("Limits the velocity at which the player can move. Is measured in units/second. In Unity 1 unit = 1 meter.")]
  public float VelocityHorizontalClamp { get; private set; }

  [field: SerializeField, Range(9.8f, 50f), Tooltip("Limits the velocity at which the player can fall. Is measured in units/second. In Unity 1 unit = 1 meter.")]
  public float VelocityVerticalClamp { get; private set; }

  /* ---------------------------------------------------------------- */
  /*                         Movement Settings                        */
  /* ---------------------------------------------------------------- */

  [field: Space(5)]
  [field: Header("Movement Settings")]

  [field: SerializeField, Tooltip("Determines whether turning around should be an intstantaneous change in velocity.")]
  public bool DoClampHorizontalVelocity { get; private set; }

  [field: SerializeField, Tooltip("Determines whether turning around should be an intstantaneous change in velocity.")]
  public bool DoClampVerticalVelocity { get; private set; }

  /* ---------------------------------------------------------------- */
  /*                          Jumping Data                            */
  /* ---------------------------------------------------------------- */

  [field: Space(5)]
  [field: Header("Jump Data")]

  [field: SerializeField, Tooltip("Maximum number of jumps the player can perform before touching the ground again.")]
  public int JumpMaximum { get; private set; }

  [field: SerializeField, Range(1f, 10f), Tooltip("Max height of the jump.")]
  public float JumpHeight { get; private set; }

  [field: SerializeField, Range(0.25f, 1.5f), Tooltip("How long it should take to reach the apex of the jump.")]
  public float JumpTimeToApex { get; private set; }

  [field: SerializeField, Range(0f, 1f), Tooltip("When the player's linear velocity on the y axis is within this threshold the gravity scale is changed based on Jump Hang Time Gravity Multiplier.")]
  public float JumpHangTimeThreshold { get; private set; }

  [field: SerializeField, Range(0f, 1f), Tooltip("")]
  public float JumpInputBuffer { get; private set; }

  /* ---------------------------------------------------------------- */
  /*                          Gravity Data                            */
  /* ---------------------------------------------------------------- */

  [field: Space(5)]
  [field: Header("Gravity Data")]

  [field: SerializeField, Range(1f, 10f), Tooltip("Affects influcence of gravity on the player as they're falling.")]
  public float FallingGravityMultiplier { get; private set; }

  [field: SerializeField, Range(0f, 1f), Tooltip("During the hang time threshold this will be applied to the current gravity scale.")]
  public float JumpHangTimeGravityMultiplier { get; private set; }

  [field: SerializeField, Range(1f, 2f), Tooltip("If the player leaves the jump early (before reaching max height) this is the multiplier that will be used instead of the falling multiplier.")]
  public float ShortJumpGravityMultiplier { get; private set; }

  /* ---------------------------------------------------------------- */
  /*                          Grounding Data                          */
  /* ---------------------------------------------------------------- */

  [field: Space(5)]
  [field: Header("Grounding Data")]

  [field: SerializeField, Tooltip("The layer to use when checking if the player is grounded.")]
  public LayerMask LayersConsideredForGroundingPlayer { get; private set; }

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

  [field: SerializeField, Range(0f, 100f), Tooltip("Controls how far the raycast goes when the player is aiming in a direction to grow bramble or perform some other ability.")]
  public float AbilityAimRaycastDistance { get; private set; }

  /* ---------------------------------------------------------------- */
  /*                          Derived Data                            */
  /* ---------------------------------------------------------------- */

  [field: Space(5)]
  [field: Header("Derived Data")]

  [field: SerializeField, ReadOnly, Tooltip("The computed initial velocity needed to reach Jump Height at Jump Time to Apex.")]
  public float JumpingPower { get; private set; }

  [field: SerializeField, ReadOnly, Tooltip("The computed scale of gravity needed to achieve Jump Height at Jump Time to Apex")]
  public float GravityScale { get; private set; }

  [field: SerializeField, ReadOnly, Tooltip("Computed acceleration value based on acceleration configured above.")]
  public float RunAccelerationAmount { get; private set; }

  [field: SerializeField, ReadOnly, Tooltip("Computed deceleration value based on acceleration configured above.")]
  public float RunDecelerationAmount { get; private set; }

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void OnValidate()
  {
    float gravityStrength = -(2 * JumpHeight) / (JumpTimeToApex * JumpTimeToApex);

    // Old jump power calculation
    // JumpingPower = 2 * JumpHeight / JumpTimeToApex;

    JumpingPower = Mathf.Abs(gravityStrength) * JumpTimeToApex;
    GravityScale = gravityStrength / Physics2D.gravity.y;

    float accelerationBase = 100;
    float decelerationBase = 100;

    RunAccelerationAmount = accelerationBase * Acceleration / RunVelocityMaximum;
    RunDecelerationAmount = decelerationBase * Deceleration / RunVelocityMaximum;
  }
}
