using UnityEngine;

[CreateAssetMenu(fileName = "New Combat Ability", menuName = "Scriptable Objects/Nero/Combat Ability")]
public class CombatAbilitySO : ScriptableObject
{
  [field: Space(10f)]
  [field: Header("Combat Params")]

  [field: SerializeField, Range(0f, 3f)]
  public float Speed { get; private set; }

  [field: SerializeField, Range(0f, 100f)]
  public int Damage { get; private set; }

  [field: SerializeField, Range(0f, 50f)]
  public float Range { get; private set; }
}
