using stal.HSM.Contexts;
using stal.HSM.Core;
using UnityEngine;

namespace stal.HSM.PlayerStates
{
  public class NeroNeutral : NeroSubstate
  {
    private readonly PlayerAttributesDataSO _playerAttributesDataSO;
    private readonly PlayerMovementDataSO _playerMovementDataSO;
    private readonly PlayerContext _playerContext;

    private readonly float _brambleDelayTime = 0.25f;
    private float _brambleDelayTimer = 0f;

    public NeroNeutral(HierarchicalStateMachine stateMachine, State parent, PlayerContext playerContext, HSMScratchpadSO scratchpad) : base(stateMachine, parent)
    {
      _playerAttributesDataSO = scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
      _playerMovementDataSO = scratchpad.GetScratchpadData<PlayerMovementDataSO>();
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
        _playerAttributesDataSO.PlayerAimDirection,
        _playerMovementDataSO.AbilityAimRaycastDistance,
        _playerMovementDataSO.LayersConsideredForGroundingPlayer
      );

      // check if we hit something
      if (aimRaycast)
      {
        if (_playerAttributesDataSO.IsConfirmingAim && _brambleDelayTimer == 0f)
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