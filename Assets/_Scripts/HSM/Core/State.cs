using System.Collections.Generic;

namespace stal.HSM.Core
{
  /// <summary>
  /// Abstract base class representing a node in the heirarchal state machine.
  /// </summary>
  public abstract class State
  {
    public readonly HierarchicalStateMachine StateMachine;
    public readonly State Parent;

    /// <summary>
    /// The child currently live under this node
    /// </summary>
    public State ActiveChild;

    public State(HierarchicalStateMachine stateMachine, State parent = null)
    {
      StateMachine = stateMachine;
      Parent = parent;
    }


    /// <returns> The child to transition to when entered, or null meaning we are the leaf node of the tree. </returns>
    protected virtual State GetInitialState() => null;

    /// <returns> The target state to switch to this frame, or null which means we stay in the current state. </returns>
    protected virtual State GetTransition() => null;

    /* ---------------------------------------------------------------- */
    /*                        LIFECYCLE METHODS                         */
    /* ---------------------------------------------------------------- */
    // Allows for overwriting behavior without breaking the order or execution.

    protected virtual void OnEnter() { }
    protected virtual void OnUpdate(float deltaTime) { }
    protected virtual void OnExit() { }

    /* ---------------------------------------------------------------- */
    /*                         INTERNAL METHODS                         */
    /* ---------------------------------------------------------------- */
    // Enforce strict method order of execution.

    internal void Enter()
    {
      if (Parent != null) Parent.ActiveChild = this;

      // we want to make sure the OnEnter method of the Parent is called 
      // before the OnEnter method of the child
      OnEnter();
      State init = GetInitialState();
      init?.Enter(); // if init is not null then enter
    }

    internal void Exit()
    {
      ActiveChild?.Exit(); // if ActiveChild is not null then exit
      ActiveChild = null;
      OnExit();
    }

    internal void Update(float deltaTime)
    {
      State stateToTransitionTo = GetTransition();
      if (stateToTransitionTo != null)
      {
        StateMachine.Sequencer.RequestTransition(this, stateToTransitionTo);

        // Used to return here if we requested a transition. I think this is for sequencing,
        // but the current implementation of the sequencer is very basic and does not process
        // the states prior to the transition... If I update the sequencer in the future 
        // I may re-enable this...
        // return;
      }

      // If we have an active child state, call its update.
      ActiveChild?.Update(deltaTime);

      // Update children before we update ourselves
      OnUpdate(deltaTime);
    }

    /// <returns> The deepest active descendant leaf node in the hierarchy. </returns>
    public State Leaf()
    {
      State deepestActiveDescendant = this;

      // if active child is null then this is the leaf of the hierarchy
      while (deepestActiveDescendant.ActiveChild != null)
      {
        // walk down the hierarchy
        deepestActiveDescendant = deepestActiveDescendant.ActiveChild;
      }

      return deepestActiveDescendant;
    }

    public IEnumerable<State> PathToRoot()
    {
      for (State state = this; state != null; state = state.Parent)
      {
        yield return state;
      }
    }
  }
}