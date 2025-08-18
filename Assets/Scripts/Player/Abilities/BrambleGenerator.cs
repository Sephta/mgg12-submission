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
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  [Button("Generate Spline")]
  private void GenerateSpline()
  {
    DeleteSplines();
    CreateNewSpline();
  }

  private void CreateNewSpline()
  {
    if (_splineContainer == null)
    {
      Debug.Log(name + " | Spline Container is Null.");
      return;
    }

    if (_splineContainer.Splines.Count > 0)
    {
      Debug.Log(name + " | Spline count > 0");
      return;
    }

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
    if (_splineContainer == null) return;

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
    GenerateSpline();
    InstantiateBrambleAlongSplineKnots();
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
    DG.Tweening.Sequence brambleSequence = DOTween.Sequence();
    foreach (GameObject bramble in _brambleComponents)
    {
      bramble.SetActive(true);
      brambleSequence.Append(
        bramble.transform.DOScale(1f, _brambleSpawnParametersSO.GrowthRate / _brambleComponents.Count)
        .SetEase(Ease.InOutCubic)
      );
    }

    brambleSequence.OnStart(() =>
    {
      _isTweening = true;
    });

    brambleSequence.OnComplete(() =>
    {
      _isTweening = false;
    });

    brambleSequence.Play();
  }

  [Button("Deactivate Bramble")]
  private void DeactivateBramble()
  {
    if (_isTweening) return;

    if (_brambleComponents.Count == 0) return;
    DG.Tweening.Sequence brambleSequence = DOTween.Sequence();
    foreach (GameObject bramble in _brambleComponents)
    {
      brambleSequence.Append(
        bramble.transform.DOScale(0f, _brambleSpawnParametersSO.GrowthRate / _brambleComponents.Count)
        .SetEase(Ease.InOutCubic)
      );
    }

    brambleSequence.OnStart(() =>
    {
      _isTweening = true;
    });

    brambleSequence.OnComplete(() =>
    {
      foreach (GameObject bramble in _brambleComponents)
      {
        bramble.SetActive(false);
      }

      _isTweening = false;
    });



    brambleSequence.Play();
  }
}
