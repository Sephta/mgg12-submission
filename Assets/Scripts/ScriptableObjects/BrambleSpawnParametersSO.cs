using UnityEngine;

[CreateAssetMenu(fileName = "BrambleSpawnParametersSO", menuName = "Scriptable Objects/Bramble Spawn Parameters")]
public class BrambleSpawnParametersSO : ScriptableObject
{
  /* ---------------------------------------------------------------- */
  /*                            Spline Data                           */
  /* ---------------------------------------------------------------- */
  [field: SerializeField, Range(0f, 15f), Tooltip("Determines how many knots appear along the spline.")]
  public int NumberOfKnots { get; private set; }
}
