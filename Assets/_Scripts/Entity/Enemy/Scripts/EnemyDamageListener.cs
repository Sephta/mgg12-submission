using System;
using NaughtyAttributes;
using UnityEngine;

public class EnemyDamageListener : MonoBehaviour
{
  [SerializeField] private Rigidbody2D _parentRigidbody2D;
  [SerializeField] private BoxCollider2D _boxCollider2D;

  [SerializeField, Expandable] private EnemyAttributesDataSO _enemyAttributesData;

  [Space(10f)]
  [SerializeField, Range(0f, 1000f)] private int _damageAmount = 10;
  [SerializeField] private IntIntIntEventChannelSO _takeDamageEvent;
  [SerializeField] private IntIntEventChannelSO _dealDamageEvent;
  [Space(5f)]
  [SerializeField, Tag] private string _tagToDealDamageTo = "Player";

  [Space(5f)]

  [SerializeField, ReadOnly] private float _currentHealth;
  [field: SerializeField, ReadOnly] private int _maxStateDuration = 30;
  [field: SerializeField, ReadOnly] private int _currentStateTimer = 0;
  [field: SerializeField, ReadOnly] private bool _isTimerDone = true;
  // [field: SerializeField, ReadOnly] private bool isStateDone = true;

  private bool _isAttacking = false;
  private bool _isTakingDamage = false;
  private bool _isDead = false;
  private EnemyCombatStates _combatStates;
  private Collider2D _colliderToDamage;


  private void Awake()
  {
    if (_takeDamageEvent == null)
    {
      Debug.LogError(name + " does not have a IntIntEventChannelSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }
    if (_dealDamageEvent == null)
    {
      Debug.LogError(name + " does not have a IntIntEventChannelSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_enemyAttributesData == null)
    {
      Debug.LogError(name + " does not have a reference to EnemyAttributesDataSO in the inspector. Disabling gameobject to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_parentRigidbody2D == null) _parentRigidbody2D = GetComponentInParent<Rigidbody2D>();
    if (_boxCollider2D == null) _boxCollider2D = GetComponent<BoxCollider2D>();
  }

  private void OnEnable()
  {
    _takeDamageEvent.OnEventRaised += DoDamageToEntity;
    _combatStates = GetComponent<EnemyCombatStates>();
    _maxStateDuration = _enemyAttributesData.MaxCombatStateDuration;
  }

  private void OnDisable()
  {
    _takeDamageEvent.OnEventRaised -= DoDamageToEntity;
  }

  private void FixedUpdate()
  {

    if (_currentStateTimer <= 1)
    {
      _currentStateTimer = 0;
      _isTimerDone = true;
    }

    if (!_isTimerDone)
    {
      _currentStateTimer--;
    }

    if (_combatStates.GetCombatState().Equals("dying", StringComparison.OrdinalIgnoreCase))
    {
      if (_isTimerDone)
      {
        _combatStates.SetCombatState("dead");
        GetComponentInParent<EnemyPatrol>().KillYourself();
      }
      return;
    }

    if (_isTimerDone)
    {
      if (_isAttacking && _colliderToDamage != null)
      {
        InflictDamageToPlayer(_colliderToDamage);
        return;
      }
      _combatStates.SetCombatState("none");
      _isAttacking = false;
      _isTakingDamage = false;
    }

  }

  private void OnCollisionEnter2D(Collision2D collision) => OnTriggerEnter2D(collision.collider);
  private void OnCollisionExit2D(Collision2D collision) => OnTriggerExit2D(collision.collider);


  // inflict damage
  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (_isDead) return;

    if (collider.CompareTag(_tagToDealDamageTo) && !_isTakingDamage)
    {
      _colliderToDamage = collider;
      InflictDamageToPlayer(collider);
    }
  }

  private void OnTriggerExit2D(Collider2D collider)
  {
    if (_isDead) return;
    if (collider.CompareTag(_tagToDealDamageTo))
    {
      _isAttacking = false;
    }
  }

  private void InflictDamageToPlayer(Collider2D collider)
  {
    if (_isTakingDamage || _isDead) return;
    if (_isTimerDone)
    {
      _isAttacking = true;
      _currentStateTimer = _maxStateDuration;
      _combatStates.SetCombatState("attacking");
      _isTimerDone = false;
    }
    _dealDamageEvent.RaiseEvent(collider.gameObject.GetInstanceID(), _damageAmount);
  }

  // take damage
  private void DoDamageToEntity(int objectID, int damageAmount, int knockbackForce)
  {
    if (_parentRigidbody2D == null) return;
    if (_enemyAttributesData == null) return;
    if (_enemyAttributesData.PlayerTransform == null) return;

    if (objectID == gameObject.GetInstanceID())
    {
      // Debug.Log($"{name} is taking {damageAmount} damage from <ID: {objectID}>");
      _isDead = _combatStates.TakeDamage(damageAmount);

      var directionToApplyForce = (transform.position - _enemyAttributesData.PlayerTransform.position).normalized;
      _parentRigidbody2D.AddForce(directionToApplyForce * knockbackForce, ForceMode2D.Impulse);

      if (_isDead)
      {
        _boxCollider2D.enabled = false;
        _parentRigidbody2D.gravityScale = 0f;
        _parentRigidbody2D.linearVelocity = Vector2.zero;
        _currentStateTimer = _maxStateDuration;
        _combatStates.SetCombatState("dying");
        _isTimerDone = false;
        _isAttacking = false;
        _isTakingDamage = false;
        return;
      }

      // if (_isTimerDone)
      // {
      //   _isTakingDamage = true;
      //   _currentStateTimer = _maxStateDuration;
      //   _combatStates.SetCombatState("taking damage");
      //   _isTimerDone = false;
      // }

      // let's see how it feels if the player can stunlock the enemy...
      _isTakingDamage = true;
      _isAttacking = false;
      _currentStateTimer = _maxStateDuration;
      _combatStates.SetCombatState("taking damage");
      _isTimerDone = false;

    }
  }


  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  public bool IsTakingDamage() { return _isTakingDamage; }
  public bool IsAttacking() { return _isAttacking; }

}
