using System;
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

  private Rigidbody2D _rb;


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
    // _animator.SetFloat("speed", 1f);
    _rb = GetComponentInParent<Rigidbody2D>();
  }

  // private void OnEnable() {}
  // private void OnDisable() {}

  private void Update()
  {
    string animationToPlay = AnimationSelector();
    _animator.CrossFade(animationToPlay, _crossfadeTransitionDuration, _animatorLayerToUse);
  }

  private string AnimationSelector()
  {
    string result = "idle";
    bool isMoving = Math.Abs(_rb.linearVelocityX) > 0.01f;
    if (_enemyAttributesData.GetCombatState().Equals("taking damage", StringComparison.OrdinalIgnoreCase))
    {
      return "jump";
    }
    if (_enemyAttributesData.GetCombatState().Equals("attacking", StringComparison.OrdinalIgnoreCase))
    {
      return "jump"; // look this is just a placeholder
    }
    if (isMoving)
    {
      return "run";
    }

    return result;
  }

  // private void FixedUpdate() {}

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
}
