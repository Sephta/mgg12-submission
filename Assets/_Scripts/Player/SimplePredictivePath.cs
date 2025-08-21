using System.Collections.Generic;
using UnityEngine;

public class SimplePredictivePath : MonoBehaviour
{

  [SerializeField] private PlayerMovementDataSO _playerMovementDataSO;

  [Space(10f)]
  [Header("Predictive Path Data")]
  [SerializeField, Range(0f, 50f)] private int _numPoints = 0;
  [SerializeField] private float _launchForce = 0f;
  [SerializeField] private float _pointSpacingValue = 0f;
  [SerializeField] private float _pointsGravityScale = 0.5f;
  [SerializeField] private GameObject _pointPrefab = null;
  [SerializeField] private Transform _pointParent = null;
  [SerializeField, ReadOnly] private List<GameObject> _points = new();


  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_playerMovementDataSO == null)
    {
      Debug.LogError(name + " does not have defined " + _playerMovementDataSO.GetType().Name + ".  Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }
  }

  private void Start()
  {
    InitializePoints();
  }

  private void Update()
  {
    if (_playerMovementDataSO.IsTakingAim && _playerMovementDataSO.PlayerAimDirection != Vector2.zero && _playerMovementDataSO.IsGrounded)
    {
      UpdatePointPositions(_launchForce);
    }
    else
    {
      ResetPoints();
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private void InitializePoints()
  {
    if (_pointPrefab == null) return;

    for (int i = 0; i < _numPoints; i++)
    {
      GameObject newPoint;

      if (_pointParent)
      {
        newPoint = Instantiate(_pointPrefab, _pointParent.position, Quaternion.identity, _pointParent);
      }
      else
      {
        newPoint = Instantiate(_pointPrefab, transform.position, Quaternion.identity);
      }

      if (newPoint)
      {
        newPoint.SetActive(false);
        _points.Add(newPoint);
      }
    }
  }

  private void ResetPoints()
  {
    if (_pointPrefab == null) return;

    for (int i = 0; i < _numPoints; i++)
    {
      _points[i].transform.localPosition = Vector3.zero;
      _points[i].SetActive(false);
    }
  }

  private Vector2 PointPosition(float t, float force)
  {
    Vector3 pointPosition = _pointParent ? _pointParent.transform.position : transform.position;

    return (Vector2)pointPosition + force * t * _playerMovementDataSO.PlayerAimDirection + t * t * _pointsGravityScale * Physics2D.gravity;
  }

  private void UpdatePointPositions(float force)
  {
    for (int i = 0; i < _numPoints; i++)
    {
      _points[i].transform.position = PointPosition((i + 1) * _pointSpacingValue, force);
      _points[i].SetActive(true);
    }
  }
}
