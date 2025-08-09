using Unity.Cinemachine;
using UnityEngine;

public class AssignPlayerToCamera : MonoBehaviour
{
  [SerializeField] private CinemachineCamera _cinemachineCamera;
  [SerializeField, ReadOnly] private Transform _playerTransform;

  private void Awake()
  {
    if (_cinemachineCamera == null) gameObject.GetComponentInChildren<CinemachineCamera>();

    GameObject[] playerTaggedObjects = GameObject.FindGameObjectsWithTag("Player");
    if (playerTaggedObjects.Length > 0)
    {
      _playerTransform = playerTaggedObjects[0].transform;
    }

    if (_playerTransform != null)
    {
      _cinemachineCamera.Target.TrackingTarget = _playerTransform;
    }
  }
}
