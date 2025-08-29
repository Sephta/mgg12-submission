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

  private void OnValidate()
  {
    // AddAnimationEventsToPlayerArmAttacks();
  }

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

    if (ArmCycledEvent != null) ArmCycledEvent.RaiseEvent();
  }

  public void CycleArmRight()
  {
    _currentArmIndex = (_currentArmIndex + 1) % ArmData.Count;

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

  // private void EnablePlayerHitzone()
  // {
  //   Debug.Log("EnablePlayerHitzone");
  // }

  // private void HandleAttackInputBuffer()
  // {
  //   Debug.Log("HandleAttackInputBuffer");
  // }

  // private void ChainAttackOrFinishCombo()
  // {
  //   Debug.Log("ChainAttackOrFinishCombo");
  // }


  // private void AddAnimationEventsToPlayerArmAttacks()
  // {
  //   foreach (NeroArmDataSO armData in ArmData)
  //   {
  //     if (armData.CombatAbility != null && armData.CombatAbility.AttackAnimationClips.Count > 0)
  //     {
  //       List<AnimationClip> attackAnimationClips = armData.CombatAbility.AttackAnimationClips;
  //       foreach (AnimationClip clip in attackAnimationClips)
  //       {
  //         AnimationEvent enableHitZone = new()
  //         {
  //           time = 0,
  //           functionName = nameof(EnablePlayerHitzone)
  //         };

  //         clip.AddEvent(enableHitZone);

  //         float timeDurringClipToRaiseInputBufferEvent = clip.length - armData.CombatAbility.AttackChainingInputBuffer;

  //         AnimationEvent startInputBuffer = new()
  //         {
  //           time = timeDurringClipToRaiseInputBufferEvent,
  //           functionName = nameof(HandleAttackInputBuffer)
  //         };

  //         clip.AddEvent(startInputBuffer);

  //         AnimationEvent endOfAttackEvent = new()
  //         {
  //           time = clip.length,
  //           functionName = nameof(ChainAttackOrFinishCombo)
  //         };

  //         clip.AddEvent(endOfAttackEvent);
  //       }
  //     }
  //   }
  // }
}
