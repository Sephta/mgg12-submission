using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraShake : MonoBehaviour
{
  [SerializeField] private CinemachineCamera _camera = null;
  [SerializeField] private CinemachineBasicMultiChannelPerlin _multiChannelPerlin;
  [SerializeField] private FloatFloatEventChannelSO _cameraShakeEvent;
  [SerializeField] private float _shakeIntensity = 0f;
  [SerializeField] private float _shakeTimer = 0f;
  [SerializeField, ReadOnly] private float _currentShakeTime = 0f;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_camera == null) _camera = GetComponent<CinemachineCamera>();
  }

  private void OnEnable()
  {
    if (_camera != null)
    {
      _multiChannelPerlin = _camera.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

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
    if (_currentShakeTime >= 0)
    {
      _currentShakeTime = Mathf.Clamp(_currentShakeTime - Time.deltaTime, 0f, _shakeTimer);

      if (_currentShakeTime <= 0f && _multiChannelPerlin != null)
      {
        _multiChannelPerlin.AmplitudeGain = 0f;
      }
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */
  [Button("Shake Camera")]
  public void ShakeCameraNoParams()
  {
    if (_multiChannelPerlin == null) return;

    _multiChannelPerlin.AmplitudeGain = _shakeIntensity;
    _currentShakeTime = _shakeTimer;
  }

  public void ShakeCamera(float intensity, float duration)
  {
    if (_multiChannelPerlin == null) return;
    Debug.Log("ShakeCamera <" + intensity + ": " + duration + ">");

    _multiChannelPerlin.AmplitudeGain = intensity;
    _shakeTimer = duration;
    _shakeIntensity = intensity;
    _currentShakeTime = duration;
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  [Button("Reset")]
  private void ResetCameraShake()
  {
    _shakeIntensity = 0f;
    _shakeTimer = 0f;
    _currentShakeTime = 0f;

    if (_multiChannelPerlin == null) return;
    _multiChannelPerlin.AmplitudeGain = _shakeIntensity;
  }
}
