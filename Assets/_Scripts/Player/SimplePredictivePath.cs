using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class SimplePredictivePath : MonoBehaviour
{

  [Required("Must provide a HSMScratchpadSO asset.")]
  [SerializeField, Expandable] private HSMScratchpadSO _scratchpad;
  private PlayerMovementDataSO _playerMovementData;
  private PlayerAttributesDataSO _playerAttributesData;
  private PlayerAbilityDataSO _playerAbilityData;
  private PlayerEventDataSO _playerEventData;

  [Space(10f)]
  [Header("Predictive Path Data")]
  [SerializeField] private LayerMask _collisionCheck;
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
    if (_scratchpad == null)
    {
      Debug.LogError(name + " does not have a HSMScratchpadSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _playerMovementData = _scratchpad.GetScratchpadData<PlayerMovementDataSO>();
    if (_playerMovementData == null)
    {
      Debug.LogError(name + " does not have a PlayerMovementDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _playerAttributesData = _scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
    if (_playerAttributesData == null)
    {
      Debug.LogError(name + " does not have a PlayerAttributesDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _playerAbilityData = _scratchpad.GetScratchpadData<PlayerAbilityDataSO>();
    if (_playerAbilityData == null)
    {
      Debug.LogError(name + " does not have a PlayerAbilityDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_playerAbilityData.ArmData.Count <= 0)
    {
      Debug.LogError(name + " contains empty PlayerAbilityDataSO.ArmData. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _playerEventData = _scratchpad.GetScratchpadData<PlayerEventDataSO>();
    if (_playerEventData == null)
    {
      Debug.LogError(name + " does not have a PlayerEventDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }
  }

  private void Start()
  {
    InitializePoints();
  }

  private void Update()
  {
    if (_playerAttributesData.IsTakingAim
      && _playerAttributesData.PlayerAimDirection != Vector2.zero
      && _playerAttributesData.IsGrounded
      && _playerAbilityData.CurrentlyEquippedArmType == NeroArmType.Neutral)
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

    return (Vector2)pointPosition + force * t * _playerAttributesData.PlayerAimDirection + t * t * _pointsGravityScale * Physics2D.gravity;
  }

  private void UpdatePointPositions(float force)
  {
    bool oneOfThePointsCollidedWithSomething = false;

    for (int i = 0; i < _numPoints; i++)
    {
      Vector2 newPointPosition = PointPosition((i + 1) * _pointSpacingValue, force);

      if (!oneOfThePointsCollidedWithSomething)
      {
        RaycastHit2D circleCastHit = Physics2D.CircleCast(
          newPointPosition,
          0.1f,
          Vector2.up,
          0f,
          _collisionCheck
        );

        oneOfThePointsCollidedWithSomething = circleCastHit;
      }

      if (Mathf.Round(Vector2.Distance((Vector2)transform.position, newPointPosition)) <= _playerMovementData.AbilityAimRaycastDistance && !oneOfThePointsCollidedWithSomething)
      {
        _points[i].transform.position = newPointPosition;
        _points[i].SetActive(true);
      }
      else
      {
        _points[i].SetActive(false);
      }
    }
  }
}
