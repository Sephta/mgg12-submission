using DG.Tweening;
using stal.HSM.Contexts;
using stal.HSM.Core;
using UnityEngine;
using UnityEngine.Splines;

namespace stal.HSM.PlayerStates
{
  public class NeroNeedle : NeroSubstate
  {
    private readonly PlayerAttributesDataSO _playerAttributesDataSO;
    private readonly PlayerMovementDataSO _playerMovementDataSO;
    private readonly PlayerEventDataSO _playerEventDataSO;
    private readonly PlayerAbilityDataSO _playerAbilityDataSO;
    private readonly PlayerContext _playerContext;

    private Vector3 _pointToTravelTo;
    private bool _doTravel = false;

    public NeroNeedle(HierarchicalStateMachine stateMachine, State parent, PlayerContext playerContext, HSMScratchpadSO scratchpad) : base(stateMachine, parent)
    {
      _playerAttributesDataSO = scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
      _playerMovementDataSO = scratchpad.GetScratchpadData<PlayerMovementDataSO>();
      _playerEventDataSO = scratchpad.GetScratchpadData<PlayerEventDataSO>();
      _playerAbilityDataSO = scratchpad.GetScratchpadData<PlayerAbilityDataSO>();
      _playerContext = playerContext;
    }


    protected override void OnEnter()
    {
      _playerContext.rigidbody2D.gravityScale = 0f;
      SetPointToTravelTo();

      if (_pointToTravelTo != null && _pointToTravelTo != Vector3.zero && _doTravel)
      {
        _playerContext.transform.DOMove(_pointToTravelTo, 0.5f)
          .SetLink(_playerContext.transform.gameObject)
          .OnComplete(() =>
          {
            NeedleTweenOnComplete();
          });
      }
      else
      {
        NeedleTweenOnComplete();
      }
    }

    protected override void OnExit()
    {
      _playerContext.rigidbody2D.gravityScale = _playerMovementDataSO.GravityScale;
      _pointToTravelTo = Vector3.zero;
      _doTravel = false;
    }

    // protected override void OnUpdate(float deltaTime) {}

    private void SetPointToTravelTo()
    {
      Vector2 rayDirection = Vector2.zero;
      if (_playerAttributesDataSO.PlayerMoveDirection != Vector2.zero)
      {
        if (Mathf.Abs(_playerAttributesDataSO.PlayerMoveDirection.x) > Mathf.Abs(_playerAttributesDataSO.PlayerMoveDirection.y))
        {
          rayDirection.x = Mathf.Sign(_playerAttributesDataSO.PlayerMoveDirection.x) >= 0 ? 1f : -1f;
        }
        else
        {
          rayDirection.y = Mathf.Sign(_playerAttributesDataSO.PlayerMoveDirection.y) >= 0 ? 1f : 0f;
        }
      }

      if (rayDirection != Vector2.zero)
      {
        _doTravel = true;
        // fire ray
        RaycastHit2D aimRaycast = Physics2D.Raycast(
          _playerContext.transform.position + (Vector3.up * 0.5f),
          rayDirection,
          _playerMovementDataSO.AbilityAimRaycastDistance,
          _playerMovementDataSO.LayersConsideredForGroundingPlayer
        );

        // check if we hit something
        if (aimRaycast)
        {
          _pointToTravelTo = new Vector3(aimRaycast.point.x, aimRaycast.point.y, 0f);
          _pointToTravelTo -= Vector3.up * 0.5f;
        }
      }
    }

    private void NeedleTweenOnComplete()
    {
      _playerAttributesDataSO.UpdateIsNeedling(false);
    }

    // private void CreateNewSpline()
    // {
    //   if (_playerContext.splineContainer == null) return;

    //   if (_playerContext.splineContainer.Splines.Count == 0)
    //   {
    //     Spline newSpline = new();
    //     _playerContext.splineContainer.AddSpline(newSpline);
    //   }

    //   GenerateKnotsAlongSpline();
    // }

    // private void GenerateKnotsAlongSpline()
    // {
    //   if (_playerContext.splineContainer == null) return;
    //   if (_playerContext.splineContainer.Spline == null || _playerContext.splineContainer.Splines.Count == 0) return;

    //   var initialPosition = Vector3.zero;

    //   BezierKnot startintKnot = new()
    //   {
    //     Position = initialPosition,
    //     Rotation = _playerContext.transform.rotation
    //   };

    //   _playerContext.splineContainer.Spline.Add(startintKnot, TangentMode.AutoSmooth);

    //   BezierKnot endingKnot = new()
    //   {
    //     Position = initialPosition,
    //     Rotation = _playerContext.transform.rotation
    //   };

    //   _playerContext.splineContainer.Spline.Add(endingKnot, TangentMode.AutoSmooth);

    //   // for (var i = 0; i < 5; i++)
    //   // {
    //   //   BezierKnot newKnot = new()
    //   //   {
    //   //     Position = initialPosition,
    //   //     Rotation = _playerContext.transform.rotation
    //   //   };

    //   //   _playerContext.splineContainer.Spline.Add(newKnot, TangentMode.AutoSmooth);

    //   //   Vector3 positionOffset = Vector3.up * 1f;
    //   //   // positionOffset.x = Random.Range(_brambleSpawnParametersSO.KnotVariance.x, _brambleSpawnParametersSO.KnotVariance.y);
    //   //   initialPosition += positionOffset;
    //   // }
    // }
  }
}