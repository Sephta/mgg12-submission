using NaughtyAttributes;
using UnityEngine;

public class PlayerHealthHandler : MonoBehaviour
{
  [Space(10f)]

  [SerializeField, Expandable, Required] private PlayerHealthSO _playerHealth;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_playerHealth == null)
    {
      Debug.LogError(name + " does not have a PlayerHealthSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }
  }

  // private void Start() {}

  private void OnEnable()
  {
    if (_playerHealth.PlayerDeathEvent != null)
    {
      _playerHealth.PlayerDeathEvent.OnEventRaised += KillPlayer;
    }
  }

  private void OnDisable()
  {
    if (_playerHealth.PlayerDeathEvent != null)
    {
      _playerHealth.PlayerDeathEvent.OnEventRaised -= KillPlayer;
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
  private void KillPlayer()
  {
    Debug.Log("Player died");
    // Destroy(gameObject);
  }
}
