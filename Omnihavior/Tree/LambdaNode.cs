using System;
using Omnihavior.Core;

namespace Omnihavior.Tree;

public class LambdaNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly Func<TInputData, NodeStatus> _lambda;
  private readonly Action? _reset;

  public LambdaNode(Func<TInputData, NodeStatus> lambda, Action? reset = null)
  {
    _lambda = lambda;
    _reset = reset;
  }

  public NodeStatus Tick(TInputData input)
  {
    return _lambda(input);
  }

  public void Reset()
  {
    _reset?.Invoke();
  }
}
