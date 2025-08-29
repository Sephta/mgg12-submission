using NaughtyAttributes;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
  [Space(10f)]

  [SerializeField, Range(0f, 1000f)] private int _damageAmount = 10;

  [Space(5f)]

  [SerializeField, Tag] private string _tagToDealDamageTo = "Player";

  [Space(5f)]

  [SerializeField] private IntIntEventChannelSO _eventToTrigger;
  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_eventToTrigger == null)
    {
      Debug.LogError(name + " does not have a IntIntEventChannelSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }
  }


  private void OnCollisionEnter2D(Collision2D collision)
  {
    OnTriggerEnter2D(collision.collider);
  }

  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (collider.CompareTag(_tagToDealDamageTo))
    {
      _eventToTrigger.RaiseEvent(collider.gameObject.GetInstanceID(), _damageAmount);
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
}
