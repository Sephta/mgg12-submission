using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class EnemyAnimatorController : MonoBehaviour
{
  [SerializeField] private Animator _animator;
  [SerializeField] private SpriteRenderer _spriteRenderer;

  [Header("Attributes Data")]

  [SerializeField, Required, Expandable] private EnemyAttributesDataSO _enemyAttributesData;


  [SerializeField] private int _crossfadeTransitionDuration = 0;
  [SerializeField] private int _animatorLayerToUse = 0;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_animator == null) GetComponent<Animator>();
    if (_spriteRenderer == null) GetComponent<SpriteRenderer>();

    if (_enemyAttributesData == null)
    {
      Debug.LogError(name + " does not have a reference to EnemyAttributesDataSO in the inspector. Disabling gameobject to avoid null object errors.");
      gameObject.SetActive(false);
    }
  }

  private void Start()
  {
    _animator.SetFloat("speed", 1f);
  }

  // private void OnEnable() {}
  // private void OnDisable() {}

  private void Update()
  {
    _animator.CrossFade("idle", _crossfadeTransitionDuration, _animatorLayerToUse);
  }

  // private void FixedUpdate() {}

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
}
