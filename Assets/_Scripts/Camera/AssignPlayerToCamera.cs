using stal.HSM.Drivers;
using Unity.Cinemachine;
using UnityEngine;

public class AssignPlayerToCamera : MonoBehaviour
{
  [SerializeField] private CinemachineCamera _cinemachineCamera;
  [SerializeField] private TransformEventChannelSO _assignPlayerToCameraEvent;
  [SerializeField] private VoidEventChannelSO _playerDeathEvent;
  [SerializeField, ReadOnly] private Transform _playerTransform = null;

  private void Awake()
  {
    if (_cinemachineCamera == null) gameObject.GetComponentInChildren<CinemachineCamera>();

    AssignToCameraOnAwake();
  }

  private void OnEnable()
  {
    if (_assignPlayerToCameraEvent == null) return;

    _assignPlayerToCameraEvent.OnEventRaised += OnAssignPlayerToCamera;

    if (_playerDeathEvent == null) return;

    _playerDeathEvent.OnEventRaised += OnPlayerDeath;
  }

  private void OnDisable()
  {
    if (_assignPlayerToCameraEvent == null) return;

    _assignPlayerToCameraEvent.OnEventRaised -= OnAssignPlayerToCamera;

    if (_playerDeathEvent == null) return;

    _playerDeathEvent.OnEventRaised -= OnPlayerDeath;
  }

  private void AssignToCameraOnAwake()
  {
    _playerTransform = null;

    PlayerHSMDriver[] playerHSMDrivers = FindObjectsByType<PlayerHSMDriver>(FindObjectsSortMode.None);
    if (playerHSMDrivers.Length > 0)
    {
      _playerTransform = playerHSMDrivers[0].transform;
    }

    if (_playerTransform != null)
    {
      _cinemachineCamera.Target.TrackingTarget = _playerTransform;
    }
  }

  private void OnAssignPlayerToCamera(Transform playerTransform)
  {
    if (playerTransform != null)
    {
      _cinemachineCamera.Target.TrackingTarget = playerTransform;
    }
  }

  private void OnPlayerDeath()
  {
    _playerTransform = null;
    _cinemachineCamera.Target.TrackingTarget = null;
  }
}
