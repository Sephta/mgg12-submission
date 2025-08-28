using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHealthSO", menuName = "Scriptable Objects/PlayerHealthSO")]
public class PlayerHealthSO : ScriptableObject
{
  [field: Space(10f)]

  [field: Header("Player Health Parameters")]

  [field: Space(5f)]

  [field: SerializeField, Range(0f, 1000f)]
  public float MinHealth { get; private set; }

  [field: Space(5f)]

  [field: SerializeField, Range(1f, 1000f)]
  public float MaxHealth { get; private set; }

  [field: Space(5f)]

  [field: SerializeField, ReadOnly]
  public float CurrentHealth { get; private set; }

  [field: Space(5f)]

  [field: SerializeField, Expandable]
  public IntIntEventChannelSO DamageEvent { get; private set; }

  [field: Space(5f)]

  [field: SerializeField, Expandable]
  public VoidEventChannelSO PlayerDeathEvent { get; private set; }

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

    if (DamageEvent != null)
    {
      DamageEvent.OnEventRaised += OnDealDamageToPlayer;
    }
  }

  private void OnDisable()
  {
    CurrentHealth = MaxHealth;

    if (DamageEvent != null)
    {
      DamageEvent.OnEventRaised -= OnDealDamageToPlayer;
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */
  public void SetCurrentHealth(float amount) => CurrentHealth = amount;

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private void OnDealDamageToPlayer(int objectID, int damageAmount)
  {
    CurrentHealth = Mathf.Clamp(CurrentHealth - damageAmount, MinHealth, MaxHealth);

    if (CurrentHealth == MinHealth && PlayerDeathEvent != null)
    {
      PlayerDeathEvent.RaiseEvent();
    }
  }
}
