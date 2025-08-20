using System.Collections.Generic;

namespace stal.HSM.Core
{
  public class TransitionSequencer
  {
    public readonly HierarchicalStateMachine StateMachine;

    public TransitionSequencer(HierarchicalStateMachine stateMachine)
    {
      StateMachine = stateMachine;
    }

    /// <summary>
    /// Requests a transition from one state to another. Could be used for orchestrating inbetween tasks.
    /// i.e. Delays, animations, async behavior
    /// </summary>
    /// <param name="from">Transitioning from this state</param>
    /// <param name="to">Transitioning to this state</param>
    public void RequestTransition(State from, State to)
    {
      StateMachine.ChangeState(from, to);
    }

    /// <returns> The lowest common ancestor between state a and b, or null if one is not found. </returns>
    public static State LowestCommonAncestor(State a, State b)
    {
      HashSet<State> aParents = new();
      for (State state = a; state != null; state = state.Parent)
      {
        aParents.Add(state);
      }

      // Find the first parent of state b that is also a parent of state a
      for (State state = b; state != null; state = state.Parent)
      {
        if (aParents.Contains(state))
        {
          return state;
        }
      }

      return null;
    }
  }
}