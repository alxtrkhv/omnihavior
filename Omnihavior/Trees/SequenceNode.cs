using System;
using System.Collections.Generic;
using Omnihavior.Core;

namespace Omnihavior.Trees;

/// <summary>
/// Defines rules for customizing the behavior of a <see cref="SequenceNode{TInputData}"/>.
/// </summary>
[Flags]
public enum SequenceRules
{
  /// <summary>
  /// Default behavior.
  /// </summary>
  None = 0,

  /// <summary>
  /// If set, the sequence will return Success even if a child fails. Otherwise, it returns Failure immediately upon child failure.
  /// </summary>
  InterceptChildsFailure = 1 << 0,

  /// <summary>
  /// If set, the sequence will continue ticking subsequent children even if one fails. The final status depends on <see cref="InterceptChildsFailure"/>.
  /// </summary>
  IgnoreChildsFailure = 1 << 1,
}

/// <summary>
/// A composite node that ticks its children sequentially until one fails or returns Running.
/// If a child fails or runs, the Sequence immediately returns that status (unless rules modify this).
/// If all children succeed, the Sequence succeeds.
/// </summary>
/// <typeparam name="TInputData">The type of input data the node operates on.</typeparam>
public class SequenceNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IReadOnlyList<IBehaviorNode<TInputData>> _children;
  private readonly SequenceRules _rules;

  private int _currentChildIndex;

  /// <summary>
  /// Initializes a new instance of the <see cref="SequenceNode{TInputData}"/> class with default rules.
  /// </summary>
  /// <param name="children">The child nodes to execute sequentially.</param>
  public SequenceNode(params IBehaviorNode<TInputData>[] children) : this(children, SequenceRules.None) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="SequenceNode{TInputData}"/> class.
  /// </summary>
  /// <param name="children">The child nodes to execute sequentially.</param>
  /// <param name="rules">The rules governing the sequence's behavior.</param>
  public SequenceNode(IReadOnlyList<IBehaviorNode<TInputData>> children, SequenceRules rules)
  {
    _children = children;
    _rules = rules;

    _currentChildIndex = 0;
  }

  /// <inheritdoc/>
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
          var shouldInterceptChildsFailure = _rules.HasFlag(SequenceRules.InterceptChildsFailure);
          failed = !shouldInterceptChildsFailure;

          var shouldIgnoreChildsFailure = _rules.HasFlag(SequenceRules.IgnoreChildsFailure);
          if (shouldIgnoreChildsFailure) {
            _currentChildIndex++;
            continue;
          }

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

  /// <inheritdoc/>
  public void Reset(TInputData input)
  {
    _currentChildIndex = 0;

    foreach (var child in _children) {
      child.Reset(input);
    }
  }
}
