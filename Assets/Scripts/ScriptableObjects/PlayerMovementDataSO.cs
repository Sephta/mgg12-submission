using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementDataSO", menuName = "Scriptable Objects/PlayerMovementData")]
public class PlayerMovementDataSO : ScriptableObject
{
  /* ---------------------------------------------------------------- */
  /*                          Configurable Stats                      */
  /* ---------------------------------------------------------------- */
  [SerializeField, Tooltip("Determines how fast the player will move.")]
  private float _movementSpeed;

  [SerializeField, Tooltip("The strength of the jump.")]
  private float _jumpingPower;

  [SerializeField, Tooltip("The player's velocity when falling cannot exceed this value. Is measured in units/second. In Unity 1 unit = 1 meter.")]
  private float _maxFallingSpeed;

  [SerializeField, Range(1f, 10f), Tooltip("Affects influcence of gravity on the player.")]
  private float _gravityMultiplier;

  [SerializeField, Range(1f, 10f), Tooltip("Affects influcence of gravity on the player, but only when falling.")]
  private float _gravityMultiplierWhileFalling;

  [SerializeField, Tooltip("Layer Mask used for grounding the player.")]
  private LayerMask _groundLayerMask;

  /* ---------------------------------------------------------------- */
  /*                           Lambda Getters                         */
  /* ---------------------------------------------------------------- */
  public float MovementSpeed => _movementSpeed;
  public float JumpingPower => _jumpingPower;
  public float MaxFallingSpeed => _maxFallingSpeed;
  public float GravityMultipler => _gravityMultiplier;
  public float GravityMultiplierWhileFalling => _gravityMultiplierWhileFalling;
  public LayerMask GroundLayerMask => _groundLayerMask;


  /* ---------------------------------------------------------------- */
  /*                       Runtime Player Stats                       */
  /* ---------------------------------------------------------------- */
  [Header("Player Data")]

  [SerializeField, ReadOnly] private Vector2 _playerVelocity;
  public Vector2 PlayerVelocity { get { return _playerVelocity; } private set { PlayerVelocity = _playerVelocity; } }

  [SerializeField, ReadOnly] private bool _isGrounded;
  public bool IsGrounded { get { return _isGrounded; } private set { IsGrounded = _isGrounded; } }


  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  void Awake()
  {
    ResetPlayerData();
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */
  public void UpdatePlayerVelocity(Vector2 state) => _playerVelocity = state;

  public void UpdateIsGrounded(bool state) => _isGrounded = state;

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private void ResetPlayerData()
  {
    _playerVelocity = new Vector2();
    _isGrounded = false;
  }

}
