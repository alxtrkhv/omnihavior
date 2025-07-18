using System.Collections.Generic;
using System.Net.NetworkInformation;
using Omnihavior.Core;

namespace Omnihavior.State;

public enum StateMachineRules
{
  None = 0,
  InterceptChildsFailure = 1 << 0,
  InterceptChildsSuccess = 1 << 1,
  NonBlockingErrors = 1 << 2,
}

public class StateMachineNode<TKey, TInputData> : IStateNode<TInputData>
{
  public static readonly StateDefinition<TKey, TInputData> NullState = new(default!, new NullState<TInputData>());

  private readonly Dictionary<TKey, StateDefinition<TKey, TInputData>> _states = [];
  private readonly List<ITransition<TKey, TInputData>> _globalTransitions = [];
  private readonly StateMachineRules _rules;

  private bool _blockTransitions;
  private TKey _defaultStateKey = default!;

  public StateDefinition<TKey, TInputData> CurrentState { get; private set; }

  public StateMachineNode(StateMachineRules rules = StateMachineRules.None)
  {
    _rules = rules;

    CurrentState = NullState;
  }

  public void InitializeRoot(TInputData input)
  {
    Enter(input);
  }

  public NodeStatus Tick(TInputData input)
  {
    TryRunTransitions(input);

    _blockTransitions = false;
    var status = CurrentState.Value.Tick(input);

    switch (status) {
      case NodeStatus.Error:
        _blockTransitions = !_rules.HasFlag(StateMachineRules.NonBlockingErrors);
        return status;

      case NodeStatus.Running:
      case NodeStatus.Failure when !_rules.HasFlag(StateMachineRules.InterceptChildsFailure):
        return status;

      case NodeStatus.Success when _rules.HasFlag(StateMachineRules.InterceptChildsSuccess):
      case NodeStatus.Failure when _rules.HasFlag(StateMachineRules.InterceptChildsFailure) &&
                                   _rules.HasFlag(StateMachineRules.InterceptChildsSuccess):
        return NodeStatus.Running;

      default:
        return NodeStatus.Success;
    }
  }

  public void Reset(TInputData input)
  {
    foreach (var state in _states) {
      state.Value.Value.Reset(input);
    }

    CurrentState.Value.Exit(input);
    CurrentState = NullState;
  }

  public void Enter(TInputData input)
  {
    SetState(_defaultStateKey, input);
  }

  public void Exit(TInputData input)
  {
    SetState(default, input);
  }

  public void AddState(TKey key, IStateNode<TInputData> state)
  {
    _states[key] = new(key, state);
  }

  public void AddTransition(ITransition<TKey, TInputData> transition)
  {
    if (transition.From == null) {
      _globalTransitions.Add(transition);
      return;
    }

    var entry = default(StateDefinition<TKey, TInputData>?);

    var contains = _states.ContainsKey(transition.From);
    if (!contains) {
      entry = new StateDefinition<TKey, TInputData>(transition.From, NullState.Value);
      _states[transition.From] = entry.Value;
    } else {
      entry = _states[transition.From];
    }

    entry.Value.Transitions.Add(transition);
  }

  public void SetDefaultState(TKey stateKey)
  {
    _defaultStateKey = stateKey;
  }

  private void SetState(TKey? key, TInputData input)
  {
    StateDefinition<TKey, TInputData> state = default!;
    if (key is null) {
      state = NullState;
    } else if (key.Equals(CurrentState.Key)) {
      return;
    } else {
      state = _states[key];
    }

    CurrentState.Value.Exit(input);
    CurrentState = state;
    CurrentState.Value.Enter(input);
  }

  private bool TryRunTransitions(TInputData input)
  {
    if (_blockTransitions) {
      return false;
    }

    foreach (var transition in _globalTransitions) {
      if (TryRunTransition(transition)) {
        return true;
      }
    }

    foreach (var currentStateTransition in CurrentState.Transitions) {
      if (TryRunTransition(currentStateTransition)) {
        return true;
      }
    }

    return false;

    bool TryRunTransition(ITransition<TKey, TInputData> transition)
    {
      var shouldInitiateTransition = transition.ConditionFulfilled(input);
      if (!shouldInitiateTransition) {
        return false;
      }

      SetState(transition.To, input);
      return true;
    }
  }
}
