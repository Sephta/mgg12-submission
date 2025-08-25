using UnityEngine;

public class DebugOnPlayerHit : MonoBehaviour
{
  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (collider.gameObject.CompareTag("Player"))
      Debug.Log("Collider from: " + collider.gameObject.name);
  }
}
