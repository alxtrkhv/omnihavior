using System;
using Omnihavior.Core;

namespace Omnihavior.Tree;

public class ThrottleNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IBehaviorNode<TInputData> _child;
  private readonly int _onceIn;
  private readonly int _offset;

  private int _counter = -1;

  public ThrottleNode(IBehaviorNode<TInputData> child, int onceIn, int offset = 0)
  {
    _child = child;
    _onceIn = Math.Max(onceIn, 1);
    _offset = Math.Max(offset, 0) % _onceIn;
  }

  public NodeStatus Tick(TInputData input)
  {
    _counter = (_counter + 1) % _onceIn;
    var effectiveCounter = (_counter + _offset) % _onceIn;
    if (effectiveCounter != 0) {
      return NodeStatus.Success;
    }

    var status = _child.Tick(input);

    return status;
  }

  public void Reset(TInputData input)
  {
    _counter = -1;
    _child.Reset(input);
  }
}
