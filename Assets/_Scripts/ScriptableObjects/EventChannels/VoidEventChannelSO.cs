using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// An event channel with no arguements.
/// </summary>
[CreateAssetMenu(fileName = "New Void Event", menuName = "Scriptable Objects/Event Channels/Void Event", order = 1)]
public class VoidEventChannelSO : AbstractEventChannel
{
  public UnityAction OnEventRaised;

  public override void RaiseEvent()
  {
    base.RaiseEvent();
    OnEventRaised?.Invoke();
  }
}
