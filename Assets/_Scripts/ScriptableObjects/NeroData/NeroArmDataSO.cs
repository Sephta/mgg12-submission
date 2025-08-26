using NaughtyAttributes;
using UnityEngine;

public enum NeroArmType
{
  Neutral,
  Needle,
  Claw,
  Gun
}

[CreateAssetMenu(fileName = "New Nero Arm", menuName = "Scriptable Objects/Nero/Nero Arm")]
public class NeroArmDataSO : ScriptableObject
{
  [Space(10f)]

  public NeroArmType ArmType;

  [field: Space(10f)]

  [field: SerializeField]
  public RuntimeAnimatorController AnimatorController { get; private set; }

  [field: Space(10f)]
  [field: Header("Abilities")]

  [field: SerializeField, Expandable]
  public EnvironmentalAbilitySO EnvironmentalAbility { get; private set; }

  [field: SerializeField, Expandable]
  public CombatAbilitySO CombatAbility { get; private set; }
}
