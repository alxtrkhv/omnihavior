using System;
using Omnihavior.Core;

namespace Omnihavior.Trees
{
  /// <summary>
  /// Defines rules for when the child of a <see cref="ResetterNode{TInputData}"/> should be reset.
  /// </summary>
  [Flags]
  public enum ResetRules : short
  {
    /// <summary>
    /// Never reset the child.
    /// </summary>
    Never = 0,

    /// <summary>
    /// Reset the child if it returns Success.
    /// </summary>
    OnSuccess = 1 << 0,

    /// <summary>
    /// Reset the child if it returns Failure.
    /// </summary>
    OnFailure = 1 << 1,

    /// <summary>
    /// Reset the child if it returns Running.
    /// </summary>
    OnRunning = 1 << 2,

    /// <summary>
    /// Reset the child if it returns Error.
    /// </summary>
    OnError = 1 << 3,

    /// <summary>
    /// Reset the child if it returns <see cref="NodeStatus.Success"/> or <see cref="NodeStatus.Failure"/>. (Default)
    /// </summary>
    OnResult = OnSuccess | OnFailure,

    /// <summary>
    /// Reset the child regardless of its return status.
    /// </summary>
    Always = OnSuccess | OnFailure | OnRunning | OnError,
  }

  /// <summary>
  /// A decorator node that resets its child node based on specified rules after the child is ticked.
  /// It returns the status of the child node unchanged.
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the node operates on.</typeparam>
  public class ResetterNode<TInputData> : IBehaviorNode<TInputData>
  {
    private readonly IBehaviorNode<TInputData> _child;
    private readonly ResetRules _rules;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetterNode{TInputData}"/> class.
    /// </summary>
    /// <param name="child">The child node to wrap.</param>
    /// <param name="rules">The rules determining when to reset the child. Defaults to <see cref="ResetRules.OnResult"/>.</param>
    public ResetterNode(IBehaviorNode<TInputData> child, ResetRules rules = ResetRules.OnResult)
    {
      _child = child;
      _rules = rules;
    }

    /// <inheritdoc/>
    public NodeStatus Tick(TInputData input)
    {
      var status = _child.Tick(input);

      switch (status, _rules) {
        case (NodeStatus.Success, _) when _rules.HasFlag(ResetRules.OnSuccess):
        case (NodeStatus.Failure, _) when _rules.HasFlag(ResetRules.OnFailure):
        case (NodeStatus.Running, _) when _rules.HasFlag(ResetRules.OnRunning):
        case (NodeStatus.Error, _) when _rules.HasFlag(ResetRules.OnError):
          _child.Reset(input);
          break;
      }

      return status;
    }

    /// <inheritdoc/>
    public void Reset(TInputData input)
    {
      _child.Reset(input);
    }
  }
}
