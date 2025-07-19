using System.Collections.Generic;
using Omnihavior.Core;

namespace Omnihavior.States
{
  /// <summary>
  /// Defines rules for customizing the behavior of a <see cref="StateMachineNode{TKey, TInputData}"/>.
  /// </summary>
  public enum StateMachineRules
  {
    /// <summary>
    /// Default behavior.
    /// </summary>
    None = 0,

    /// <summary>
    /// If set, the state machine will return <see cref="NodeStatus.Success"/> even if the current state returns <see cref="NodeStatus.Failure"/>.
    /// </summary>
    InterceptChildsFailure = 1 << 0,

    /// <summary>
    /// If set, the state machine will return <see cref="NodeStatus.Running"/> when the current state returns <see cref="NodeStatus.Success"/>.
    /// </summary>
    InterceptChildsSuccess = 1 << 1,

    /// <summary>
    /// If set, errors from the current state do not block transitions from being evaluated.
    /// </summary>
    NonBlockingErrors = 1 << 2,
  }

  /// <summary>
  /// A state machine node that manages multiple states and transitions between them.
  /// The state machine executes the current state and evaluates transitions to determine state changes.
  /// </summary>
  /// <typeparam name="TKey">The type used to identify states.</typeparam>
  /// <typeparam name="TInputData">The type of input data the state machine operates on.</typeparam>
  public class StateMachineNode<TKey, TInputData> : IStateNode<TInputData>
  {
    /// <summary>
    /// A special null state used when no valid state is set.
    /// </summary>
    public static readonly StateDefinition<TKey, TInputData> NullState = new(default!, new NullState<TInputData>());

    private readonly Dictionary<TKey, StateDefinition<TKey, TInputData>> _states = new();
    private readonly List<ITransition<TKey, TInputData>> _globalTransitions = new();
    private readonly StateMachineRules _rules;

    private bool _blockTransitions;
    private TKey _defaultStateKey = default!;

    /// <summary>
    /// Gets the currently active state definition.
    /// </summary>
    public StateDefinition<TKey, TInputData> CurrentState { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StateMachineNode{TKey, TInputData}"/> class.
    /// </summary>
    /// <param name="rules">The rules governing the state machine's behavior.</param>
    public StateMachineNode(StateMachineRules rules = StateMachineRules.None)
    {
      _rules = rules;

      CurrentState = NullState;
    }

    /// <summary>
    /// Initializes the state machine by entering the default state.
    /// </summary>
    /// <param name="input">The input data for initialization.</param>
    public void InitializeRoot(TInputData input)
    {
      Enter(input);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void Reset(TInputData input)
    {
      foreach (var state in _states) {
        state.Value.Value.Reset(input);
      }

      CurrentState.Value.Exit(input);
      CurrentState = NullState;
    }

    /// <inheritdoc/>
    public void Enter(TInputData input)
    {
      SetState(_defaultStateKey, input);
    }

    /// <inheritdoc/>
    public void Exit(TInputData input)
    {
      SetState(default, input);
    }

    /// <summary>
    /// Adds a state to the state machine.
    /// </summary>
    /// <param name="key">The unique key identifying the state.</param>
    /// <param name="state">The state node that implements the behavior for this state.</param>
    public void AddState(TKey key, IStateNode<TInputData> state)
    {
      _states[key] = new(key, state);
    }

    /// <summary>
    /// Adds a transition to the state machine.
    /// </summary>
    /// <param name="transition">The transition to add. If the From property is null, it becomes a global transition.</param>
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

    /// <summary>
    /// Sets the default state that the state machine will enter when initialized.
    /// </summary>
    /// <param name="stateKey">The key of the state to use as the default.</param>
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
}
