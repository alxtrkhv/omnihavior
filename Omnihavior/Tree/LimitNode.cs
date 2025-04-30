using System;
using Omnihavior.Core;

namespace Omnihavior.Tree;

/// <summary>
/// A decorator node that limits the number of times its child node can be executed successfully or fail.
/// After the limit is reached, it returns the last status returned by the child.
/// </summary>
/// <typeparam name="TInputData">The type of input data the node operates on.</typeparam>
public class LimitNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IBehaviorNode<TInputData> _child;
  private readonly int _numberOfMaxRuns;

  private int _counter;
  private NodeStatus _cache;

  /// <summary>
  /// Initializes a new instance of the <see cref="LimitNode{TInputData}"/> class.
  /// </summary>
  /// <param name="child">The child node to limit.</param>
  /// <param name="numberOfMaxRuns">The maximum number of times the child can run (Success or Failure).</param>
  public LimitNode(IBehaviorNode<TInputData> child, int numberOfMaxRuns)
  {
    _child = child;
    _numberOfMaxRuns = Math.Max(numberOfMaxRuns, 1);
  }

  /// <inheritdoc/>
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

  /// <inheritdoc/>
  public void Reset(TInputData input)
  {
    _counter = 0;
    _child.Reset(input);
  }
}
