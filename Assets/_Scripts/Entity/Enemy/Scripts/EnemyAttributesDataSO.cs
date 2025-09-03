using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttributesDataSO", menuName = "Scriptable Objects/EnemyAttributesDataSO")]
public class EnemyAttributesDataSO : ScriptableObject
{
  [field: Space(10f)]

  [field: SerializeField]
  public float PatrolSpeed { get; private set; }

  [field: SerializeField]
  public float ChaseSpeed { get; private set; }

  [field: SerializeField, Range(0f, 2f)]
  public float Acceleration { get; private set; }

  [field: SerializeField, Range(0f, 2f)]
  public float Deceleration { get; private set; }

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
  [field: SerializeField, Range(1, 60)] public int MaxCombatStateDuration = 30;

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


  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
}
