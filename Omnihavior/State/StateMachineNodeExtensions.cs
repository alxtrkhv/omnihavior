using System;
using Omnihavior.Core;

namespace Omnihavior.State;

public static class StateMachineNodeExtensions
{
  public static void AddState<TInputData>(this StateMachineNode<TInputData> node, string key,
    Func<TInputData, StateMachineContext<TInputData>, NodeStatus>? tick = null,
    Action<TInputData, StateMachineContext<TInputData>>? enter = null,
    Action<TInputData, StateMachineContext<TInputData>>? exit = null,
    Action<TInputData, StateMachineContext<TInputData>>? reset = null)
  {
    var newState = new LambdaStateNode<TInputData>(key, tick, enter, exit, reset);
    node.AddState(newState);
  }

  public static void AddTransition<TInputData>(this StateMachineNode<TInputData> node, string? from, string to,
    Func<TInputData, bool>? condition = null)
  {
    var newTransition = new LambdaTransition<TInputData>(from, to, condition);
    node.AddTransition(newTransition);
  }

  public static void AddTransition<TInputData>(this StateMachineNode<TInputData> node, string to,
    Func<TInputData, bool>? condition = null)
  {
    var newTransition = new LambdaTransition<TInputData>(null, to, condition);
    node.AddTransition(newTransition);
  }
}
