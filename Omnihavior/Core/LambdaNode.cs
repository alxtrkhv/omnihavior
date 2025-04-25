using System;

namespace Omnihavior.Core;

public class LambdaNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly Func<TInputData, NodeStatus> _lambda;
  private readonly Action<TInputData>? _reset;

  public LambdaNode(Func<TInputData, NodeStatus> lambda, Action<TInputData>? reset = null)
  {
    _lambda = lambda;
    _reset = reset;
  }

  public NodeStatus Tick(TInputData input)
  {
    return _lambda(input);
  }

  public void Reset(TInputData input)
  {
    _reset?.Invoke(input);
  }
}
