using System;
using Omnihavior.States;

// ReSharper disable once CheckNamespace
namespace Omnihavior.Core
{
  /// <summary>
  /// Provides a way to build behaviors conveniently with default parameters and implicit input type.
  /// This class uses partial definitions to separate builder methods for different node types (e.g., Tree, Utility).
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the behavior nodes will operate on.</typeparam>
  public partial class BehaviourBuilder<TInputData>
  {
    /// <summary>
    /// Creates a new state machine node.
    /// </summary>
    /// <typeparam name="TKey">The type used to identify states.</typeparam>
    /// <param name="rules">The rules governing the state machine's behavior. Uses default settings if null.</param>
    /// <returns>A new <see cref="StateMachineNode{TKey, TInputData}"/> instance.</returns>
    public StateMachineNode<TKey, TInputData> StateMachine<TKey>(StateMachineRules? rules = null)
    {
      return new(
        rules ?? Settings.DefaultStateMachineRules
      );
    }

    /// <summary>
    /// Creates a new lambda-based state node.
    /// </summary>
    /// <param name="tick">The function to execute when the state is ticked. If null, returns <see cref="NodeStatus.Success"/>.</param>
    /// <param name="enter">The action to execute when the state is entered. If null, does nothing.</param>
    /// <param name="exit">The action to execute when the state is exited. If null, does nothing.</param>
    /// <param name="reset">The action to execute when the state is reset. If null, does nothing.</param>
    /// <returns>A new <see cref="LambdaStateNode{TInputData}"/> instance.</returns>
    public LambdaStateNode<TInputData> LambdaState(
      Func<TInputData, NodeStatus>? tick = null,
      Action<TInputData>? enter = null,
      Action<TInputData>? exit = null,
      Action<TInputData>? reset = null)
    {
      return new(tick, enter, exit, reset);
    }

    /// <summary>
    /// Creates a new lambda-based transition.
    /// </summary>
    /// <typeparam name="TKey">The type used to identify states.</typeparam>
    /// <param name="from">The source state key. If null, this becomes a global transition.</param>
    /// <param name="to">The target state key to transition to.</param>
    /// <param name="condition">The function to evaluate the transition condition. If null, the transition is always triggered.</param>
    /// <returns>A new <see cref="LambdaTransition{TKey, TInputData}"/> instance.</returns>
    public LambdaTransition<TKey, TInputData> LambdaTransition<TKey>(TKey? from, TKey to,
      Func<TInputData, bool>? condition = null)
    {
      return new(from, to, condition);
    }

    /// <summary>
    /// Creates a new lambda-based global transition.
    /// </summary>
    /// <typeparam name="TKey">The type used to identify states.</typeparam>
    /// <param name="to">The target state key to transition to.</param>
    /// <param name="condition">The function to evaluate the transition condition. If null, the transition is always triggered.</param>
    /// <returns>A new <see cref="LambdaTransition{TKey, TInputData}"/> instance.</returns>
    public LambdaTransition<TKey, TInputData> LambdaTransition<TKey>(TKey to,
      Func<TInputData, bool>? condition = null)
    {
      return new(default, to, condition);
    }
  }
}
