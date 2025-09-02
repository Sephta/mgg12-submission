using NaughtyAttributes;
using UnityEngine;

public class EnemyCombatStates : MonoBehaviour
{

  [SerializeField, Expandable] private EnemyAttributesDataSO _enemyAttributesData;
  [field: SerializeField, ReadOnly] public float CurrentHealth { get; private set; }
  [field: SerializeField, ReadOnly] private string _combatState = "none";  // look, I'm just getting it done

  [SerializeField] private GameObject _enemyVisuals;
  private float _minHealth;
  private float _maxHealth;
  private EnemyAnimatorController _animController;


  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */
  private void Awake()
  {
    if (_enemyAttributesData == null)
    {
      Debug.LogError(name + " does not have a reference to EnemyAttributesDataSO in the inspector. Disabling gameobject to avoid null object errors.");
      gameObject.SetActive(false);
    }
    if (_enemyVisuals == null)
    {
      Debug.LogError(name + " does not have a reference to EnemyVisuals in the inspector. Disabling gameobject to avoid null object errors.");
      gameObject.SetActive(false);
    }
  }

  private void OnEnable()
  {
    _minHealth = _enemyAttributesData.MinHealth;
    _maxHealth = _enemyAttributesData.MaxHealth;
    _combatState = "none";
    _animController = _enemyVisuals.GetComponent<EnemyAnimatorController>();
    CurrentHealth = _maxHealth;
  }

  private void OnDisable()
  {
    CurrentHealth = _maxHealth;
    _combatState = "none";
  }


  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */


  public void SetCurrentHealth(float amount) => CurrentHealth = amount;
  public void SetCombatState(string state)
  {
    if (_combatState.Equals("dead", System.StringComparison.OrdinalIgnoreCase)) return;
    _combatState = state;
    _animController.SetAnimationCombatState(state);
  }
  public string GetCombatState() { return _combatState; }
  public bool IsDead() { return _combatState.Equals("dead", System.StringComparison.OrdinalIgnoreCase); }

  public bool TakeDamage(float damageAmount)
  {
    bool isDead = false;
    CurrentHealth = Mathf.Clamp(CurrentHealth - damageAmount, _minHealth, _maxHealth);
    if (CurrentHealth == _minHealth)
    {
      isDead = true;
      Debug.Log("I dieded");
    }
    return isDead;
  }


  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

}

