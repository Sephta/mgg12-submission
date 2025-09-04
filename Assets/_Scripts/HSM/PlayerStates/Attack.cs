using stal.HSM.Contexts;
using stal.HSM.Core;
using UnityEngine;

namespace stal.HSM.PlayerStates
{
  public class Attack : State
  {
    private readonly PlayerAttributesDataSO _playerAttributesDataSO;
    private readonly PlayerMovementDataSO _playerMovementDataSO;
    private readonly PlayerEventDataSO _playerEventDataSO;
    private readonly PlayerAbilityDataSO _playerAbilityDataSO;
    private readonly PlayerContext _playerContext;

    public Attack(HierarchicalStateMachine stateMachine, State parent, PlayerContext playerContext, HSMScratchpadSO scratchpad) : base(stateMachine, parent)
    {
      _playerAttributesDataSO = scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
      _playerMovementDataSO = scratchpad.GetScratchpadData<PlayerMovementDataSO>();
      _playerEventDataSO = scratchpad.GetScratchpadData<PlayerEventDataSO>();
      _playerAbilityDataSO = scratchpad.GetScratchpadData<PlayerAbilityDataSO>();
      _playerContext = playerContext;
    }

    ~Attack()
    {
      OnExit();
    }

    protected override void OnEnter()
    {
      _playerEventDataSO.AttackChainCompleted.OnEventRaised += RequestTransitionOutOfAttackState;

      if (_playerAbilityDataSO.CurrentlyEquippedArm != null && _playerAbilityDataSO.CurrentlyEquippedArmType == NeroArmType.Gun)
      {
        FireGunArm();
      }
    }

    protected override void OnExit()
    {
      _playerEventDataSO.AttackChainCompleted.OnEventRaised -= RequestTransitionOutOfAttackState;
    }

    protected override void OnUpdate(float deltaTime) { }

    private void RequestTransitionOutOfAttackState()
    {
      _playerAttributesDataSO.UpdateIsAttacking(false);
      StateMachine.Sequencer.RequestTransition(this, ((PlayerRoot)Parent).Movement);
    }

    private void FireGunArm()
    {
      GameObject bulletObject = Object.Instantiate(
        _playerContext.bullet,
        _playerContext.bulletSpawnTransform.position,
        _playerContext.bulletSpawnTransform.rotation,
        _playerContext.bulletSpawnTransform
      );

      BulletHandler bulletHandler = bulletObject.GetComponent<BulletHandler>();

      bulletHandler.DirectionOfFire.x = ((Vector2)(_playerContext.bulletSpawnTransform.position - _playerContext.transform.position)).normalized.x;
    }
  }
}