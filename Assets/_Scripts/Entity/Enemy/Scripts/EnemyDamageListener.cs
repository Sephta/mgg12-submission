using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class EnemyDamageListener : MonoBehaviour
{
  [SerializeField] private Rigidbody2D _parentRigidbody2D;

  [SerializeField, Expandable] private EnemyAttributesDataSO _enemyAttributesData;

  [Space(10f)]
  [SerializeField, Range(0f, 1000f)] private int _damageAmount = 10;
  [SerializeField] private IntIntEventChannelSO _takeDamageEvent;
  [SerializeField] private IntIntEventChannelSO _dealDamageEvent;
  [Space(5f)]
  [SerializeField, Tag] private string _tagToDealDamageTo = "Player";
  [Space(5f)]

  [SerializeField] private GameObject _enemyVisuals;
  [SerializeField, Range(0f, 1f)] private float _howLongToFlashDamageColor = 1f;
  [SerializeField, ReadOnly] private bool _coroutineRunning = false;


  [field: SerializeField, Range(1, 30)] private int _maxStateDuration = 15;
  [field: SerializeField, ReadOnly] private int _currentStateTimer = 0;
  [field: SerializeField, ReadOnly] private bool _isTimerDone = true;
  // [field: SerializeField, ReadOnly] private bool isStateDone = true;

  private bool _isAttacking = false;
  private bool _isTakingDamage = false;
  private SpriteRenderer _sr;
  private Animator _animator;
  private Color _originalSpriteColor;


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

    if (_parentRigidbody2D == null) _parentRigidbody2D = GetComponentInParent<Rigidbody2D>();
  }

  private void Start()
  {
    _sr = _enemyVisuals.GetComponent<SpriteRenderer>();
    _originalSpriteColor = _sr.color;
    _animator = _enemyVisuals.GetComponent<Animator>();
  }

  private void OnEnable()
  {
    _takeDamageEvent.OnEventRaised += DoDamageToEntity;
  }

  private void OnDisable()
  {
    _takeDamageEvent.OnEventRaised -= DoDamageToEntity;
  }

  private void Update()
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
    else
    {
      _enemyAttributesData.SetCombatState("none");
      _isAttacking = false;
      _isTakingDamage = false;
    }
  }

  private void OnCollisionEnter2D(Collision2D collision) => OnTriggerEnter2D(collision.collider);

  // inflict damage
  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (collider.CompareTag(_tagToDealDamageTo) && !_isTakingDamage)
    {
      if (_isTimerDone)
      {
        _isAttacking = true;
        _currentStateTimer = _maxStateDuration;
        _enemyAttributesData.SetCombatState("attacking");
        _isTimerDone = false;
      }
      _dealDamageEvent.RaiseEvent(collider.gameObject.GetInstanceID(), _damageAmount);
      Debug.Log($"{name} attempting to inflict {_damageAmount} damage to <ID: {collider.gameObject.GetInstanceID()}>");
    }
  }

  // take damage
  private void DoDamageToEntity(int objectID, int damageAmount)
  {
    if (_parentRigidbody2D == null) return;
    if (_enemyAttributesData == null) return;
    if (_enemyAttributesData.PlayerTransform == null) return;

    if (objectID == gameObject.GetInstanceID())
    {
      StartDamageFlash();
      Debug.Log($"{name} is taking {damageAmount} damage from <ID: {objectID}>");
      _enemyAttributesData.TakeDamage(damageAmount);

      var directionToApplyForce = (transform.position - _enemyAttributesData.PlayerTransform.position).normalized;
      _parentRigidbody2D.AddForce(directionToApplyForce * _enemyAttributesData.KnockbackForce, ForceMode2D.Impulse);

      if (_isTimerDone)
      {
        _isTakingDamage = true;
        _currentStateTimer = _maxStateDuration;
        _enemyAttributesData.SetCombatState("taking damage");
        _isTimerDone = false;
      }

    }
  }


  private void StartDamageFlash()
  {
    // Don't start another coroutine if we're already running one
    if (_coroutineRunning) return;

    IEnumerator flashRoutine = Wait(_howLongToFlashDamageColor);

    StartCoroutine(flashRoutine);
  }

  private IEnumerator Wait(float duration)
  {
    _coroutineRunning = true;

    _sr.color = Color.red;
    yield return new WaitForSecondsRealtime(duration);
    _sr.color = _originalSpriteColor;

    _coroutineRunning = false;
  }




  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  public bool IsTakingDamage() { return _isTakingDamage; }
  public bool IsAttacking() { return _isAttacking; }

}
