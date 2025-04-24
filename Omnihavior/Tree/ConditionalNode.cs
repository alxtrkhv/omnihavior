using System;
using Omnihavior.Core;

namespace Omnihavior.Tree;

[Flags]
public enum ConditionRules : short
{
  None = 0,
  CheckConditionOnlyOnce = 1 << 0,
  ReturnRawStatus = 1 << 1,
}

public class ConditionalNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IBehaviorNode<TInputData> _condition;
  private readonly IBehaviorNode<TInputData> _positiveBody;
  private readonly IBehaviorNode<TInputData>? _negativeBody;

  private readonly ConditionRules _rules;

  private NodeStatus? _conditionCache;

  public ConditionalNode(IBehaviorNode<TInputData> condition, IBehaviorNode<TInputData> positiveBody,
    IBehaviorNode<TInputData>? negativeBody = null, ConditionRules rules = ConditionRules.None)
  {
    _condition = condition;
    _positiveBody = positiveBody;
    _rules = rules;
    _negativeBody = negativeBody;
  }

  public NodeStatus Tick(TInputData input)
  {
    var conditionStatus = _conditionCache ?? _condition.Tick(input);
    var body = default(IBehaviorNode<TInputData>?);

    var useConditionCache = _rules.HasFlag(ConditionRules.CheckConditionOnlyOnce);
    var returnRawStatus = _rules.HasFlag(ConditionRules.ReturnRawStatus);

    switch (conditionStatus) {
      case NodeStatus.Success:
        body = _positiveBody;
        break;

      case NodeStatus.Failure:
        body = _negativeBody;
        break;

      case NodeStatus.Running:
      case NodeStatus.Error:
        returnRawStatus = true;
        break;
    }

    var bodyStatus = body?.Tick(input);
    switch (bodyStatus) {
      case NodeStatus.Success:
      case NodeStatus.Failure:
        if (!useConditionCache) {
          _conditionCache = null;
        }

        break;

      case NodeStatus.Error:
      case NodeStatus.Running:
        _conditionCache = conditionStatus;
        returnRawStatus = true;
        break;
    }

    if (returnRawStatus) {
      return bodyStatus ?? conditionStatus;
    }

    _conditionCache = useConditionCache ? conditionStatus : null;

    return NodeStatus.Success;
  }

  public void Reset(TInputData input)
  {
    _condition.Reset(input);
    _positiveBody.Reset(input);
    _negativeBody?.Reset(input);

    _conditionCache = null;
  }
}
