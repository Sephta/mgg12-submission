using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Splines;

public class BrambleSpawner : MonoBehaviour
{
  [Header("Components")]
  [SerializeField]
  private CircleCollider2D _circleCollider2D;
  [SerializeField]
  private SplineContainer _splineContainer;

  [Header("Spawn Parameters")]
  [SerializeField, Expandable]
  private BrambleSpawnParametersSO _brambleSpawnParametersSO;

  [Header("For Testing")]
  [SerializeField]
  private LayerMask _layersToCollideWith;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_circleCollider2D == null) _circleCollider2D = GetComponent<CircleCollider2D>();

    // Check if we have one attached, if not then add one
    if (_splineContainer == null) _splineContainer = GetComponent<SplineContainer>();
    if (_splineContainer == null) _splineContainer = gameObject.AddComponent<SplineContainer>();

    if (_brambleSpawnParametersSO == null)
    {
      Debug.LogError("_brambleSpawnParametersSO is null for game object: " + name);
    }
  }

  // private void Start() {}
  // private void Update() {}
  // private void FixedUpdate() {}

  /* ---------------------------------------------------------------- */
  /*                        Collider2D Messages                       */
  /* ---------------------------------------------------------------- */

  public void OnCollisionEnter2D(Collision2D collision)
  {
    HandleCollision(collision);
  }

  // public void OnCollisionStay2D(Collision2D collision)
  // {
  //   HandleCollision(collision);
  // }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  public void SpawnBramble()
  {
    Debug.Log("Spawning bramble...");
    // _splineContainer.Spline.Add(new K)
  }


  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

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
      Debug.Log(collision.gameObject.name);
      SpawnBramble();
    }
  }
}
