using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New Player Events SO", menuName = "Scriptable Objects/Player/Player Event Data")]
public class PlayerEventDataSO : ScratchpadDataSO
{
  [field: Space(10f)]
  [field: Header("Input Events")]
  [field: Space(5)]

  [field: SerializeField, Expandable]
  public CallbackContextEventChannelSO Move { get; private set; }

  public void RaiseOnMove(InputAction.CallbackContext callbackContext)
  {
    if (Move != null) Move.RaiseEvent(callbackContext);
  }

  [field: SerializeField, Expandable]
  public CallbackContextEventChannelSO Look { get; private set; }

  public void RaiseOnLook(InputAction.CallbackContext callbackContext)
  {
    if (Look != null) Look.RaiseEvent(callbackContext);
  }

  [field: SerializeField, Expandable]
  public CallbackContextEventChannelSO Attack { get; private set; }

  public void RaiseOnAttack(InputAction.CallbackContext callbackContext)
  {
    if (Attack != null) Attack.RaiseEvent(callbackContext);
  }

  [field: SerializeField, Expandable]
  public CallbackContextEventChannelSO Jump { get; private set; }

  public void RaiseOnJump(InputAction.CallbackContext callbackContext)
  {
    if (Jump != null) Jump.RaiseEvent(callbackContext);
  }

  [field: SerializeField, Expandable]
  public CallbackContextEventChannelSO TakeAim { get; private set; }

  public void RaiseOnTakeAim(InputAction.CallbackContext callbackContext)
  {
    if (TakeAim != null) TakeAim.RaiseEvent(callbackContext);
  }

  [field: SerializeField, Expandable]
  public CallbackContextEventChannelSO ConfirmAim { get; private set; }

  public void RaiseOnConfirmAim(InputAction.CallbackContext callbackContext)
  {
    if (ConfirmAim != null) ConfirmAim.RaiseEvent(callbackContext);
  }

  [field: SerializeField, Expandable]
  public CallbackContextEventChannelSO SwapArmLeft { get; private set; }

  public void RaiseOnSwapArmLeft(InputAction.CallbackContext callbackContext)
  {
    if (SwapArmLeft != null) SwapArmLeft.RaiseEvent(callbackContext);
  }

  [field: SerializeField, Expandable]
  public CallbackContextEventChannelSO SwampArmRight { get; private set; }

  public void RaiseOnSwapArmRight(InputAction.CallbackContext callbackContext)
  {
    if (SwampArmRight != null) SwampArmRight.RaiseEvent(callbackContext);
  }

  [field: Space(10f)]
  [field: Header("Combat Events")]
  [field: Space(5)]

  [field: SerializeField, Expandable]
  public VoidEventChannelSO AttackChainCompleted { get; private set; }

  [field: SerializeField, Expandable]
  public VoidEventChannelSO PlayerArmFinishedCycling { get; private set; }

  [field: SerializeField, Expandable]
  public IntIntEventChannelSO DoDamageToEntity { get; private set; }
}
