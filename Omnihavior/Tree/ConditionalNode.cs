using System;
using Omnihavior.Core;

namespace Omnihavior.Tree;

/// <summary>
/// Defines rules for customizing the behavior of a <see cref="ConditionalNode{TInputData}"/>.
/// </summary>
[Flags]
public enum ConditionRules : short
{
  /// <summary>
  /// Default behavior.
  /// </summary>
  None = 0,

  /// <summary>
  /// If set, the result of the condition node is cached if the condition returns <see cref="NodeStatus.Success"/> or <see cref="NodeStatus.Failure"/>.
  /// The cached result is used in the following ticks instead of re-evaluating the condition.
  /// </summary>
  CacheCondition = 1 << 0,

  /// <summary>
  /// If set, the conditional node will return <see cref="NodeStatus.Success"/> even if the executed body (positive or negative) returns Failure.
  /// </summary>
  InterceptChildsFailure = 1 << 1,

  /// <summary>
  /// If set, and the condition fails and there is no negative body, the node returns <see cref="NodeStatus.Success"/>. Otherwise, it returns Failure.
  /// </summary>
  InterceptFlowsFailure = 1 << 2,
}

/// <summary>
/// A composite node that executes one of two child branches based on the result of a condition node.
/// If the condition succeeds, the positive body is executed.
/// If the condition fails, the negative body (if provided) is executed.
/// </summary>
/// <typeparam name="TInputData">The type of input data the node operates on.</typeparam>
public class ConditionalNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IBehaviorNode<TInputData> _condition;
  private readonly IBehaviorNode<TInputData> _positiveBody;
  private readonly IBehaviorNode<TInputData>? _negativeBody;

  private readonly ConditionRules _rules;

  private NodeStatus? _conditionCache;

  /// <summary>
  /// Initializes a new instance of the <see cref="ConditionalNode{TInputData}"/> class.
  /// </summary>
  /// <param name="condition">The node whose result determines which body to execute.</param>
  /// <param name="positiveBody">The node to execute if the condition succeeds.</param>
  /// <param name="negativeBody">The optional node to execute if the condition fails.</param>
  /// <param name="rules">The rules governing the conditional's behavior.</param>
  public ConditionalNode(IBehaviorNode<TInputData> condition, IBehaviorNode<TInputData> positiveBody,
    IBehaviorNode<TInputData>? negativeBody = null, ConditionRules rules = ConditionRules.None)
  {
    _condition = condition;
    _positiveBody = positiveBody;
    _rules = rules;
    _negativeBody = negativeBody;
  }

  /// <inheritdoc/>
  public NodeStatus Tick(TInputData input)
  {
    var conditionStatus = _conditionCache ?? _condition.Tick(input);
    var body = default(IBehaviorNode<TInputData>?);

    var useConditionCache = _rules.HasFlag(ConditionRules.CacheCondition);
    var shouldInterceptChildsFailure = _rules.HasFlag(ConditionRules.InterceptChildsFailure);
    var shouldInterceptFlowsFailureWithNullNegativeBody = _rules.HasFlag(ConditionRules.InterceptFlowsFailure);

    switch (conditionStatus) {
      case NodeStatus.Success:
        body = _positiveBody;
        break;

      case NodeStatus.Failure:
        body = _negativeBody;
        break;

      case NodeStatus.Running:
      case NodeStatus.Error:
        return conditionStatus;
    }

    var result = conditionStatus;
    var bodyStatus = body?.Tick(input);
    switch (bodyStatus) {
      case null when shouldInterceptFlowsFailureWithNullNegativeBody:
      case NodeStatus.Success:
      case NodeStatus.Failure when shouldInterceptChildsFailure:
        result = NodeStatus.Success;
        break;

      case NodeStatus.Failure:
        result = NodeStatus.Failure;
        break;

      case NodeStatus.Error:
      case NodeStatus.Running:
        _conditionCache = conditionStatus;
        return bodyStatus.Value;
    }

    _conditionCache = useConditionCache ? conditionStatus : null;

    return result;
  }

  /// <inheritdoc/>
  public void Reset(TInputData input)
  {
    _condition.Reset(input);
    _positiveBody.Reset(input);
    _negativeBody?.Reset(input);

    _conditionCache = null;
  }
}
