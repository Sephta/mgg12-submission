using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FlipSpriteBasedOnPlayerState : MonoBehaviour
{
  [Header("Component References"), Space(10f)]

  [SerializeField] private SpriteRenderer _spriteRenderer;

  [Header("Player Data"), Space(10f)]

  [Required("Must provide a HSMScratchpadSO asset.")]
  [SerializeField] private HSMScratchpadSO _scratchpad;

  [Space(15f)]
  [InfoBox("If the fields bellow are left empty they will be populated from the Scratchpad data at runtime on Awake().")]
  [Space(5f)]

  [SerializeField, Expandable] private PlayerMovementDataSO _playerMovementData;
  [SerializeField, Expandable] private PlayerAttributesDataSO _playerAttributesData;
  [SerializeField, Expandable] private PlayerAbilityDataSO _playerAbilityData;


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

    if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
    if (_spriteRenderer == null)
    {
      Debug.LogError(name + " does not have a SpriteRenderer referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }
  }

  // private void Start() {}
  // private void OnEnable() {}
  // private void OnDisable() {}

  private void Update()
  {
    FlipSpriteBasedOnPlayerAttributesData();
  }

  // private void FixedUpdate() {}

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private void FlipSpriteBasedOnPlayerAttributesData()
  {
    if (_playerAttributesData.IsLatchedOntoWall)
    {
      Vector2 wallDirection = GetWallDirection();

      if (wallDirection != Vector2.zero)
      {
        _spriteRenderer.flipX = wallDirection.x > 0;
        return;
      }
    }

    if (_playerAttributesData.IsAttacking || _playerAttributesData.IsNeedling) return;

    if (_playerAttributesData.IsTakingAim
      && _playerAbilityData.CurrentlyEquippedArmType == NeroArmType.Neutral
      && _playerAttributesData.IsGrounded)
    {
      if (_playerAttributesData.PlayerAimDirection.x != 0)
        _spriteRenderer.flipX = _playerAttributesData.PlayerAimDirection.x < 0;
    }
    else if (_playerAttributesData.PlayerMoveDirection.x != 0)
    {
      _spriteRenderer.flipX = _playerAttributesData.PlayerMoveDirection.x < 0;
    }
  }

  private Vector2 GetWallDirection()
  {
    Vector2 wallDirection = Vector2.zero;

    RaycastHit2D rayCastLeft = Physics2D.Raycast(
      transform.position + (Vector3.up * 0.5f),
      Vector2.left,
      1f,
      _playerMovementData.LayersConsideredForPlayerTouchingWall
    );

    RaycastHit2D rayCastRight = Physics2D.Raycast(
      transform.position + (Vector3.up * 0.5f),
      Vector2.right,
      1f,
      _playerMovementData.LayersConsideredForPlayerTouchingWall
    );

    if (rayCastLeft) wallDirection = Vector2.left;
    if (rayCastRight) wallDirection = Vector2.right;

    return wallDirection;
  }
}
