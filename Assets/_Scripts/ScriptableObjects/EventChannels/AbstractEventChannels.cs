using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This is the base class for all event channels.
/// </summary>
public abstract class AbstractEventChannel : ScriptableObject
{
#if UNITY_EDITOR
  [SerializeField] private bool _logEventWhenRaised = false;
#endif

  public virtual void RaiseEvent()
  {
#if UNITY_EDITOR
    if (_logEventWhenRaised)
    {
      Debug.Log("Event Channel \"" + this.name + "\" has been invoked.");
    }
#endif
  }
}

/// <summary>
/// A generic single varible event channel.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class EventChannelSO<T> : AbstractEventChannel
{
  public UnityAction<T> OnEventRaised;

  public void RaiseEvent(T var)
  {
    base.RaiseEvent();

    OnEventRaised?.Invoke(var);
  }
}

/// <summary>
/// A generic two varible event channel.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class EventChannelSO<TOne, TTwo> : AbstractEventChannel
{
  public UnityAction<TOne, TTwo> OnEventRaised;

  public void RaiseEvent(TOne var1, TTwo var2)
  {
    base.RaiseEvent();

    OnEventRaised?.Invoke(var1, var2);
  }
}

/// <summary>
/// A generic three varible event channel.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class EventChannelSO<TOne, TTwo, TThree> : AbstractEventChannel
{
  public UnityAction<TOne, TTwo, TThree> OnEventRaised;

  public void RaiseEvent(TOne var1, TTwo var2, TThree var3)
  {
    base.RaiseEvent();

    OnEventRaised?.Invoke(var1, var2, var3);
  }
}

/// <summary>
/// A generic four varible event channel.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class EventChannelSO<TOne, TTwo, TThree, TFour> : AbstractEventChannel
{
  public UnityAction<TOne, TTwo, TThree, TFour> OnEventRaised;

  public void RaiseEvent(TOne var1, TTwo var2, TThree var3, TFour var4)
  {
    base.RaiseEvent();

    OnEventRaised?.Invoke(var1, var2, var3, var4);
  }
}
