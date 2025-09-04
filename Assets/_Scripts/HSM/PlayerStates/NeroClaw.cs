using stal.HSM.Contexts;
using stal.HSM.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace stal.HSM.PlayerStates
{
  public class NeroClaw : NeroSubstate
  {
    private readonly PlayerAttributesDataSO _playerAttributesDataSO;
    private readonly PlayerMovementDataSO _playerMovementDataSO;
    private readonly PlayerEventDataSO _playerEventDataSO;
    private readonly PlayerAbilityDataSO _playerAbilityDataSO;
    private readonly PlayerContext _playerContext;

    private float _currentTimeTillSlideDownWall = 0f;
    private readonly float _wallJumpBaseForce = 1000f;
    private bool _jumpPressed = false;

    public NeroClaw(HierarchicalStateMachine stateMachine, State parent, PlayerContext playerContext, HSMScratchpadSO scratchpad) : base(stateMachine, parent)
    {
      _playerAttributesDataSO = scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
      _playerMovementDataSO = scratchpad.GetScratchpadData<PlayerMovementDataSO>();
      _playerEventDataSO = scratchpad.GetScratchpadData<PlayerEventDataSO>();
      _playerAbilityDataSO = scratchpad.GetScratchpadData<PlayerAbilityDataSO>();
      _playerContext = playerContext;
    }

    ~NeroClaw()
    {
      OnExit();
    }

    protected override void OnEnter()
    {
      _jumpPressed = false;
      _playerEventDataSO.Jump.OnEventRaised += OnJump;

      UpdateGravityScale(0f);
      _playerContext.rigidbody2D.linearVelocityY = 0f;
      _currentTimeTillSlideDownWall = _playerMovementDataSO.TimeTillSlideDownWall;
    }

    protected override void OnExit()
    {
      _playerEventDataSO.Jump.OnEventRaised -= OnJump;
      UpdateGravityScale(_playerMovementDataSO.GravityScale);
      _playerContext.jumpEndEarly = false;
      Vector2 wallDirection = GetWallDirection();

      if (wallDirection != Vector2.zero && _jumpPressed)
      {
        _jumpPressed = false;
        _playerContext.rigidbody2D.linearVelocityX = _playerMovementDataSO.WallJumpHorizontalForce * _wallJumpBaseForce * Time.fixedDeltaTime * -wallDirection.x;
        _playerContext.rigidbody2D.linearVelocityY = _playerMovementDataSO.WallJumpVerticalForce * _wallJumpBaseForce * Time.fixedDeltaTime;
      }

      _currentTimeTillSlideDownWall = 0f;

      _playerAttributesDataSO.UpdateIsLatchedOntoWall(false);
    }

    protected override void OnUpdate(float deltaTime)
    {
      _currentTimeTillSlideDownWall = Mathf.Clamp(_currentTimeTillSlideDownWall - deltaTime, 0f, _playerMovementDataSO.TimeTillSlideDownWall);

      if (_jumpPressed)
      {
        _playerAttributesDataSO.UpdateIsLatchedOntoWall(false);
        _playerAttributesDataSO.UpdateIsTouchingWall(false);
        _playerAttributesDataSO.UpdateIsTakingAim(false);
      }

      if (_currentTimeTillSlideDownWall <= 0f)
      {
        UpdateGravityScale(_playerMovementDataSO.SlideDownWallGravityMultiplier);
      }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
      if (context.performed)
      {
        _jumpPressed = true;
      }
    }

    private void ResetGravityScale() => _playerContext.rigidbody2D.gravityScale = _playerMovementDataSO.GravityScale;
    private void UpdateGravityScale(float scale) => _playerContext.rigidbody2D.gravityScale = _playerMovementDataSO.GravityScale * scale;

    private Vector2 GetWallDirection()
    {
      Vector2 wallDirection = Vector2.zero;

      RaycastHit2D rayCastLeft = Physics2D.Raycast(
        _playerContext.transform.position + (Vector3.up * 0.5f),
        Vector2.left,
        1f,
        _playerMovementDataSO.LayersConsideredForPlayerTouchingWall
      );

      RaycastHit2D rayCastRight = Physics2D.Raycast(
        _playerContext.transform.position + (Vector3.up * 0.5f),
        Vector2.right,
        1f,
        _playerMovementDataSO.LayersConsideredForPlayerTouchingWall
      );

      if (rayCastLeft) wallDirection = Vector2.left;
      if (rayCastRight) wallDirection = Vector2.right;

      return wallDirection;
    }
  }
}