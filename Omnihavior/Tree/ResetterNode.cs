using System;
using Omnihavior.Core;

namespace Omnihavior.Tree;

[Flags]
public enum ResetRules : short
{
  Always = 0,
  OnSuccess = 1,
  OnFailure = 2,
  OnRunning = 4,
  OnError = 8,
}

public class ResetterNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IBehaviorNode<TInputData> _child;
  private readonly ResetRules _rules;

  public ResetterNode(IBehaviorNode<TInputData> child, ResetRules rules)
  {
    _child = child;
    _rules = rules;
  }

  public NodeStatus Tick(TInputData input)
  {
    var status = _child.Tick(input);

    switch (status, _rules) {
      case (_, ResetRules.Always):
      case (NodeStatus.Success, _) when (_rules & ResetRules.OnSuccess) != 0:
      case (NodeStatus.Failure, _) when (_rules & ResetRules.OnFailure) != 0:
      case (NodeStatus.Running, _) when (_rules & ResetRules.OnRunning) != 0:
      case (NodeStatus.Error, _) when (_rules & ResetRules.OnError) != 0:
        _child.Reset();
        break;
    }

    return status;
  }

  public void Reset()
  {
    _child.Reset();
  }
}
