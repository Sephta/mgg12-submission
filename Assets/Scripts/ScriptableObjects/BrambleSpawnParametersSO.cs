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

  [field: SerializeField, MinMaxSlider(-1f, 1f), Tooltip("Offsets a given knot left or right by ")]
  public Vector2 KnotVariance { get; private set; }

  [field: SerializeField, Range(0.05f, 1f), Tooltip("Offsets a given knot left or right by ")]
  public float GrowthRate { get; private set; }

  [field: Header("Runtime Data")]

  [field: SerializeField, ReadOnly, Tooltip("Determines how many knots appear along the spline.")]
  public Vector3 SurfaceNormal { get; private set; }
}
