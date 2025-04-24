using Omnihavior.Core;

namespace Omnihavior.Tree;

public class InverterNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IBehaviorNode<TInputData> _child;

  public InverterNode(IBehaviorNode<TInputData> child)
  {
    _child = child;
  }

  public NodeStatus Tick(TInputData input)
  {
    var status = _child.Tick(input);
    switch (status) {
      case NodeStatus.Success:
        return NodeStatus.Failure;

      case NodeStatus.Failure:
        return NodeStatus.Success;

      case NodeStatus.Running:
      case NodeStatus.Error:
        return status;
    }

    return NodeStatus.Error;
  }

  public void Reset(TInputData input)
  {
    _child.Reset(input);
  }
}
