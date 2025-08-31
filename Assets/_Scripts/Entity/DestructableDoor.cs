using System.Collections.Generic;
using LDtkUnity;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DestructableDoor : MonoBehaviour
{
  [Header("Component Fields")]

  [SerializeField] private BoxCollider2D _boxCollider2D;
  [SerializeField, ReadOnly] private LDtkComponentEntity _ldtkComponentEntity;

  [Header("Configurable Data")]

  [OnValueChanged("UpdateBoxColliderOffset")]
  [SerializeField] private Vector2 _colliderOffset = new();

  [SerializeField] private GameObject _doorPiece;

  [SerializeField] private Vector2 _doorPieceOffset = new();
  [SerializeField] private Vector2 _doorBuildDirection = new();

  [SerializeField, Tag] private string _tagToCompareTo = "Bullet";

  [Header("Debug")]

  [SerializeField, ReadOnly] private List<GameObject> _doorPieces = new();

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_boxCollider2D == null) _boxCollider2D = GetComponent<BoxCollider2D>();
    if (_ldtkComponentEntity == null) _ldtkComponentEntity = GetComponent<LDtkComponentEntity>();

    if (_doorPiece == null)
    {
      Debug.LogError(name + " is missing prefab reference to _doorPiece. Disabling gameobject to avoid null object errors.");
      gameObject.SetActive(false);
    }
  }

  // private void Start() {}

  private void OnEnable()
  {
    UpdateBoxColliderOffset();

    if (_ldtkComponentEntity != null)
    {
      if (_ldtkComponentEntity.Identifier == "Door_Lock")
      {
        _doorPieces.Clear();

        transform.localScale = new(1f, 1f, 1f);

        _boxCollider2D.size = _ldtkComponentEntity.Size;

        int numberOfDoorPiecesToSpawn = 0;
        if (_doorBuildDirection.y != 0) numberOfDoorPiecesToSpawn = (int)_ldtkComponentEntity.Size.y;
        if (_doorBuildDirection.x != 0) numberOfDoorPiecesToSpawn = (int)_ldtkComponentEntity.Size.x;

        Vector3 doorPiecePosition = transform.position + (Vector3)_doorPieceOffset;

        for (int i = 0; i < numberOfDoorPiecesToSpawn; i++)
        {
          GameObject newDoorPiece = Instantiate(
            _doorPiece,
            doorPiecePosition,
            transform.rotation,
            transform
          );

          newDoorPiece.transform.localScale = new(1f, 1f, 1f);

          _doorPieces.Add(newDoorPiece);

          doorPiecePosition += (Vector3)_doorBuildDirection;
        }
      }
    }
  }

  // private void OnDisable() {}
  // private void Update() {}
  // private void FixedUpdate() {}

  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (collision.gameObject.CompareTag(_tagToCompareTo))
    {
      Debug.Log("Hit made contact with door");
      foreach (GameObject doorPiece in _doorPieces)
      {
        Destroy(doorPiece);
      }

      Destroy(gameObject);
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private void UpdateBoxColliderOffset()
  {
    if (_boxCollider2D == null) return;

    _boxCollider2D.offset = _colliderOffset;
  }
}
