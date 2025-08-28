using stal.HSM.Contexts;
using stal.HSM.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace stal.HSM.PlayerStates
{
  public class PlayerRoot : State
  {
    public readonly Movement Movement;
    public readonly Attack Attack;
    public readonly Nero Nero;

    private readonly PlayerAttributesDataSO _playerAttributesDataSO;
    private readonly PlayerMovementDataSO _playerMovementDataSO;
    private readonly PlayerEventDataSO _playerEventDataSO;
    private readonly PlayerAbilityDataSO _playerAbilityDataSO;
    private readonly PlayerContext _playerContext;

    public PlayerRoot(HierarchicalStateMachine stateMachine, PlayerContext playerContext, HSMScratchpadSO scratchpad) : base(stateMachine, null)
    {
      _playerAttributesDataSO = scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
      _playerMovementDataSO = scratchpad.GetScratchpadData<PlayerMovementDataSO>();
      _playerEventDataSO = scratchpad.GetScratchpadData<PlayerEventDataSO>();
      _playerAbilityDataSO = scratchpad.GetScratchpadData<PlayerAbilityDataSO>();
      _playerContext = playerContext;

      // Child States
      Movement = new(stateMachine, this, playerContext, scratchpad);
      Attack = new(stateMachine, this, playerContext, scratchpad);
      Nero = new(stateMachine, this, playerContext, scratchpad);

    }

    protected override State GetInitialState() => Movement;

    protected override void OnEnter()
    {
      _playerEventDataSO.Attack.OnEventRaised += OnAttack;
      _playerEventDataSO.Environment.OnEventRaised += OnEnvironment;
    }

    protected override void OnExit()
    {
      _playerEventDataSO.Attack.OnEventRaised -= OnAttack;
      _playerEventDataSO.Environment.OnEventRaised -= OnEnvironment;
    }

    protected override void OnUpdate(float deltaTime)
    {
      if (_playerAttributesDataSO.IsTouchingWall
        && _playerAttributesDataSO.IsTakingAim
        && !_playerAttributesDataSO.IsLatchedOntoWall
        && _playerAbilityDataSO.CurrentlyEquippedArmType == NeroArmType.Claw
      )
      {
        _playerAttributesDataSO.UpdateIsLatchedOntoWall(true);
        StateMachine.Sequencer.RequestTransition(this, Nero);
      }

      // We need to decelerate the player back to zero no matter what state we're in because
      // otherwise we slide infinitely with no friction outside of the Movement state. 
      // The deceleration is the friction bringing our linear velocity back to zero.
      if (ActiveChild != Movement)
      {
        float accelRate;

        if (_playerAttributesDataSO.IsGrounded)
        {
          accelRate = _playerMovementDataSO.RunDecelerationAmount;
        }
        else
        {
          accelRate = _playerMovementDataSO.RunDecelerationAmount * _playerMovementDataSO.DecelerationAirMultiplier;
        }

        float delta = 0 - _playerContext.rigidbody2D.linearVelocityX;

        float force = delta * accelRate;

        // Multiplying by Vector2.right is a quick way to convert the calculation into a vector
        _playerContext.rigidbody2D.AddForce(force * Time.fixedDeltaTime * Vector2.right, ForceMode2D.Force);

        ClampPlayerMovement();
      }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
      // If our arm has a combat ability and we are grounded we should immediatly enter the attack state
      if (context.started
        && _playerAbilityDataSO.CurrentlyEquippedArm != null
        && _playerAbilityDataSO.CurrentlyEquippedArm.CombatAbility != null
        && _playerAttributesDataSO.IsGrounded
        && !_playerAttributesDataSO.IsAttacking)
      {
        _playerAttributesDataSO.UpdateIsAttacking(true);
        StateMachine.Sequencer.RequestTransition(this, Attack);
      }
    }

    private void OnEnvironment(InputAction.CallbackContext context)
    {
      if (context.started)
      {

        if (_playerAbilityDataSO.CurrentlyEquippedArmType == NeroArmType.Needle
          && !_playerAttributesDataSO.IsNeedling
          && !_playerAttributesDataSO.IsAttacking
          && PlayerFoundAnchorPointForNeedle())
        {
          _playerAttributesDataSO.UpdateIsNeedling(true);
          StateMachine.Sequencer.RequestTransition(this, Nero);
        }
      }
    }

    private bool PlayerFoundAnchorPointForNeedle()
    {
      bool result = false;
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
          result = true;
        }
      }

      return result;
    }

    private void ClampPlayerMovement()
    {
      // Clamps velocity by the amount configured in _playerMovementDataSO
      if (_playerMovementDataSO.DoClampHorizontalVelocity)
      {
        _playerContext.rigidbody2D.linearVelocityX = Mathf.Clamp(_playerContext.rigidbody2D.linearVelocityX, -_playerMovementDataSO.VelocityHorizontalClamp, _playerMovementDataSO.VelocityHorizontalClamp);
      }
      if (_playerMovementDataSO.DoClampVerticalVelocity)
      {
        _playerContext.rigidbody2D.linearVelocityY = Mathf.Clamp(_playerContext.rigidbody2D.linearVelocityY, -_playerMovementDataSO.VelocityVerticalClamp, _playerMovementDataSO.VelocityVerticalClamp);
      }
    }
  }
}
