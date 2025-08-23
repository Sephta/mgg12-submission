using System;
using System.Threading;
using System.Threading.Tasks;
using stal.HSM.Core.Interfaces;

// Activities are useful for defining different things we want to do durring a state transition.
// These can be things like playing an animation, enabling or disabling input, changing and object's material, etc.
// Activities can be attached to specific states and will be executed in order as we enter or exit the state.
namespace stal.HSM.Core.Activities
{
  public enum ActivityStatus
  {
    Inactive,
    Activating,
    Active,
    Deactivating
  }

  public class Activity : IActivity
  {
    public ActivityStatus Status { get; protected set; } = ActivityStatus.Inactive;

    public virtual async Task ActivateAsync(CancellationToken cancellationToken)
    {
      if (Status != ActivityStatus.Inactive) return;

      Status = ActivityStatus.Activating;
      await Task.CompletedTask;
      Status = ActivityStatus.Active;
    }

    public virtual async Task DeactivateAsync(CancellationToken cancellationToken)
    {
      if (Status != ActivityStatus.Active) return;

      Status = ActivityStatus.Deactivating;
      await Task.CompletedTask;
      Status = ActivityStatus.Inactive;
    }
  }

  /// <summary>
  /// Example activity. Simply performs a delay before continuing onto the next task.
  /// </summary>
  public class DelayActivity : Activity
  {
    public float Seconds = 0.2f;

    public override async Task ActivateAsync(CancellationToken cancellationToken)
    {
      await Task.Delay(TimeSpan.FromSeconds(Seconds), cancellationToken);
      await base.ActivateAsync(cancellationToken);
    }
  }

  public class DebugLogActivity : Activity
  {
    public override async Task ActivateAsync(CancellationToken cancellationToken)
    {
      if (Status != ActivityStatus.Inactive) return;
      Status = ActivityStatus.Activating;
      await Task.Run(PrintDebug);
      Status = ActivityStatus.Active;
    }

    private void PrintDebug()
    {
      UnityEngine.Debug.Log("DEBUG ACTIVITY HELLO WORLD. Status: " + Status);
    }

    public override async Task DeactivateAsync(CancellationToken cancellationToken)
    {
      if (Status != ActivityStatus.Active) return;
      Status = ActivityStatus.Deactivating;
      await Task.Run(PrintDebug);
      Status = ActivityStatus.Inactive;
    }
  }
}