using System;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;


public class EnemyDamageListener : MonoBehaviour
{
  [Space(10f)]
  [SerializeField, Range(0f, 1000f)] private int _damageAmount = 10;
  [SerializeField] private IntIntEventChannelSO _takeDamageEvent;
  [SerializeField] private IntIntEventChannelSO _dealDamageEvent;
  [Space(5f)]
  [SerializeField, Tag] private string _tagToDealDamageTo = "Player";
  [Space(5f)]

  [SerializeField] private GameObject _enemyVisuals;

  private bool _isTakingDamage = false;
  private bool _isAttacking = false;
  private SpriteRenderer _sr;
  private Animator _animator;

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
  }

  private void Start()
  {
    _sr = _enemyVisuals.GetComponent<SpriteRenderer>();
    _animator = _enemyVisuals.GetComponent<Animator>();
  }

  private void OnEnable()
  {
    _takeDamageEvent.OnEventRaised += DoDamageToEntity;
    // _isTakingDamage = true;
  }

  private void OnDisable()
  {
    _takeDamageEvent.OnEventRaised -= DoDamageToEntity;
    // _isTakingDamage = false;
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    OnTriggerEnter2D(collision.collider);
  }

  // inflict damage
  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (collider.CompareTag(_tagToDealDamageTo) && !_isTakingDamage)
    {
      _isAttacking = true;
      _dealDamageEvent.RaiseEvent(collider.gameObject.GetInstanceID(), _damageAmount);
      Debug.Log($"{name} attempting to inflict {_damageAmount} damage to <ID: {collider.gameObject.GetInstanceID()}>");
    }
    _isAttacking = false;
  }

  // take damage
  private void DoDamageToEntity(int objectID, int damageAmount)
  {
    Color originalColor = _sr.color;
    if (objectID == gameObject.GetInstanceID())
    {
      _isTakingDamage = true;
      _sr.color = Color.red;
      Debug.Log($"{name} is taking {damageAmount} damage from <ID: {objectID}>");
    }
    _isTakingDamage = false;
  }

  // I know we talked about putting public at top, but considering the functions of out private
  // methods versus this public one... lol
  public bool IsTakingDamage()
  {
    return _isTakingDamage;
  }

  public bool IsAttacking()
  {
    return _isAttacking;
  }

}
