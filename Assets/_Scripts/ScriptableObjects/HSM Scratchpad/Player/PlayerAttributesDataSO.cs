using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Attributes SO", menuName = "Scriptable Objects/Player/Player Attributes Data")]
public class PlayerAttributesDataSO : ScratchpadDataSO
{
  /* ---------------------------------------------------------------- */
  /*                       Runtime Player Stats                       */
  /* ---------------------------------------------------------------- */

  [field: Space(10f)]
  [field: Header("Runtime Data")]

  [field: SerializeField, ReadOnly]
  public Vector2 PlayerVelocity { get; private set; }

  [field: Space(5f)]
  [field: Header("Input Data")]

  [field: SerializeField, ReadOnly]
  public Vector2 PlayerMoveDirection { get; private set; }

  [field: SerializeField, ReadOnly]
  public Vector2 PlayerMousePosition { get; private set; }

  [field: SerializeField, ReadOnly]
  public Vector2 PlayerAimDirection { get; private set; }

  [field: SerializeField, ReadOnly]
  public bool IsGrounded { get; private set; }

  [field: SerializeField, ReadOnly]
  public bool IsJumping { get; private set; }

  [field: SerializeField, ReadOnly]
  public bool IsAttacking { get; private set; }

  [field: SerializeField, ReadOnly]
  public bool IsTakingAim { get; private set; }

  [field: SerializeField, ReadOnly]
  public bool IsConfirmingAim { get; private set; }

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    ResetData();
  }

  private void Reset()
  {
    ResetData();
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  // Public Setters for Runtime Data
  public void UpdatePlayerVelocity(Vector2 state) => PlayerVelocity = state;
  public void UpdatePlayerDirectionInput(Vector2 state) => PlayerMoveDirection = state;
  public void UpdatePlayerMousePosition(Vector2 state) => PlayerMousePosition = state;
  public void UpdatePlayerAimDirection(Vector2 state) => PlayerAimDirection = state;
  public void UpdateIsGrounded(bool state) => IsGrounded = state;
  public void UpdateIsJumping(bool state) => IsJumping = state;
  public void UpdateIsAttacking(bool state) => IsAttacking = state;
  public void UpdateIsTakingAim(bool state) => IsTakingAim = state;
  public void UpdateIsConfirmingAim(bool state) => IsConfirmingAim = state;

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  [Button("Reset Data")]
  private void ResetData()
  {
    PlayerVelocity = Vector2.zero;
    PlayerMoveDirection = Vector2.zero;
    PlayerMousePosition = Vector2.zero;
    PlayerAimDirection = Vector2.zero;
    IsGrounded = false;
    IsJumping = false;
    IsAttacking = false;
    IsTakingAim = false;
    IsConfirmingAim = false;
  }
}
