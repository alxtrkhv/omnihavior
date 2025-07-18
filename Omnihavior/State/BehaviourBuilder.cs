using System;
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
  public StateMachineNode<TKey, TInputData> StateMachine<TKey>(StateMachineRules? rules = null,
    string? defaultState = null)
  {
    return new(
      rules ?? Settings.DefaultStateMachineRules
    );
  }

  public LambdaStateNode<TInputData> LambdaState(
    Func<TInputData, NodeStatus>? tick = null,
    Action<TInputData>? enter = null,
    Action<TInputData>? exit = null,
    Action<TInputData>? reset = null)
  {
    return new(tick, enter, exit, reset);
  }

  public LambdaTransition<TKey, TInputData> LambdaTransition<TKey>(TKey? from, TKey to,
    Func<TInputData, bool>? condition = null)
  {
    return new(from, to, condition);
  }

  public LambdaTransition<TKey, TInputData> LambdaTransition<TKey>(TKey to,
    Func<TInputData, bool>? condition = null)
  {
    return new(default, to, condition);
  }
}
