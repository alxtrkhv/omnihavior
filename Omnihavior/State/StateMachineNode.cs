using System.Collections.Generic;
using Omnihavior.Core;

namespace Omnihavior.State;

public enum StateMachineRules
{
  None = 0,
  InterceptChildsFailure = 1 << 0,
  InterceptChildsSuccess = 1 << 1,
  NonBlockingErrors = 1 << 2,
  AllowManualStateChangeWhenBlocked = 1 << 3,
}

public class StateMachineNode<TInputData> : IStateNode<TInputData>
{
  public const string NullStateKey = "Null";
  public static readonly IStateNode<TInputData> NullState = new NullState<TInputData>();

  private readonly List<IStateNode<TInputData>> _states;
  private readonly List<ITransition<TInputData>> _transitions;
  private readonly StateMachineRules _rules;

  private StateMachineContext<TInputData> _context;
  private int _currentStateIndex;

  private int _defaultStateIndex;
  private bool _blockTransitions;

  public string Key { get; }

  public IStateNode<TInputData> CurrentState { get; private set; }

  public StateMachineContext<TInputData> Context
  {
    get => _context;
    set => SetAndPropagateContext(value);
  }

  public StateMachineContext<TInputData> RootContext => new(
    this,
    null,
    -1,
    0,
    new() {
      { Key, [] },
      { NullStateKey, [int.MinValue,] },
    },
    [-1,]
  );

  public StateMachineNode(string? key = null, List<IStateNode<TInputData>>? states = null,
    List<ITransition<TInputData>>? transitions = null, StateMachineRules rules = StateMachineRules.None)
  {
    Key = string.IsNullOrWhiteSpace(key) ? string.Empty : key;
    _states = states ?? [];
    _transitions = transitions ?? [];
    _rules = rules;
    CurrentState = NullState;
    _currentStateIndex = int.MinValue;
    _defaultStateIndex = _currentStateIndex;
  }

  public void InitializeRoot(TInputData input)
  {
    Context = RootContext;
    Enter(input);
  }

  public NodeStatus Tick(TInputData input)
  {
    if (TryRunTransitions(input)) {
      AchieveTargetState(_context.GetSelfState(), input);
    }

    _blockTransitions = false;
    var status = CurrentState.Tick(input);

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
      state.Reset(input);
    }

    CurrentState.Exit(input);
    _currentStateIndex = int.MinValue;
    CurrentState = NullState;

    _context.Reset();
  }

  public void Enter(TInputData input)
  {
    AchieveTargetState(_context.GetSelfState(), input);
  }

  public void Exit(TInputData input)
  {
    AchieveTargetState(int.MinValue, input);
  }

  public void AddState(IStateNode<TInputData> state)
  {
    _states.Add(state);
  }

  public void AddTransition(ITransition<TInputData> transition)
  {
    _transitions.Add(transition);
  }

  public void SetDefaultState(string? stateKey)
  {
    if (stateKey == null) {
      _defaultStateIndex = int.MinValue;
      return;
    }

    var stateIndex = _states.FindIndex(s => s.Key == stateKey);
    if (stateIndex == -1) {
      _defaultStateIndex = int.MinValue;
      return;
    }

    _defaultStateIndex = stateIndex;
  }

  public void SetState(string? key)
  {
    if (_blockTransitions && !_rules.HasFlag(StateMachineRules.AllowManualStateChangeWhenBlocked)) {
      return;
    }

    _context.SetState(key);
  }

  private void AchieveTargetState(int stateIndex, TInputData input)
  {
    if (stateIndex == _currentStateIndex) {
      return;
    }

    var state = StateByIndex(ref stateIndex);

    CurrentState.Exit(input);
    _currentStateIndex = stateIndex;
    CurrentState = state;
    CurrentState.Enter(input);
  }

  private IStateNode<TInputData> StateByIndex(ref int targetStateIndex)
  {
    if (targetStateIndex >= _states.Count) {
      return NullState;
    }

    if (targetStateIndex == -1) {
      targetStateIndex = _defaultStateIndex;
    }

    if (targetStateIndex < 0) {
      return NullState;
    }

    return _states[targetStateIndex];
  }

  private bool TryRunTransitions(TInputData input)
  {
    if (_blockTransitions) {
      return false;
    }

    foreach (var transition in _transitions) {
      if (transition.From != null && transition.From != CurrentState.Key) {
        continue;
      }

      var shouldInitiateTransition = transition.ConditionFulfilled(input);
      if (!shouldInitiateTransition) {
        continue;
      }

      _context.SetState(transition.To);
      return true;
    }

    return false;
  }

  private void SetAndPropagateContext(StateMachineContext<TInputData> value)
  {
    _context = value;

    for (var i = 0; i < _states.Count; i++) {
      var child = _states[i];
      var parent = this;

      Context.RegisterStateInMap(parent, child, i);
      child.Context = _context.GetChildContext(parent, i);
    }
  }
}
