using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerHealthHandler : MonoBehaviour
{
  [Space(10f)]

  [SerializeField, Expandable, Required] private PlayerHealthSO _playerHealth;

  [Space(5f)]

  [SerializeField] private LayerMask _harmfulTerrain;
  [SerializeField] private BoxCollider2D _boxCollider2D;

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

    if (_boxCollider2D == null) _boxCollider2D = GetComponent<BoxCollider2D>();
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

  private void OnCollisionEnter2D(Collision2D collision)
  {
    OnTriggerEnter2D(collision.collider);
  }

  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (_boxCollider2D == null) return;

    if (LayerMaskContainsLayer(_harmfulTerrain, collider.gameObject.layer))
    {
      Debug.Log("Deal damage to player...");
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private bool LayerMaskContainsLayer(LayerMask layerMask, int layer) => ((1 << layer) & layerMask) != 0;

  private void KillPlayer()
  {
    Debug.Log("Player died");
    Destroy(gameObject);
  }
}
