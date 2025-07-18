using System;
using Omnihavior.Core;

namespace Omnihavior.States;

public class LambdaStateNode<TInputData> : IStateNode<TInputData>
{
  private readonly Func<TInputData, NodeStatus>? _tick;
  private readonly Action<TInputData>? _reset;
  private readonly Action<TInputData>? _enter;
  private readonly Action<TInputData>? _exit;

  public LambdaStateNode(Func<TInputData, NodeStatus>? tick = null,
    Action<TInputData>? enter = null,
    Action<TInputData>? exit = null,
    Action<TInputData>? reset = null)
  {
    _tick = tick;
    _enter = enter;
    _exit = exit;
    _reset = reset;
  }

  public NodeStatus Tick(TInputData input)
  {
    return _tick?.Invoke(input) ?? NodeStatus.Success;
  }

  public void Reset(TInputData input)
  {
    _reset?.Invoke(input);
  }

  public void Enter(TInputData input)
  {
    _enter?.Invoke(input);
  }

  public void Exit(TInputData input)
  {
    _exit?.Invoke(input);
  }
}
