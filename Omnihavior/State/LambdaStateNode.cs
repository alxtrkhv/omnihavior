using System;
using Omnihavior.Core;

namespace Omnihavior.State;

public class LambdaStateNode<TInputData> : IStateNode<TInputData>
{
  private readonly Func<TInputData, StateMachineContext<TInputData>, NodeStatus>? _tick;
  private readonly Action<TInputData, StateMachineContext<TInputData>>? _reset;
  private readonly Action<TInputData, StateMachineContext<TInputData>>? _enter;
  private readonly Action<TInputData, StateMachineContext<TInputData>>? _exit;

  public string Key { get; set; }
  public StateMachineContext<TInputData> Context { get; set; }

  public LambdaStateNode(Func<TInputData, StateMachineContext<TInputData>, NodeStatus>? tick = null,
    Action<TInputData, StateMachineContext<TInputData>>? enter = null,
    Action<TInputData, StateMachineContext<TInputData>>? exit = null,
    Action<TInputData, StateMachineContext<TInputData>>? reset = null) : this(null, tick, enter, exit, reset) { }

  public LambdaStateNode(string? key = null, Func<TInputData, StateMachineContext<TInputData>, NodeStatus>? tick = null,
    Action<TInputData, StateMachineContext<TInputData>>? enter = null,
    Action<TInputData, StateMachineContext<TInputData>>? exit = null,
    Action<TInputData, StateMachineContext<TInputData>>? reset = null)
  {
    Key = key ?? string.Empty;
    _tick = tick;
    _enter = enter;
    _exit = exit;
    _reset = reset;
  }

  public NodeStatus Tick(TInputData input)
  {
    return _tick?.Invoke(input, Context) ?? NodeStatus.Success;
  }

  public void Reset(TInputData input)
  {
    _reset?.Invoke(input, Context);
  }

  public void Enter(TInputData input)
  {
    _enter?.Invoke(input, Context);
  }

  public void Exit(TInputData input)
  {
    _exit?.Invoke(input, Context);
  }
}
