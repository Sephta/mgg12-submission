using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class BrambleGenerator : MonoBehaviour
{
  [Header("Bramble Generator Components")]

  [SerializeField]
  private SplineContainer _splineContainer;

  [SerializeField, Expandable]
  private BrambleSpawnParametersSO _brambleSpawnParametersSO;

  [SerializeField]
  private GameObject _brambleComponent;

  [SerializeField]
  private List<GameObject> _brambleComponents;

  [Header("Kill Timer")]
  [SerializeField] private bool _keepAliveForever = false;
  [SerializeField, Range(0f, 30f)] private float _aliveTime;
  [SerializeField, ReadOnly] private float _timeLeftAlive;

  [SerializeField] private LayerMask _layersForCollisionCheck;
  [SerializeField, Range(0f, 2f)] private float _collisionDetectionRadius = 1f;

  [Header("Debug")]
  [SerializeField, ReadOnly]
  private bool _isTweening = false;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  void Awake()
  {
    if (_splineContainer == null) _splineContainer = GetComponent<SplineContainer>();

    if (_brambleSpawnParametersSO == null)
    {
      Debug.LogError("_brambleSpawnParametersSO is null for game object: " + name);
    }

    _isTweening = false;
  }

  void Start()
  {
    DestroySplinesAndBramble();

    if (_brambleComponents.Count == 0) GenerateSplineWithBramble();

    BuildGrowthSequence().Play();

    _timeLeftAlive = _aliveTime;
  }

  private void Update()
  {
    _timeLeftAlive = Mathf.Clamp(_timeLeftAlive - Time.deltaTime, 0f, _aliveTime);

    if (_timeLeftAlive == 0f && !_keepAliveForever)
    {
      DG.Tweening.Sequence decaySequence = BuildDecaySequence();
      decaySequence.Play();
    }
  }

  // private void OnDrawGizmos()
  // {
  //   foreach (Spline spline in _splineContainer.Splines)
  //   {
  //     foreach (BezierKnot knot in spline.Knots)
  //     {
  //       Vector3 worldPositionOfKnot = transform.TransformPoint(knot.Position.ConvertTo<Vector3>());

  //       Gizmos.DrawSphere(new(worldPositionOfKnot.x, worldPositionOfKnot.y, 0f), 1f);
  //     }
  //   }
  // }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  [Button("Generate Spline")]
  private void GenerateSpline()
  {
    if (_isTweening) return;

    DeleteSplines();
    CreateNewSpline();
  }

  private void CreateNewSpline()
  {
    if (_splineContainer == null) return;

    if (_splineContainer.Splines.Count == 0)
    {
      Spline newSpline = new();
      _splineContainer.AddSpline(newSpline);
    }

    GenerateKnotsAlongSpline();
  }

  private void GenerateKnotsAlongSpline()
  {
    if (_splineContainer == null) return;
    if (_splineContainer.Spline == null || _splineContainer.Splines.Count == 0) return;

    var initialPosition = Vector3.zero;
    for (var i = 0; i < _brambleSpawnParametersSO.NumberOfKnots; i++)
    {
      BezierKnot newKnot = new()
      {
        Position = initialPosition,
        Rotation = transform.rotation
      };

      _splineContainer.Spline.Add(newKnot, TangentMode.AutoSmooth);

      Vector3 positionOffset = Vector3.up * _brambleSpawnParametersSO.KnotOffset;
      positionOffset.x = Random.Range(_brambleSpawnParametersSO.KnotVariance.x, _brambleSpawnParametersSO.KnotVariance.y);
      initialPosition += positionOffset;
    }
  }

  [Button("Delete Spline Data From Container")]
  private void DeleteSplines()
  {
    if (_splineContainer == null || _isTweening) return;

    if (_splineContainer.Splines.Count > 0)
    {
      foreach (Spline spline in _splineContainer.Splines)
      {
        spline.Clear();
        _splineContainer.RemoveSpline(spline);
      }
    }
  }

  [Button("Generate Spline With Bramble")]
  private void GenerateSplineWithBramble()
  {
    if (_isTweening) return;

    GenerateSpline();
    InstantiateBrambleAlongSplineKnots();

    BuildGrowthSequence().Play();
  }

  private void InstantiateBrambleAlongSplineKnots()
  {
    if (_brambleComponent == null) return;
    if (_brambleComponents.Count > 0) DestroyBrambleAlongSplineKnots();

    foreach (Spline spline in _splineContainer.Splines)
    {
      foreach (BezierKnot knot in spline.Knots)
      {

        Vector3 worldPositionOfKnot = transform.TransformPoint(knot.Position.ConvertTo<Vector3>());

        Collider2D[] collidersOverlappingCircle = Physics2D.OverlapCircleAll(
          new(worldPositionOfKnot.x, worldPositionOfKnot.y),
          1f,
          _brambleSpawnParametersSO.StuffToWatchOutForWhenSpawning
        );

        if (collidersOverlappingCircle.Length <= 0)
        {
          float randomEularRotationZ = Random.Range(_brambleSpawnParametersSO.RandomRotationRange.x, _brambleSpawnParametersSO.RandomRotationRange.y);

          GameObject result = Instantiate(
            _brambleComponent,
            worldPositionOfKnot,
            transform.rotation,
            transform
          );

          RaycastHit2D circleCastHit = Physics2D.CircleCast(
            (Vector2)worldPositionOfKnot,
            _collisionDetectionRadius,
            Vector2.up,
            0f,
            _layersForCollisionCheck
          );

          // We dont want to grow where the player is. The layer mask should contain the player layer.
          if (circleCastHit) return;

          if (result != null)
          {
            SetRandomPlantSprite resultVisuals = result.GetComponentInChildren<SetRandomPlantSprite>();
            if (resultVisuals != null)
            {
              resultVisuals.transform.rotation = Quaternion.Euler(0f, 0f, randomEularRotationZ);
            }

            result.SetActive(false);
            result.transform.localScale = Vector3.zero;
            _brambleComponents.Add(result);
          }
        }
        else
        {
          Debug.Log("Player colliders found when generating bramble.");
          return;
        }
      }
    }
  }

  [Button("Destroy Splines and Bramble")]
  private void DestroySplinesAndBramble()
  {
    if (_isTweening) return;

    DeleteSplines();
    DestroyBrambleAlongSplineKnots();
  }

  private void DestroyBrambleAlongSplineKnots()
  {
    foreach (GameObject bramble in _brambleComponents)
    {
      DestroyImmediate(bramble);
    }

    _brambleComponents.Clear();
  }

  [Button("Activate Bramble")]
  private void ActivateBramble()
  {
    if (_isTweening) return;
    if (_brambleComponents.Count == 0) return;

    DG.Tweening.Sequence brambleSequence = BuildGrowthSequence();
    brambleSequence.Play();
  }

  [Button("Deactivate Bramble")]
  private void DeactivateBramble()
  {
    if (_isTweening) return;
    if (_brambleComponents.Count == 0) return;

    DG.Tweening.Sequence brambleSequence = BuildDecaySequence();
    brambleSequence.Play();
  }

  private DG.Tweening.Sequence BuildGrowthSequence()
  {
    DG.Tweening.Sequence brambleSequence = DOTween.Sequence();

    foreach (GameObject bramble in _brambleComponents)
    {
      bramble.SetActive(true);
      brambleSequence.Append(
        bramble.transform.DOScale(1f, _brambleSpawnParametersSO.GrowthRate / _brambleComponents.Count)
        .SetEase(_brambleSpawnParametersSO.BrambleEaseType)
      );
    }

    brambleSequence.OnStart(() => GrowthSequenceOnStart());
    brambleSequence.OnComplete(() =>
    {
      GrowthSequenceOnComplete();
      brambleSequence.Kill();
    });

    brambleSequence.SetLink(gameObject);

    return brambleSequence;
  }

  private void GrowthSequenceOnStart()
  {
    _isTweening = true;
  }

  private void GrowthSequenceOnComplete()
  {
    _isTweening = false;
  }

  private DG.Tweening.Sequence BuildDecaySequence()
  {
    DG.Tweening.Sequence brambleSequence = DOTween.Sequence();

    foreach (GameObject bramble in _brambleComponents)
    {
      brambleSequence.Append(
        bramble.transform.DOScale(0f, _brambleSpawnParametersSO.GrowthRate / _brambleComponents.Count)
        .SetEase(_brambleSpawnParametersSO.BrambleEaseType)
      );
    }

    brambleSequence.OnStart(() => DecaySequenceOnStart());
    brambleSequence.OnComplete(() =>
    {
      DecaySequenceOnComplete();
      brambleSequence.Kill();
    });

    brambleSequence.SetLink(gameObject);

    return brambleSequence;
  }

  private void DecaySequenceOnStart()
  {
    _isTweening = true;
  }

  private void DecaySequenceOnComplete()
  {
    foreach (GameObject bramble in _brambleComponents)
    {
      bramble.SetActive(false);
    }

    _isTweening = false;

    Destroy(gameObject);
  }
}
