using System;
using Omnihavior.Core;

namespace Omnihavior.Tree;

[Flags]
public enum InterceptionRules : short
{
  None = 0,
  OnSuccess = 1 << 0,
  OnFailure = 1 << 1,
  OnRunning = 1 << 2,
  OnError = 1 << 3,
  Placeholder = 1 << 4,
  Negative = 1 << 5,
}

public class InterceptorNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IBehaviorNode<TInputData> _child;
  private readonly InterceptionRules _rules;

  public InterceptorNode(IBehaviorNode<TInputData> child, InterceptionRules rules)
  {
    _child = child;
    _rules = rules;
  }

  public NodeStatus Tick(TInputData input)
  {
    var negative = (_rules & InterceptionRules.Negative) != 0;
    var newStatus = negative ? NodeStatus.Failure : NodeStatus.Success;

    if ((_rules & InterceptionRules.Placeholder) != 0) {
      return newStatus;
    }

    var actualStatus = _child.Tick(input);

    return actualStatus switch {
      NodeStatus.Success when (_rules & InterceptionRules.OnSuccess) != 0 => newStatus,
      NodeStatus.Failure when (_rules & InterceptionRules.OnFailure) != 0 => newStatus,
      NodeStatus.Running when (_rules & InterceptionRules.OnRunning) != 0 => newStatus,
      NodeStatus.Error when (_rules & InterceptionRules.OnError) != 0 => newStatus,
      _ => actualStatus,
    };
  }

  public void Reset(TInputData input)
  {
    _child.Reset(input);
  }
}
