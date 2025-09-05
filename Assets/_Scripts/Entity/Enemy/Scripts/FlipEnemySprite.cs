using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FlipEnemySprite : MonoBehaviour
{
  [Header("Component References"), Space(10f)]

  [SerializeField] private SpriteRenderer _spriteRenderer;
  [SerializeField] private Rigidbody2D _parentRigidbody2D;

  [SerializeField] private bool _spriteIsDefaultFacingLeft = false;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
    if (_parentRigidbody2D == null) _parentRigidbody2D = GetComponentInParent<Rigidbody2D>();
  }

  // private void Start() {}
  // private void OnEnable() {}
  // private void OnDisable() {}

  private void Update()
  {
    FlipSpriteBasedOnPlayerAttributesData();
  }

  // private void FixedUpdate() {}

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
  private void FlipSpriteBasedOnPlayerAttributesData()
  {
    if (_parentRigidbody2D == null) return;

    if (Mathf.Abs(_parentRigidbody2D.linearVelocityX) >= 0.01f)
    {
      _spriteRenderer.flipX = _spriteIsDefaultFacingLeft ? _parentRigidbody2D.linearVelocityX > 0f : _parentRigidbody2D.linearVelocityX < 0f;
    }
  }
}
