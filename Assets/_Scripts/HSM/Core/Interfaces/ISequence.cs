namespace stal.HSM.Core.Interfaces
{
  /// <summary>
  /// Interface for implementing a "unit" of transition work. A transition can consist of one or more of these sequences.
  /// </summary>
  public interface ISequence
  {
    /// <summary>
    /// Tells us whether the sequence has completed.
    /// </summary>
    bool IsDone { get; }

    /// <summary>
    /// Called once to begin the sequence.
    /// </summary>
    void Start();

    /// <summary>
    /// Called every frame while the sequence is active.
    /// </summary>
    /// <returns>true if the sequence is complete, false otherwise.</returns>
    bool Update();
  }
}
