using System;
using Omnihavior.Core;

namespace Omnihavior.Trees
{
  /// <summary>
  /// Defines rules for when the <see cref="InterceptorNode{TInputData}"/> should intercept and change the status returned by its child.
  /// </summary>
  [Flags]
  public enum InterceptionRules : short
  {
    /// <summary>
    /// Never intercept the child's status.
    /// </summary>
    Never = 0,

    /// <summary>
    /// Intercept if the child returns <see cref="NodeStatus.Success"/>.
    /// </summary>
    OnSuccess = 1 << 0,

    /// <summary>
    /// Intercept if the child returns Failure.
    /// </summary>
    OnFailure = 1 << 1,

    /// <summary>
    /// Intercept if the child returns Running.
    /// </summary>
    OnRunning = 1 << 2,

    /// <summary>
    /// Intercept if the child returns Error.
    /// </summary>
    OnError = 1 << 3,

    /// <summary>
    /// If set, the interceptor immediately returns the new status without ticking the child.
    /// </summary>
    SkipChildTick = 1 << 4,

    /// <summary>
    /// Intercept regardless of the child's return status (unless SkipChildTick is set, which takes precedence).
    /// </summary>
    Always = OnSuccess | OnFailure | OnRunning | OnError,
  }

  /// <summary>
  /// A decorator node that intercepts the status returned by its child node based on specified rules
  /// and replaces it with a predefined status.
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the node operates on.</typeparam>
  public class InterceptorNode<TInputData> : IBehaviorNode<TInputData>
  {
    private readonly IBehaviorNode<TInputData> _child;
    private readonly InterceptionRules _rules;
    private readonly NodeStatus _newStatus;

    /// <summary>
    /// Initializes a new instance of the <see cref="InterceptorNode{TInputData}"/> class.
    /// </summary>
    /// <param name="child">The child node whose status might be intercepted.</param>
    /// <param name="rules">The rules determining when to intercept the child's status. Defaults to <see cref="InterceptionRules.Always"/>.</param>
    /// <param name="newStatus">The status to return when interception occurs. Defaults to <see cref="NodeStatus.Success"/>.</param>
    public InterceptorNode(IBehaviorNode<TInputData> child, InterceptionRules rules = InterceptionRules.Always,
      NodeStatus newStatus = NodeStatus.Success)
    {
      _child = child;
      _rules = rules;
      _newStatus = newStatus;
    }

    /// <inheritdoc/>
    public NodeStatus Tick(TInputData input)
    {
      var shouldReturnWithoutTicking = _rules.HasFlag(InterceptionRules.SkipChildTick);
      if (shouldReturnWithoutTicking) {
        return _newStatus;
      }

      var actualStatus = _child.Tick(input);

      return actualStatus switch {
        NodeStatus.Success when (_rules & InterceptionRules.OnSuccess) != 0 => _newStatus,
        NodeStatus.Failure when (_rules & InterceptionRules.OnFailure) != 0 => _newStatus,
        NodeStatus.Running when (_rules & InterceptionRules.OnRunning) != 0 => _newStatus,
        NodeStatus.Error when (_rules & InterceptionRules.OnError) != 0 => _newStatus,
        _ => actualStatus,
      };
    }

    /// <inheritdoc/>
    public void Reset(TInputData input)
    {
      _child.Reset(input);
    }
  }
}
