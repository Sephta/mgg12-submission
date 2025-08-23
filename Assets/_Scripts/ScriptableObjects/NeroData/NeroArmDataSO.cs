using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "New Nero Arm", menuName = "Scriptable Objects/Nero/Nero Arm")]
public class NeroArmDataSO : ScriptableObject
{
  [field: Space(10f)]
  [field: Header("Abilities")]

  [field: SerializeField, Expandable]
  public EnvironmentalAbilitySO EnvironmentalAbility { get; private set; }

  [field: SerializeField, Expandable]
  public CombatAbilitySO CombatAbility { get; private set; }
}
