using System.Collections.Generic;

namespace stal.HSM.Core
{
  /// <summary>
  /// Represents a grouping of multiple States.
  /// </summary>
  public class HierarchicalStateMachine
  {
    public readonly State Root;
    public readonly TransitionSequencer Sequencer;
    public bool hasStarted;

    public HierarchicalStateMachine(State root)
    {
      Root = root;
      Sequencer = new TransitionSequencer(this);
    }

    public void Start()
    {
      if (hasStarted) return;

      hasStarted = true;
      Root.Enter();
    }

    public void Tick(float deltaTime)
    {
      if (!hasStarted) Start();

      // The TransitionSequencer's tick will perform our StateMachine's internal tick as long as
      // it's not already performing a requested state transition.
      Sequencer.Tick(deltaTime);
    }

    // Having an internal vs. external Tick method makes sequencing easier
    internal void InternalTick(float deltaTime) => Root.Update(deltaTime);

    /// <summary>
    /// Perform the actual switching of states by exiting up to the shared ancestor, 
    /// then entering down to the target state.
    /// </summary>
    public void ChangeState(State from, State to)
    {
      if (from == to || from == null || to == null) return;

      State lowestCommonAncestor = TransitionSequencer.LowestCommonAncestor(from, to);

      // Exit current branch up to (but no including) the lowest common ancestor
      for (State state = from; state != lowestCommonAncestor; state = state.Parent)
      {
        state.Exit();
      }

      // Enter the target branch from the lowest common ancestor down to the target
      Stack<State> stateStack = new();
      for (State state = to; state != lowestCommonAncestor; state = state.Parent)
      {
        stateStack.Push(state);
      }

      while (stateStack.Count > 0)
      {
        stateStack.Pop().Enter();
      }
    }
  }
}