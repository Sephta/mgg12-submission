using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraShake : MonoBehaviour
{
  [SerializeField] private CinemachineCamera _camera = null;
  [SerializeField] private FloatFloatEventChannelSO _cameraShakeEvent;
  [SerializeField] private float shakeIntensity = 0f;
  [SerializeField] private float shakeTimer = 0f;
  [SerializeField, ReadOnly] private float currShakeTime = 0f;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_camera == null) _camera = GetComponent<CinemachineCamera>();
  }

  private void OnEnable()
  {
    if (_cameraShakeEvent == null) return;

    _cameraShakeEvent.OnEventRaised += ShakeCamera;
  }

  private void OnDisable()
  {
    if (_cameraShakeEvent == null) return;

    _cameraShakeEvent.OnEventRaised -= ShakeCamera;
  }

  private void Update()
  {
    if (currShakeTime >= 0)
    {
      currShakeTime = Mathf.Clamp(currShakeTime - Time.deltaTime, 0f, shakeTimer);

      if (currShakeTime <= 0f)
      {
        CinemachineBasicMultiChannelPerlin mChannelPerlin =
          _camera.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();

        mChannelPerlin.AmplitudeGain = 0f;
      }
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */
  [Button("Shake Camera")]
  public void ShakeCamera()
  {
    CinemachineBasicMultiChannelPerlin mChannelPerlin =
      _camera.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();

    mChannelPerlin.AmplitudeGain = shakeIntensity;
    currShakeTime = shakeTimer;
  }

  public void ShakeCamera(float intensity, float duration)
  {
    CinemachineBasicMultiChannelPerlin mChannelPerlin =
      _camera.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();

    mChannelPerlin.AmplitudeGain = intensity;
    currShakeTime = duration;
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
}
