using System.Threading;
using System.Threading.Tasks;
using stal.HSM.Core.Activities;

namespace stal.HSM.Core.Interfaces
{
  /// <summary>
  /// An interface for defining how the TransitionSequencer should activate or deactive behavior durring a transition.
  /// </summary>
  public interface IActivity
  {
    /// <summary>
    /// The activity's current status.
    /// </summary>
    ActivityStatus Status { get; }

    /// <summary>
    /// Called once we enter a state. Allows us to trigger any setup behavior asynchronously for the state.
    /// </summary>
    public Task ActivateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Called once we exit a state. Allows us to perform cleanup behavior asynchronously for the state.
    /// </summary>
    public Task DeactivateAsync(CancellationToken cancellationToken);
  }
}
