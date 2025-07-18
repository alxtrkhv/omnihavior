using System;
using Omnihavior.Core;

namespace Omnihavior.State;

public static class StateMachineNodeExtensions
{
  public static void AddState<TKey, TInputData>(this StateMachineNode<TKey, TInputData> node, TKey key,
    Func<TInputData, NodeStatus>? tick = null,
    Action<TInputData>? enter = null,
    Action<TInputData>? exit = null,
    Action<TInputData>? reset = null)
  {
    var newState = new LambdaStateNode<TInputData>(tick, enter, exit, reset);
    node.AddState(key, newState);
  }

  public static void AddTransition<TKey, TInputData>(this StateMachineNode<TKey, TInputData> node, TKey? from,
    TKey to,
    Func<TInputData, bool>? condition = null)
  {
    var newTransition = new LambdaTransition<TKey, TInputData>(from, to, condition);
    node.AddTransition(newTransition);
  }

  public static void AddTransition<TKey, TInputData>(this StateMachineNode<TKey, TInputData> node, TKey to,
    Func<TInputData, bool>? condition = null)
  {
    var newTransition = new LambdaTransition<TKey, TInputData>(default, to, condition);
    node.AddTransition(newTransition);
  }
}
