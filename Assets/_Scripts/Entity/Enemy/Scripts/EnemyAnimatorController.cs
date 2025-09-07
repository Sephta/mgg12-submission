using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class EnemyAnimatorController : MonoBehaviour
{
  [SerializeField] private Animator _animator;
  [SerializeField] private SpriteRenderer _spriteRenderer;

  [SerializeField, Range(0f, 1f)] private float _howLongToFlashDamageColor = 1f;
  [SerializeField, ReadOnly] private bool _coroutineRunning = false;

  [SerializeField] private int _crossfadeTransitionDuration = 0;
  [SerializeField] private int _animatorLayerToUse = 0;

  [SerializeField, ReadOnly] private string _selectedAnimation;
  private Rigidbody2D _rb;
  private Color _originalSpriteColor;



  [Header("Attributes Data")]
  [SerializeField, Required, Expandable] private EnemyAttributesDataSO _enemyAttributesData;

  private string _combatState;



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

  private void OnEnable()
  {
    _rb = GetComponentInParent<Rigidbody2D>();
    _originalSpriteColor = _spriteRenderer.color;
    _combatState = "none";
  }
  private void OnDisable()
  {
    _spriteRenderer.color = _originalSpriteColor;
  }

  private void Update()
  {
    string animationToPlay = AnimationSelector();
    _selectedAnimation = animationToPlay;
    _animator.CrossFade(animationToPlay, _crossfadeTransitionDuration, _animatorLayerToUse);
  }

  private string AnimationSelector()
  {
    string result = "idle";
    bool isMoving = Math.Abs(_rb.linearVelocityX) > 0.01f;
    if (_combatState.Equals("dead", StringComparison.OrdinalIgnoreCase))
    {
      return "dead";
    }
    if (_combatState.Equals("dying", StringComparison.OrdinalIgnoreCase))
    {
      return "death";
    }
    if (_combatState.Equals("taking damage", StringComparison.OrdinalIgnoreCase))
    {
      StartDamageFlash();
      return "ouch";
    }
    if (_combatState.Equals("attacking", StringComparison.OrdinalIgnoreCase))
    {
      return "attack"; // look this is just a placeholder
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
  public void SetAnimationCombatState(string state) => _combatState = state;
  public string GetCurrentAnimation() { return _selectedAnimation; }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private void StartDamageFlash()
  {
    // Don't start another coroutine if we're already running one
    if (_coroutineRunning) return;

    IEnumerator flashRoutine = Wait(_howLongToFlashDamageColor);

    StartCoroutine(flashRoutine);
  }

  private IEnumerator Wait(float duration)
  {
    _coroutineRunning = true;

    _spriteRenderer.color = Color.red;
    yield return new WaitForSecondsRealtime(duration);
    _spriteRenderer.color = _originalSpriteColor;

    _coroutineRunning = false;
  }
}
