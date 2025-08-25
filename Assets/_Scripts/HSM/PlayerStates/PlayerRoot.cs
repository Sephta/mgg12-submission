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
    }

    protected override void OnExit()
    {
      _playerEventDataSO.Attack.OnEventRaised -= OnAttack;
    }

    protected override void OnUpdate(float deltaTime)
    {
      // We need to decelerate the player back to zero no matter what state we're in because
      // otherwise we slide infinitely with no friction outside of the Movement state. 
      // The deceleration is the friction bringing our linear velocity back to zero.
      if (ActiveChild != Movement)
      {
        float delta = 0 - _playerContext.rigidbody2D.linearVelocityX;

        float force = delta * _playerMovementDataSO.RunDecelerationAmount;

        // Multiplying by Vector2.right is a quick way to convert the calculation into a vector
        _playerContext.rigidbody2D.AddForce(force * Time.fixedDeltaTime * Vector2.right, ForceMode2D.Force);
      }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
      // If our arm has a combat ability and we are grounded we should immediatly enter the attack state
      if (_playerAbilityDataSO.CurrentlyEquippedArm != null
        && _playerAbilityDataSO.CurrentlyEquippedArm.CombatAbility != null
        && _playerAttributesDataSO.IsGrounded)
      {
        if (context.started && !_playerAttributesDataSO.IsAttacking) _playerAttributesDataSO.UpdateIsAttacking(true);
        StateMachine.Sequencer.RequestTransition(this, Attack);
      }
    }

  }
}
