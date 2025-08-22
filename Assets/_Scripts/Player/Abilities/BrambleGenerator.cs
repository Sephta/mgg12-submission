using System.Collections.Generic;
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
    if (_brambleComponents.Count > 0) DestroyBrambleAlongSplineKnots();

    foreach (Spline spline in _splineContainer.Splines)
    {
      foreach (BezierKnot knot in spline.Knots)
      {
        if (_brambleComponent == null) return;

        Vector3 worldPositionOfKnot = transform.TransformPoint(knot.Position.ConvertTo<Vector3>());

        GameObject result = Instantiate(
          _brambleComponent,
          worldPositionOfKnot,
          transform.rotation,
          transform
        );

        result.SetActive(false);
        result.transform.localScale = Vector3.zero;

        if (result != null) _brambleComponents.Add(result);
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
        .SetEase(_brambleSpawnParametersSO.EaseType)
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
        .SetEase(_brambleSpawnParametersSO.EaseType)
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
