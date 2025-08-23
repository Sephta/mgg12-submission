using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Ability SO", menuName = "Scriptable Objects/Player/Player Ability Data")]
public class PlayerAbilityDataSO : ScratchpadDataSO
{
  [field: SerializeField, Expandable, ReadOnly]
  public NeroArmDataSO CurrentlyEquippedNeroArm { get; private set; }

  [Expandable] public List<NeroArmDataSO> ArmData = new();

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (!CurrentlyEquippedNeroArm && ArmData.Count > 0)
    {
      CurrentlyEquippedNeroArm = ArmData[0];
    }
  }
}
