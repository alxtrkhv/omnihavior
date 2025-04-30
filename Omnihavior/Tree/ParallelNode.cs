using System;
using System.Collections.Generic;
using Omnihavior.Core;

namespace Omnihavior.Tree;

/// <summary>
/// A composite node that ticks all its children in parallel.
/// It succeeds if no more than a specified number of children fail.
/// It runs if any child is running.
/// It fails if more than the allowed number of children fails.
/// It errors if any child errors.
/// </summary>
/// <typeparam name="TInputData">The type of input data the node operates on.</typeparam>
public class ParallelNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IReadOnlyList<IBehaviorNode<TInputData>> _children;
  private readonly int _failureAllowance;

  /// <summary>
  /// Initializes a new instance of the <see cref="ParallelNode{TInputData}"/> class with zero failure allowance.
  /// </summary>
  /// <param name="children">The child nodes to execute in parallel.</param>
  public ParallelNode(params IBehaviorNode<TInputData>[] children) : this(children, 0) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ParallelNode{TInputData}"/> class.
  /// </summary>
  /// <param name="children">The child nodes to execute in parallel.</param>
  /// <param name="failureAllowance">The maximum number of children allowed to fail for the node to still succeed.</param>
  public ParallelNode(IReadOnlyList<IBehaviorNode<TInputData>> children, int failureAllowance)
  {
    _children = children;
    _failureAllowance = Math.Clamp(failureAllowance, 0, children.Count);
  }

  /// <inheritdoc/>
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

  /// <inheritdoc/>
  public void Reset(TInputData input)
  {
    foreach (var child in _children) {
      child.Reset(input);
    }
  }
}
