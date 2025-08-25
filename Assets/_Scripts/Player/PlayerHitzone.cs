using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerHitzone : MonoBehaviour
{
  [SerializeField] private Vector2 _hitBoxOffset = new(0.5f, 0.5f);
  [SerializeField] private BoxCollider2D _boxCollider2D;

  [SerializeField, Tag] private string _tagToCollideWith = "Enemy";

  [Required("Must provide a HSMScratchpadSO asset.")]
  [SerializeField] private HSMScratchpadSO _scratchpad;
  [SerializeField, Expandable] private PlayerAbilityDataSO _playerAbilityData;
  [SerializeField, Expandable] private PlayerEventDataSO _playerEventData;
  [SerializeField, Expandable] private PlayerAttributesDataSO _playerAttributesData;

  private void Awake()
  {
    if (_scratchpad == null)
    {
      Debug.LogError(name + " does not have a HSMScratchpadSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _playerAbilityData = _scratchpad.GetScratchpadData<PlayerAbilityDataSO>();
    if (_playerAbilityData == null)
    {
      Debug.LogError(name + " does not have a PlayerAbilityDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }


    _playerEventData = _scratchpad.GetScratchpadData<PlayerEventDataSO>();
    if (_playerEventData == null)
    {
      Debug.LogError(name + " does not have a PlayerEventDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _playerAttributesData = _scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
    if (_playerAttributesData == null)
    {
      Debug.LogError(name + " does not have a PlayerAttributesDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_boxCollider2D == null) _boxCollider2D = GetComponent<BoxCollider2D>();
  }

  private void Start()
  {
    transform.localPosition = new(
      _hitBoxOffset.x,
      _hitBoxOffset.y,
      transform.localPosition.z
    );
  }

  private void Update()
  {
    FlipHitboxOnPlayerAttributesData();
  }

  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (collider.gameObject.CompareTag(_tagToCollideWith))
    {
      Debug.Log("Collider from: " + collider.gameObject.name);
      if (_playerAbilityData.CurrentlyEquippedArm != null && _playerAbilityData.CurrentlyEquippedArm.CombatAbility != null)
      {
        _playerEventData.DoDamageToEntity.RaiseEvent(
          collider.gameObject.GetInstanceID(),
          _playerAbilityData.CurrentlyEquippedArm.CombatAbility.Damage
        );
      }
    }
  }

  private void FlipHitboxOnPlayerAttributesData()
  {
    if (_playerAttributesData.IsAttacking) return;

    if (_playerAttributesData.PlayerMoveDirection.x != 0)
    {
      transform.localPosition = new(
        _playerAttributesData.PlayerMoveDirection.x < 0 ? -_hitBoxOffset.x : _hitBoxOffset.x,
        transform.localPosition.y,
        transform.localPosition.z
      );
    }
  }
}
