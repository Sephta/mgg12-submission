using stal.HSM.Core;
using stal.HSM.Drivers;
using UnityEngine;

namespace stal.HSM.PlayerStates
{
  public class NeroNeutral : State
  {
    private readonly PlayerMovementDataSO _playerMovementDataSO;
    private readonly PlayerContext _playerContext;

    private readonly float _brambleDelayTime = 0.25f;
    private float _brambleDelayTimer = 0f;

    public NeroNeutral(HierarchicalStateMachine stateMachine, State Parent, PlayerMovementDataSO playerMovementDataSO, PlayerContext playerContext) : base(stateMachine, Parent)
    {
      _playerMovementDataSO = playerMovementDataSO;
      _playerContext = playerContext;
    }

    protected override void OnUpdate(float deltaTime)
    {
      _brambleDelayTimer = Mathf.Clamp(_brambleDelayTimer - deltaTime, 0f, _brambleDelayTime);

      CheckForBrambleSpawning();
    }

    protected override void OnExit()
    {
      _brambleDelayTimer = 0f;
    }

    private void CheckForBrambleSpawning()
    {
      // fire ray
      RaycastHit2D aimRaycast = Physics2D.Raycast(
        _playerContext.transform.position + (Vector3.up * 0.5f),
        _playerMovementDataSO.PlayerAimDirection,
        _playerMovementDataSO.AbilityAimRaycastDistance,
        _playerMovementDataSO.LayersConsideredForGroundingPlayer
      );

      // check if we hit something
      if (aimRaycast)
      {
        if (_playerContext.isConfirmingAim && _brambleDelayTimer == 0f)
        {
          _brambleDelayTimer = _brambleDelayTime;

          // Instantiate Bramble...
          GameObject result = Object.Instantiate(
            _playerContext.bramble,
            new Vector3(aimRaycast.point.x, aimRaycast.point.y, 0f),
            Quaternion.FromToRotation(Vector2.up, aimRaycast.normal)
          );
        }
      }
    }
  }

}