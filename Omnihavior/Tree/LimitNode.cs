using System;
using Omnihavior.Core;

namespace Omnihavior.Tree;

public class LimitNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IBehaviorNode<TInputData> _child;
  private readonly int _numberOfMaxRuns;

  private int _counter;
  private NodeStatus _cache;

  public LimitNode(IBehaviorNode<TInputData> child, int numberOfMaxRuns = 1)
  {
    _child = child;
    _numberOfMaxRuns = Math.Max(numberOfMaxRuns, 1);
  }

  public NodeStatus Tick(TInputData input)
  {
    if (_counter >= _numberOfMaxRuns) {
      return _cache;
    }

    var status = _child.Tick(input);
    switch (status) {
      case NodeStatus.Running:
      case NodeStatus.Error:
        return status;
    }

    _cache = status;
    _counter++;

    return status;
  }

  public void Reset(TInputData input)
  {
    _counter = 0;
    _child.Reset(input);
  }
}
