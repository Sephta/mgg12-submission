using System.Collections.Generic;
using NaughtyAttributes;
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

  [field: SerializeField, Range(-50f, 50f)]
  public float AttackMovementForce { get; private set; }

  [field: SerializeField, Tooltip("Ignores the player move direction when applying attack movement forces.")]
  public bool IgnoreMoveDirection { get; private set; }

  [field: SerializeField, Range(0f, 1f)]
  public float AttackChainingInputBuffer { get; private set; }

  [field: Space(10f)]

  public List<AnimationClip> AttackAnimationClips = new();

  [field: Space(10f)]

  [field: SerializeField, Expandable]
  public CameraShakeSettingsSO CameraShakeSettings { get; private set; }

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
}
