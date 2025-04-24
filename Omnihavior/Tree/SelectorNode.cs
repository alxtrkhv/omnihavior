using System.Collections.Generic;
using Omnihavior.Core;

namespace Omnihavior.Tree;

public class SelectorNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IReadOnlyList<IBehaviorNode<TInputData>> _children;

  private int _currentChildIndex;

  public SelectorNode(params IReadOnlyList<IBehaviorNode<TInputData>> children)
  {
    _children = children;
    _currentChildIndex = 0;
  }

  public NodeStatus Tick(TInputData input)
  {
    var succeeded = false;
    for (var i = _currentChildIndex; i < _children.Count; i++) {
      var child = _children[i];
      var status = child.Tick(input);

      switch (status) {
        case NodeStatus.Success:
          succeeded = true;
          break;

        case NodeStatus.Failure:
          _currentChildIndex++;
          continue;

        case NodeStatus.Running:
        case NodeStatus.Error:
          return status;
      }

      if (succeeded) {
        break;
      }
    }

    _currentChildIndex = 0;
    return succeeded ? NodeStatus.Success : NodeStatus.Failure;
  }

  public void Reset()
  {
    _currentChildIndex = 0;

    foreach (var child in _children) {
      child.Reset();
    }
  }
}
