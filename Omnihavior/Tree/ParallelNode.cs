using System;
using System.Collections.Generic;
using Omnihavior.Core;

namespace Omnihavior.Tree;

public class ParallelNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IReadOnlyList<IBehaviorNode<TInputData>> _children;
  private readonly int _failureAllowance;

  public ParallelNode(params IReadOnlyList<IBehaviorNode<TInputData>> children) : this(children, 0) { }

  public ParallelNode(int failureAllowance, params IReadOnlyList<IBehaviorNode<TInputData>> children) : this(
    children,
    failureAllowance
  ) { }

  public ParallelNode(IReadOnlyList<IBehaviorNode<TInputData>> children, int failureAllowance)
  {
    _children = children;
    _failureAllowance = Math.Clamp(failureAllowance, 0, children.Count);
  }

  public NodeStatus Tick(TInputData input)
  {
    var failureCount = 0;
    var running = false;
    var error = false;

    foreach (var child in _children) {
      var status = child.Tick(input);
      switch (status) {
        case NodeStatus.Success:
          break;
        case NodeStatus.Failure:
          failureCount++;
          break;
        case NodeStatus.Running:
          running = true;
          break;
        case NodeStatus.Error:
          error = true;
          break;
      }
    }

    if (error) {
      return NodeStatus.Error;
    }

    if (failureCount > _failureAllowance) {
      return NodeStatus.Failure;
    }

    return running ? NodeStatus.Running : NodeStatus.Success;
  }

  public void Reset(TInputData input)
  {
    foreach (var child in _children) {
      child.Reset(input);
    }
  }
}
