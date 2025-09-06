using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Ability SO", menuName = "Scriptable Objects/Player/Player Ability Data")]
public class PlayerAbilityDataSO : ScratchpadDataSO
{
  [SerializeField, ReadOnly]
  private int _currentArmIndex = 0;

  [Expandable] public List<NeroArmDataSO> ArmData = new();

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  public NeroArmDataSO CurrentlyEquippedArm => ArmData[_currentArmIndex];

  public NeroArmType CurrentlyEquippedArmType => ArmData[_currentArmIndex].ArmType;

  [Expandable] public VoidEventChannelSO ArmCycledEvent;

  private void OnEnable()
  {
    ResetArms();
  }

  private void OnDisable()
  {
    ResetArms();
  }

  public void CycleArmLeft()
  {
    _currentArmIndex--;

    if (_currentArmIndex < 0)
    {
      _currentArmIndex = ArmData.Count - 1;
    }

    if (ArmCycledEvent != null) ArmCycledEvent.RaiseEvent();
  }

  public void CycleArmRight()
  {
    _currentArmIndex = (_currentArmIndex + 1) % ArmData.Count;

    if (ArmCycledEvent != null) ArmCycledEvent.RaiseEvent();
  }

  public bool AddAbility(NeroArmDataSO ability)
  {
    if (!ArmData.Contains(ability))
    {
      ArmData.Add(ability);
      return true;
    }

    return false;
  }

  public void RemoveAbility(NeroArmType abilityType)
  {

  }

  public void ResetArms()
  {
    if (ArmData.Count > 1)
    {
      ArmData.RemoveRange(1, ArmData.Count - 1);
    }
  }

  [Button("Reset Current Arm Index")]
  public void ResetCurrentArmIndex()
  {
    _currentArmIndex = 0;
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
}
