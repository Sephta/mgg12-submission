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

  public void CycleArmLeft()
  {
    _currentArmIndex--;

    if (_currentArmIndex < 0)
    {
      _currentArmIndex = ArmData.Count - 1;
    }
    UnityEngine.Debug.Log("Index: " + _currentArmIndex);

    if (ArmCycledEvent != null) ArmCycledEvent.RaiseEvent();
  }

  public void CycleArmRight()
  {
    _currentArmIndex = (_currentArmIndex + 1) % ArmData.Count;
    UnityEngine.Debug.Log("Index: " + _currentArmIndex);

    if (ArmCycledEvent != null) ArmCycledEvent.RaiseEvent();
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  [Button("ResetCurrentArmIndex")]
  private void ResetCurrentArmIndex()
  {
    _currentArmIndex = 0;
  }
}
