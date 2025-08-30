using System;
using LDtkUnity;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class AbilityPickup : MonoBehaviour
{
  [Header("Component Fields")]

  [SerializeField] private CircleCollider2D _circleCollider2D;
  [SerializeField, ReadOnly] private LDtkComponentEntity _ldtkComponentEntity;

  [Header("Configuration")]

  [SerializeField, Range(0f, 3f)] private float _colliderRadius = 0.5f;
  [SerializeField, Tag] private string _playerTag = "Player";
  [SerializeField] private bool _ignoreLDtk = false;
  [ShowIf("_ignoreLDtk"), SerializeField] private NeroArmType _armTypeToGivePlayer = NeroArmType.Needle;

  [Header("Data Fields")]

  [Space(10f)]

  [SerializeField, Expandable] private NeroArmDataSO _needleAbilityData;
  [SerializeField, Expandable] private NeroArmDataSO _clawAbilityData;
  [SerializeField, Expandable] private NeroArmDataSO _cannonAbilityData;

  [Space(10f)]

  [SerializeField, Expandable] private PlayerAbilityDataSO _playerAbilityData;

  private static string Needle { get { return "Needle_Pickup"; } }
  private static string Claw { get { return "Claw_Pickup"; } }
  private static string Gun { get { return "Cannon_Pickup"; } }

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_circleCollider2D == null) _circleCollider2D = GetComponent<CircleCollider2D>();
    if (_ldtkComponentEntity == null) _ldtkComponentEntity = GetComponent<LDtkComponentEntity>();

    if (_needleAbilityData == null)
    {
      Debug.LogError(name + " does not have a NeroArmDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_clawAbilityData == null)
    {
      Debug.LogError(name + " does not have a NeroArmDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_cannonAbilityData == null)
    {
      Debug.LogError(name + " does not have a NeroArmDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_playerAbilityData == null)
    {
      Debug.LogError(name + " does not have a PlayerAbilityDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }
  }

  // private void Start() {}

  private void OnEnable()
  {
    _circleCollider2D.radius = _colliderRadius;
    _circleCollider2D.isTrigger = true;
  }

  // private void OnDisable() {}

  // private void Update() {}
  // private void FixedUpdate() {}

  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (collider.CompareTag(_playerTag))
    {
      if (_ignoreLDtk)
      {
        bool result = _playerAbilityData.AddAbility(
          _armTypeToGivePlayer switch
          {
            NeroArmType.Needle => _needleAbilityData,
            NeroArmType.Claw => _clawAbilityData,
            NeroArmType.Gun => _cannonAbilityData,
            _ => _needleAbilityData,
          }
        );

        if (result)
        {
          AbilityPickupManager.Instance.StartAbilityPickupCooldown(gameObject);
        }
      }
      else
      {
        if (_ldtkComponentEntity != null)
        {
          NeroArmDataSO abilityToAdd = _ldtkComponentEntity.Identifier switch
          {
            "Needle_Pickup" => _needleAbilityData,
            "Claw_Pickup" => _clawAbilityData,
            "Cannon_Pickup" => _cannonAbilityData,
            _ => null,
          };

          if (abilityToAdd != null)
          {
            if (_playerAbilityData.AddAbility(abilityToAdd))
            {
              AbilityPickupManager.Instance.StartAbilityPickupCooldown(gameObject);
            }
          }
        }
      }
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
}
