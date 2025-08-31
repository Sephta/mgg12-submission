using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "BrambleSpawnParametersSO", menuName = "Scriptable Objects/Bramble Spawn Parameters")]
public class BrambleSpawnParametersSO : ScriptableObject
{
  /* ---------------------------------------------------------------- */
  /*                            Spline Data                           */
  /* ---------------------------------------------------------------- */

  [field: Header("Bramble Spawnpoint Data")]

  [field: SerializeField, Range(0f, 50f), Tooltip("Determines how many knots appear along the spline.")]
  public int NumberOfKnots { get; private set; }

  [field: SerializeField, Range(0f, 1f), Tooltip("Determines the distance between each knot.")]
  public float KnotOffset { get; private set; }

  [field: SerializeField, MinMaxSlider(-1f, 1f)]
  public Vector2 KnotVariance { get; private set; }

  [field: SerializeField, Range(0.05f, 1f), Tooltip("Growth rate is computed by taking the value given in this field and dividing it by the number of knots along the spline.")]
  public float GrowthRate { get; private set; }

  [field: SerializeField, MinMaxSlider(-180f, 180f), Tooltip("Used for adding variance to the rotation of the bamble segment sprite")]
  public Vector2 RandomRotationRange { get; private set; }

  [field: SerializeField, Tooltip("The layer to use when checking if the player is grounded.")]
  public LayerMask StuffToWatchOutForWhenSpawning { get; private set; }

  [field: Header("DOTween Settings")]

  [field: SerializeField, Tooltip("The ease of the animation.")]
  public Ease BrambleEaseType { get; private set; }
}
