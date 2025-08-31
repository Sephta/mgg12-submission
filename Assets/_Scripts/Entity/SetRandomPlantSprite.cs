using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SetRandomPlantSprite : MonoBehaviour
{
  [SerializeField, Required] private SpriteRenderer _spriteRenderer = null;
  [SerializeField] private List<Sprite> plantSprites = new();

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
  }

  private void Start()
  {
    if (plantSprites.Count <= 0) return;

    _spriteRenderer.sprite = plantSprites[(int)Random.Range(0f, plantSprites.Count)];
  }

  // private void OnEnable() {}
  // private void OnDisable() {}
  // private void Update() {}
  // private void FixedUpdate() {}

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
}
