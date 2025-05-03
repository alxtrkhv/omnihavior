using System;
using System.Collections.Generic;
using Omnihavior.State;

// ReSharper disable once CheckNamespace
namespace Omnihavior.Core;

/// <summary>
/// Provides a way to build behaviors conveniently with default parameters and implicit input type.
/// This class uses partial definitions to separate builder methods for different node types (e.g., Tree, Utility).
/// </summary>
/// <typeparam name="TInputData">The type of input data the behavior nodes will operate on.</typeparam>
public partial class BehaviourBuilder<TInputData>
{
  public StateMachineNode<TInputData> StateMachine(string key, List<IStateNode<TInputData>>? states = null,
    List<ITransition<TInputData>>? transitions = null,
    StateMachineRules? rules = null, string? defaultState = null)
  {
    return new(
      key,
      states,
      transitions,
      rules ?? Settings.DefaultStateMachineRules,
      defaultState
    );
  }

  public LambdaStateNode<TInputData> LambdaState(string? key = null,
    Func<TInputData, StateMachineContext<TInputData>, NodeStatus>? tick = null,
    Action<TInputData, StateMachineContext<TInputData>>? enter = null,
    Action<TInputData, StateMachineContext<TInputData>>? exit = null,
    Action<TInputData, StateMachineContext<TInputData>>? reset = null)
  {
    return new(key, tick, enter, exit, reset);
  }

  public LambdaTransition<TInputData> LambdaTransition(string? from, string to,
    Func<TInputData, bool>? condition = null)
  {
    return new(from, to, condition);
  }

  public LambdaTransition<TInputData> LambdaTransition(string to,
    Func<TInputData, bool>? condition = null)
  {
    return new(null, to, condition);
  }
}
