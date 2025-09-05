using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class BulletHandler : MonoBehaviour
{
  public Vector2 DirectionOfFire = Vector2.zero;

  [Space(10f)]

  [SerializeField] private BulletHandlerComponentReferences _componentRefs;
  [SerializeField] private IntIntIntEventChannelSO _doDamageToEntity;

  [Space(10f)]

  [SerializeField, Tag] private string _tagToCollideWith = "Enemy";
  [SerializeField] private LayerMask _layersToCollideWith;

  [Space(10f)]

  [SerializeField, Expandable] private PlayerAbilityDataSO _playerAbilityData;
  [SerializeField, Expandable] private BulletDataSO _bulletData;
  [SerializeField] private FloatFloatEventChannelSO _cameraShakeEvent;

  private readonly float _bulletSpeedBase = 1000f;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_bulletData == null)
    {
      Debug.LogError(name + " does not have a BulletDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_playerAbilityData == null)
    {
      Debug.LogError(name + " does not have a PlayerAbilityDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_componentRefs.rigidbody2D == null) _componentRefs.rigidbody2D = GetComponent<Rigidbody2D>();
    if (_componentRefs.circleCollider2D == null) _componentRefs.circleCollider2D = GetComponent<CircleCollider2D>();
    if (_componentRefs.spriteRenderer == null) _componentRefs.spriteRenderer = GetComponent<SpriteRenderer>();
  }

  private void Start()
  {
    if (DirectionOfFire == Vector2.zero)
      _componentRefs.rigidbody2D.linearVelocity = _bulletData.Speed * _bulletSpeedBase * Vector2.right * Time.fixedDeltaTime;
    else
      _componentRefs.rigidbody2D.linearVelocity = _bulletData.Speed * _bulletSpeedBase * DirectionOfFire * Time.fixedDeltaTime;
  }

  // private void OnEnable() {}
  // private void OnDisable() {}
  // private void Update() {}
  // private void FixedUpdate() {}

  public void OnCollisionEnter2D(Collision2D collision)
  {
    OnTriggerEnter2D(collision.collider);
  }

  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (collider.gameObject.CompareTag(_tagToCollideWith))
    {
      if (_playerAbilityData.CurrentlyEquippedArm != null
        && _playerAbilityData.CurrentlyEquippedArm.CombatAbility != null
        && _playerAbilityData.CurrentlyEquippedArmType == NeroArmType.Gun)
      {
        if (_doDamageToEntity != null)
        {
          _doDamageToEntity.RaiseEvent(
            collider.gameObject.GetInstanceID(),
            _playerAbilityData.CurrentlyEquippedArm.CombatAbility.Damage,
            _playerAbilityData.CurrentlyEquippedArm.CombatAbility.KnockbackForce
          );

          if (_cameraShakeEvent != null)
          {
            _cameraShakeEvent.RaiseEvent(
              _playerAbilityData.CurrentlyEquippedArm.CombatAbility.CameraShakeSettings.Intensity,
              _playerAbilityData.CurrentlyEquippedArm.CombatAbility.CameraShakeSettings.Duration
            );
          }
        }
      }
    }

    if ((_layersToCollideWith & (1 << collider.gameObject.layer)) != 0)
    {
      Destroy(gameObject);
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
}
