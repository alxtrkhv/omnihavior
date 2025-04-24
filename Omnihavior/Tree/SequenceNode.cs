using System.Collections.Generic;
using Omnihavior.Core;

namespace Omnihavior.Tree;

public class SequenceNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IReadOnlyList<IBehaviorNode<TInputData>> _children;

  private int _currentChildIndex;

  public SequenceNode(params IReadOnlyList<IBehaviorNode<TInputData>> children)
  {
    _children = children;
    _currentChildIndex = 0;
  }

  public NodeStatus Tick(TInputData input)
  {
    var failed = false;
    for (var i = _currentChildIndex; i < _children.Count; i++) {
      var child = _children[i];
      var status = child.Tick(input);

      switch (status) {
        case NodeStatus.Success:
          _currentChildIndex++;
          continue;

        case NodeStatus.Failure:
          failed = true;
          break;

        case NodeStatus.Running:
        case NodeStatus.Error:
          return status;
      }

      if (failed) {
        break;
      }
    }

    _currentChildIndex = 0;
    return failed ? NodeStatus.Failure : NodeStatus.Success;
  }

  public void Reset(TInputData input)
  {
    _currentChildIndex = 0;

    foreach (var child in _children) {
      child.Reset(input);
    }
  }
}
