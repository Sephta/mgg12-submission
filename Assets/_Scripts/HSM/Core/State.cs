using System.Collections.Generic;
using stal.HSM.Core.Interfaces;

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

    private readonly List<IActivity> _activities = new();
    public IReadOnlyList<IActivity> Activities => _activities;

    public State(HierarchicalStateMachine stateMachine, State parent = null)
    {
      StateMachine = stateMachine;
      Parent = parent;
    }

    /// <summary>
    /// Attach behavior to the state that runs durring state transitions, such as delays or effects,
    /// without coupling it with the state's internal logic.
    /// </summary>
    public void AddActivity(IActivity activity)
    {
      if (activity != null) _activities.Add(activity);
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

      // if stateToTransitionTo is null then we dont want to transition. Continue update cycle
      // if its not null we need to make sure we arn't transitioning into and already active child state.
      if (stateToTransitionTo != null && stateToTransitionTo != ActiveChild)
      {
        StateMachine.Sequencer.RequestTransition(this, stateToTransitionTo);

        // Return early. If we requested a transition we don't need to update our currently active child state
        // since that child state is about to change.
        return;
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