using Omnihavior.Core;

namespace Omnihavior.Trees;

/// <summary>
/// A decorator node that inverts the result of its child node.
/// <see cref="NodeStatus.Success"/> becomes <see cref="NodeStatus.Failure"/>, and <see cref="NodeStatus.Failure"/> becomes <see cref="NodeStatus.Success"/>.
/// Running and Error statuses are returned unchanged.
/// </summary>
/// <typeparam name="TInputData">The type of input data the node operates on.</typeparam>
public class InverterNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IBehaviorNode<TInputData> _child;

  /// <summary>
  /// Initializes a new instance of the <see cref="InverterNode{TInputData}"/> class.
  /// </summary>
  /// <param name="child">The child node whose result is to be inverted.</param>
  public InverterNode(IBehaviorNode<TInputData> child)
  {
    _child = child;
  }

  /// <inheritdoc/>
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

  /// <inheritdoc/>
  public void Reset(TInputData input)
  {
    _child.Reset(input);
  }
}
