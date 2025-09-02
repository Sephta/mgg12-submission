using UnityEngine;

[CreateAssetMenu(fileName = "New Camera Shake Settings", menuName = "Scriptable Objects/CameraShakeSettingsSO")]
public class CameraShakeSettingsSO : ScriptableObject
{

  [field: SerializeField, Range(0f, 10f)]
  public float Intensity { get; private set; }

  [field: SerializeField, Range(0f, 1f)]
  public float Duration { get; private set; }

}