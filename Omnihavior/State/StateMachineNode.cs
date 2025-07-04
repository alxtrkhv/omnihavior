using System.Collections.Generic;
using Omnihavior.Core;

namespace Omnihavior.State;

public enum StateMachineRules
{
  None = 0,
  InterceptChildsFailure = 1 << 0,
  InterceptChildsSuccess = 1 << 1,
  NonBlockingErrors = 1 << 2,
  AllowManualStateChangesWhenBlocked = 1 << 3,
}

public class StateMachineNode<TInputData> : IStateNode<TInputData>
{
  public const string NullStateKey = "Null";
  private const string RootKey = "Root";
  public static readonly StateDefinition<TInputData> NullState = new(NullStateKey, new NullState<TInputData>());

  private readonly List<StateDefinition<TInputData>> _states = [];
  private readonly List<ITransition<TInputData>> _globalTransitions = [];
  private readonly StateMachineRules _rules;

  private int _currentStateIndex;

  private int _defaultStateIndex;
  private bool _blockTransitions;

  public StateDefinition<TInputData> CurrentState { get; private set; }

  public StateMachineContext<TInputData> Context { get; private set; }

  public StateMachineContext<TInputData> RootContext => new(
    -1,
    0,
    new() {
      { RootKey, [] },
      { NullStateKey, [int.MinValue,] },
    },
    [-1,]
  );

  public StateMachineNode(StateMachineRules rules = StateMachineRules.None)
  {
    _rules = rules;

    CurrentState = NullState;
    _currentStateIndex = int.MinValue;
    _defaultStateIndex = _currentStateIndex;
  }

  public void InitializeRoot(TInputData input)
  {
    SetAndPropagateContext(RootContext, RootKey);
    Enter(input);
  }

  public NodeStatus Tick(TInputData input)
  {
    if (TryRunTransitions(input)) {
      AchieveTargetState(Context.GetSelfState(), input);
    }

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
      state.Value.Reset(input);
    }

    CurrentState.Value.Exit(input);
    _currentStateIndex = int.MinValue;
    CurrentState = NullState;

    Context.Reset();
  }

  public void Enter(TInputData input)
  {
    AchieveTargetState(Context.GetSelfState(), input);
  }

  public void Exit(TInputData input)
  {
    AchieveTargetState(int.MinValue, input);
  }

  public void AddState(string key, IStateNode<TInputData> state)
  {
    _states.Add(new(key, state));
  }

  public void AddTransition(ITransition<TInputData> transition)
  {
    if (transition.From == null) {
      _globalTransitions.Add(transition);
      return;
    }

    var entry = default(StateDefinition<TInputData>?);

    var index = _states.FindIndex(s => s.Key == transition.From);
    if (index == -1) {
      entry = new StateDefinition<TInputData>(transition.From, NullState.Value);
      _states.Add(entry.Value);
    } else {
      entry = _states[index];
    }

    entry.Value.Transitions.Add(transition);
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
    if (_blockTransitions && !_rules.HasFlag(StateMachineRules.AllowManualStateChangesWhenBlocked)) {
      return;
    }

    Context.SetState(key);
  }

  private void AchieveTargetState(int stateIndex, TInputData input)
  {
    if (stateIndex == _currentStateIndex) {
      return;
    }

    var state = StateByIndex(ref stateIndex);

    CurrentState.Value.Exit(input);
    _currentStateIndex = stateIndex;
    CurrentState = state;
    CurrentState.Value.Enter(input);
  }

  private StateDefinition<TInputData> StateByIndex(ref int targetStateIndex)
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

    bool TryRunTransition(ITransition<TInputData> transition)
    {
      var shouldInitiateTransition = transition.ConditionFulfilled(input);
      if (!shouldInitiateTransition) {
        return false;
      }

      Context.SetState(transition.To);
      return true;
    }
  }

  private void SetAndPropagateContext(StateMachineContext<TInputData> value, string key)
  {
    Context = value;

    for (var i = 0; i < _states.Count; i++) {
      var child = _states[i];

      Context.RegisterChildLayer();
      Context.RegisterStateInMap(key, child.Key, i);

      if (child.Value is not StateMachineNode<TInputData> childMachineNode) {
        continue;
      }

      var childContext = Context.GetChildContext(i);
      childMachineNode.SetAndPropagateContext(childContext, child.Key);
    }
  }
}
