using System;
using System.Collections.Generic;
using Omnihavior.Core;

namespace Omnihavior.Trees
{
  /// <summary>
  /// Defines rules for customizing the behavior of a <see cref="SelectorNode{TInputData}"/>.
  /// </summary>
  [Flags]
  public enum SelectorRules
  {
    /// <summary>
    /// Default behavior.
    /// </summary>
    None = 0,

    /// <summary>
    /// If set, the selector will return <see cref="NodeStatus.Success"/> even if all children fail.
    /// Otherwise, it returns <see cref="NodeStatus.Failure"/>.
    /// </summary>
    InterceptFlowsFailure = 1 << 0,
  }

  /// <summary>
  /// A composite node that ticks its children sequentially until one succeeds or returns Running.
  /// If a child succeeds or runs, the Selector immediately returns that status.
  /// If all children fail, the Selector fails (unless <see cref="SelectorRules.InterceptFlowsFailure"/> is set).
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the node operates on.</typeparam>
  public class SelectorNode<TInputData> : IBehaviorNode<TInputData>
  {
    private readonly IReadOnlyList<IBehaviorNode<TInputData>> _children;
    private readonly SelectorRules _rules;

    private int _currentChildIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectorNode{TInputData}"/> class with default rules.
    /// </summary>
    /// <param name="children">The child nodes to execute sequentially.</param>
    public SelectorNode(params IBehaviorNode<TInputData>[] children) : this(children, SelectorRules.None) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectorNode{TInputData}"/> class.
    /// </summary>
    /// <param name="children">The child nodes to execute sequentially.</param>
    /// <param name="rules">The rules governing the selector's behavior.</param>
    public SelectorNode(IReadOnlyList<IBehaviorNode<TInputData>> children, SelectorRules rules)
    {
      _children = children;
      _rules = rules;

      _currentChildIndex = 0;
    }

    /// <inheritdoc/>
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

      var shouldInterceptFlowFailure = _rules.HasFlag(SelectorRules.InterceptFlowsFailure);
      if (shouldInterceptFlowFailure) {
        succeeded = true;
      }

      return succeeded ? NodeStatus.Success : NodeStatus.Failure;
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
}
