using System;
using Omnihavior.Core;

namespace Omnihavior.States;

/// <summary>
/// Provides extension methods for <see cref="StateMachineNode{TKey, TInputData}"/> to simplify state and transition creation.
/// </summary>
public static class StateMachineNodeExtensions
{
  /// <summary>
  /// Adds a lambda-based state to the state machine.
  /// </summary>
  /// <typeparam name="TKey">The type used to identify states.</typeparam>
  /// <typeparam name="TInputData">The type of input data the state operates on.</typeparam>
  /// <param name="node">The state machine to add the state to.</param>
  /// <param name="key">The unique key identifying the state.</param>
  /// <param name="tick">The function to execute when the state is ticked. If null, returns <see cref="NodeStatus.Success"/>.</param>
  /// <param name="enter">The action to execute when the state is entered. If null, does nothing.</param>
  /// <param name="exit">The action to execute when the state is exited. If null, does nothing.</param>
  /// <param name="reset">The action to execute when the state is reset. If null, does nothing.</param>
  public static void AddState<TKey, TInputData>(this StateMachineNode<TKey, TInputData> node, TKey key,
    Func<TInputData, NodeStatus>? tick = null,
    Action<TInputData>? enter = null,
    Action<TInputData>? exit = null,
    Action<TInputData>? reset = null)
  {
    var newState = new LambdaStateNode<TInputData>(tick, enter, exit, reset);
    node.AddState(key, newState);
  }

  /// <summary>
  /// Adds a lambda-based transition to the state machine.
  /// </summary>
  /// <typeparam name="TKey">The type used to identify states.</typeparam>
  /// <typeparam name="TInputData">The type of input data the transition operates on.</typeparam>
  /// <param name="node">The state machine to add the transition to.</param>
  /// <param name="from">The source state key. If null, this becomes a global transition.</param>
  /// <param name="to">The target state key to transition to.</param>
  /// <param name="condition">The function to evaluate the transition condition. If null, the transition is always triggered.</param>
  public static void AddTransition<TKey, TInputData>(this StateMachineNode<TKey, TInputData> node, TKey? from,
    TKey to,
    Func<TInputData, bool>? condition = null)
  {
    var newTransition = new LambdaTransition<TKey, TInputData>(from, to, condition);
    node.AddTransition(newTransition);
  }

  /// <summary>
  /// Adds a lambda-based global transition to the state machine.
  /// </summary>
  /// <typeparam name="TKey">The type used to identify states.</typeparam>
  /// <typeparam name="TInputData">The type of input data the transition operates on.</typeparam>
  /// <param name="node">The state machine to add the transition to.</param>
  /// <param name="to">The target state key to transition to.</param>
  /// <param name="condition">The function to evaluate the transition condition. If null, the transition is always triggered.</param>
  public static void AddTransition<TKey, TInputData>(this StateMachineNode<TKey, TInputData> node, TKey to,
    Func<TInputData, bool>? condition = null)
  {
    var newTransition = new LambdaTransition<TKey, TInputData>(default, to, condition);
    node.AddTransition(newTransition);
  }
}
