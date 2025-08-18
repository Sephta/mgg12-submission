using UnityEngine;

public class HelloWorldOnCollisionEnter : MonoBehaviour
{
  [Header("For Testing")]
  [SerializeField]
  private LayerMask _layersToCollideWith;

  public void OnCollisionEnter2D(Collision2D collision)
  {
    HandleCollision(collision);
  }

  private bool LayerMaskContainsLayer(LayerMask layerMask, int layer) => ((1 << layer) & layerMask) != 0;

  private void HandleCollision(Collision2D collision)
  {
    if (_layersToCollideWith == 0)
    {
      Debug.LogWarning("_layersToCollideWith is default layer for OnCollisionEnter2D on game object with name: " + name);
      return;
    }


    if (LayerMaskContainsLayer(_layersToCollideWith, collision.gameObject.layer))
    {
      Debug.Log("I am collider: " + name);
    }
  }
}
