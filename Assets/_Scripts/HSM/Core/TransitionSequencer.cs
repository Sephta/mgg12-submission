using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using stal.HSM.Core.Activities;
using stal.HSM.Core.Interfaces;
using stal.HSM.Core.Sequences;

namespace stal.HSM.Core
{

  /// <summary>
  /// When make state transitions this class will handle sequencing any behavior that should take place in between those transitions.
  /// </summary>
  public class TransitionSequencer
  {
    public readonly HierarchicalStateMachine StateMachine;

    /// <summary>
    /// The current phase of the transition. Usually we are either deactivating a branch of the state machine or
    /// we're activating a new branch.
    /// </summary>
    private ISequence _sequencer;

    /// <summary>
    /// Activates a delegate to perform the logic of switching between sequences (phases) of the active transition.
    /// </summary>
    private Action ChangeToNextPhase;

    /// <summary>
    /// If a transition is requested while one is currently in progress we can queue it here to run aftwards.
    /// </summary>
    private (State _from, State _to)? _pending;  // coalesce a single _pending sequence

    // for tracking the previous transition request. Useful for logging and debugging transitions.
    private State _lastFrom;
    private State _lastTo;

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
      // Ignore invalid transitions.
      if (to == null || from == to) return;

      // if we're in the middle of a transition then queue up the request.
      if (_sequencer != null)
      {
        _pending = (from, to);
        return;
      }

      // If the transition is valid, and we're not activily transitioning, begin transition.
      BeginTransition(from, to);
    }

    private CancellationTokenSource cancellationTokenSource;

    /// <summary>
    /// Set to false to use parallel sequencing.
    /// </summary>
    public readonly bool UseSequential = true;

    private void BeginTransition(State from, State to)
    {
      cancellationTokenSource?.Cancel();
      cancellationTokenSource = new();

      State lowestCommonAncestor = LowestCommonAncestor(from, to);
      List<State> exitChain = StatesToExit(from, lowestCommonAncestor);
      List<State> enterChain = StatesToEnter(to, lowestCommonAncestor);

      // 1. Deactivate the "old branch"
      List<PhaseStep> exitSteps = GatherPhaseSteps(exitChain, deactivate: true);
      _sequencer = UseSequential
        ? new SequentialPhase(exitSteps, cancellationTokenSource.Token)
        : new ParallelPhase(exitSteps, cancellationTokenSource.Token);

      _sequencer.Start();

      ChangeToNextPhase += () =>
      {
        // 2. Change state
        StateMachine.ChangeState(from, to);

        // 3. Activate the "new branch"
        List<PhaseStep> enterSteps = GatherPhaseSteps(enterChain, deactivate: false);
        _sequencer = UseSequential
          ? new SequentialPhase(enterSteps, cancellationTokenSource.Token)
          : new ParallelPhase(enterSteps, cancellationTokenSource.Token);
        _sequencer.Start();
      };
    }

    private void EndTransition()
    {
      // clear out the final phase of the transition.
      _sequencer = null;

      // if we have another transition request queued, begin that transition.
      if (_pending.HasValue)
      {
        (State from, State to) = _pending.Value;
        _pending = null;
        BeginTransition(from, to);
      }
    }

    public void Tick(float deltaTime)
    {
      // if not null then we are currently transitioning
      if (_sequencer != null)
      {
        if (_sequencer.Update())
        {
          if (ChangeToNextPhase != null)
          {
            Action cachedPhaseSwap = ChangeToNextPhase;
            ChangeToNextPhase = null;
            cachedPhaseSwap();
          }
          else
          {
            // Clean up and check for any pending transition requests.
            EndTransition();
          }
        }
        // return early to avoid normal state updates during a transition
        return;
      }

      // If we're not actively performing a transition we tick the state machine
      StateMachine.InternalTick(deltaTime);
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

    /// <summary>
    /// States to exit: from → ... up to (but excluding) lca; bottom→up order.
    /// </summary>
    private static List<State> StatesToExit(State from, State lowestCommonAncestor)
    {
      List<State> statesToExit = new();

      for (State state = from; state != null && state != lowestCommonAncestor; state = state.Parent)
      {
        statesToExit.Add(state);
      }

      return statesToExit;
    }

    /// <summary>
    /// States to enter: path from 'to' up to (but excluding) lca; returned in enter order (top→down).
    /// </summary>
    private static List<State> StatesToEnter(State to, State lowestCommonAncestor)
    {
      Stack<State> statesToEnter = new();

      for (State state = to; state != null && state != lowestCommonAncestor; state = state.Parent)
      {
        statesToEnter.Push(state);
      }

      return new List<State>(statesToEnter);
    }

    static List<PhaseStep> GatherPhaseSteps(List<State> chain, bool deactivate)
    {
      List<PhaseStep> phaseSteps = new();

      for (int i = 0; i < chain.Count; i++)
      {
        State state = chain[i];
        IReadOnlyList<IActivity> activities = chain[i].Activities;
        for (int j = 0; j < activities.Count; j++)
        {
          IActivity activity = activities[j];
          bool include = deactivate
            ? activity.Status == ActivityStatus.Active
            : activity.Status == ActivityStatus.Inactive;

          if (!include) continue;

          UnityEngine.Debug.Log($"[Phase {(deactivate ? "Exit" : "Enter")}] state={state.GetType().Name}, activity={activity.GetType().Name}, mode={activity.Status}");

          phaseSteps.Add(cancellationToken => deactivate ? activity.DeactivateAsync(cancellationToken) : activity.ActivateAsync(cancellationToken));
        }
      }

      return phaseSteps;
    }
  }
}