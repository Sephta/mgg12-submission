using System;
using LDtkUnity;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class AbilityPickup : MonoBehaviour
{
  [Header("Component Fields")]

  [SerializeField] private SpriteRenderer _spriteRenderer;
  [SerializeField] private CircleCollider2D _circleCollider2D;
  [SerializeField, ReadOnly] private LDtkComponentEntity _ldtkComponentEntity;
  [SerializeField] private Sprite _brambleIcon;
  [SerializeField] private Sprite _needleIcon;
  [SerializeField] private Sprite _clawIcon;
  [SerializeField] private Sprite _cannonIcon;

  [Header("Configuration")]

  [SerializeField, Range(0f, 3f)] private float _colliderRadius = 0.5f;
  [SerializeField, Tag] private string _playerTag = "Player";
  [SerializeField] private bool _ignoreLDtk = false;

  [ShowIf("_ignoreLDtk"), SerializeField, OnValueChanged(nameof(UpdateAbilitySprite))]
  private NeroArmType _armTypeToGivePlayer = NeroArmType.Needle;

  [SerializeField, Range(0f, 1f), OnValueChanged(nameof(OnIconScaleChanged))]
  private float _iconScale = 0.45f;

  [SerializeField, Range(0f, 1f)] private float _amplitude = 1f; // Controls the height of the movement
  [SerializeField, Range(0f, 25f)] private float _frequency = 1f;  // Controls the speed of the movement
  [SerializeField, MinMaxSlider(-1f, 1f)] private Vector2 _randomStartTimeOffsetMinMax = Vector2.zero;

  [SerializeField, ReadOnly] private Vector3 _initialPosition; // Stores the object's starting position
  [SerializeField, ReadOnly] private float _amountOfTimeToWaitTillAnimate = 0f;
  [SerializeField, ReadOnly] private float _randomStartTime = 0f;

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
    if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
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

  private void Start()
  {
    _initialPosition = transform.position;

    UpdateAbilitySprite();
  }

  private void OnEnable()
  {
    _amountOfTimeToWaitTillAnimate = UnityEngine.Random.Range(_randomStartTimeOffsetMinMax.x, _randomStartTimeOffsetMinMax.y);
    _randomStartTime += _amountOfTimeToWaitTillAnimate;

    transform.position = new(
      transform.position.x,
      transform.position.y + 0.5f,
      transform.position.z
    );

    _circleCollider2D.radius = _colliderRadius;
    _circleCollider2D.isTrigger = true;
  }

  // private void OnDisable() {}

  private void Update()
  {
    _randomStartTime += Time.deltaTime;
    _randomStartTime = Mathf.Clamp(_randomStartTime + Time.deltaTime, 0f, float.MaxValue);
    if (_randomStartTime >= float.MaxValue) _randomStartTime = 0f;

    transform.position = new Vector3(
      _initialPosition.x,
      _initialPosition.y + Mathf.Sin(_randomStartTime * _frequency) * _amplitude,
      _initialPosition.z
    );
  }

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

  private void UpdateAbilitySprite()
  {
    if (_ldtkComponentEntity != null)
    {
      _spriteRenderer.sprite = _ldtkComponentEntity.Identifier switch
      {
        "Needle_Pickup" => _needleIcon,
        "Claw_Pickup" => _clawIcon,
        "Cannon_Pickup" => _cannonIcon,
        _ => _brambleIcon,
      };
    }
    else
    {
      _spriteRenderer.sprite = _armTypeToGivePlayer switch
      {
        NeroArmType.Neutral => _brambleIcon,
        NeroArmType.Needle => _needleIcon,
        NeroArmType.Claw => _clawIcon,
        NeroArmType.Gun => _cannonIcon,
        _ => _brambleIcon,
      };
    }
  }

  private void OnIconScaleChanged()
  {
    transform.localScale = Vector3.one * _iconScale;
  }
}
