using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttributesDataSO", menuName = "Scriptable Objects/EnemyAttributesDataSO")]
public class EnemyAttributesDataSO : ScriptableObject
{
  [field: Space(10f)]

  [field: SerializeField]
  public float PatrolSpeed { get; private set; }

  [field: SerializeField]
  public float ChaseSpeed { get; private set; }

  [field: SerializeField]
  public float RangeAwakeDistance { get; private set; }

  [field: SerializeField, Range(1f, 100f)]
  public float KnockbackForce { get; private set; }

  [field: Space(10f)]

  [field: Header("Enemy Health Parameters")]

  [field: Space(5f)]

  [field: SerializeField, Range(0f, 1000f)]
  public float MinHealth { get; private set; }

  [field: Space(5f)]

  [field: SerializeField, Range(1f, 1000f)]
  public float MaxHealth { get; private set; }

  [field: Space(5f)]

  [field: SerializeField, ReadOnly]
  public float CurrentHealth { get; private set; }

  [ReadOnly] public Transform PlayerTransform;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void OnValidate()
  {
    CurrentHealth = MaxHealth;
  }

  private void OnEnable()
  {
    CurrentHealth = MaxHealth;
  }

  private void OnDisable()
  {
    CurrentHealth = MaxHealth;
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */
  public void SetCurrentHealth(float amount) => CurrentHealth = amount;

  public void TakeDamage(float damageAmount)
  {
    CurrentHealth = Mathf.Clamp(CurrentHealth - damageAmount, MinHealth, MaxHealth);

    if (CurrentHealth == MinHealth)
    {
      Debug.Log("I dieded");
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
}
