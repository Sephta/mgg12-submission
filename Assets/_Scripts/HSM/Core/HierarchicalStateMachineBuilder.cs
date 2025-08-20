using System.Collections.Generic;
using System.Reflection;

namespace stal.HSM.Core
{
  public class HierarchicalStateMachineBuilder
  {
    private readonly State _root;

    public HierarchicalStateMachineBuilder(State root)
    {
      _root = root;
    }

    public HierarchicalStateMachine BuildStateMachine()
    {
      HierarchicalStateMachine newStateMachine = new(_root);

      ConstructStateMachineHierarchy(_root, newStateMachine, new HashSet<State>());

      return newStateMachine;
    }

    private void ConstructStateMachineHierarchy(State state, HierarchicalStateMachine stateMachine, HashSet<State> visitedStates)
    {
      if (state == null) return;

      // avoids cycles and avoids doing redundant work
      if (!visitedStates.Add(state)) return;

      BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

      // Uses reflectiuon to bind the "StateMachine" field inside the State class to a locally scoped variable "stateMachineField"
      FieldInfo stateMachineField = typeof(State).GetField("StateMachine", bindingFlags);

      stateMachineField?.SetValue(state, stateMachine); // if field binding is not null then set its value

      foreach (FieldInfo field in state.GetType().GetFields(bindingFlags))
      {
        // skip any fields that are not of type State or a subclass thereof
        if (!typeof(State).IsAssignableFrom(field.FieldType)) continue;

        // skip parent to avoid infinite recursion
        if (field.Name == "Parent") continue;

        State childState = (State)field.GetValue(state);
        if (childState == null) continue;

        // Ensures this is a direct descendant of state
        if (!ReferenceEquals(childState.Parent, state)) continue;

        ConstructStateMachineHierarchy(childState, stateMachine, visitedStates);
      }
    }
  }
}