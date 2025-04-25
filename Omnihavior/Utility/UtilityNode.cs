using System;
using System.Collections.Generic;
using Omnihavior.Core;

namespace Omnihavior.Utility;

[Flags]
public enum UtilityRules : short
{
  None = 0,
  IfEqualSelectLast = 1 << 0,
  IfEmptyFail = 1 << 1,
  IfNoActionSelectedFail = 1 << 2,
  ReturnRawStatus = 1 << 3,
}

public class UtilityNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IReadOnlyList<IEvaluatable<TInputData>> _evaluations;
  private readonly IReadOnlyList<IBehaviorNode<TInputData>> _nodes;
  private readonly UtilityRules _rules;

  private readonly float _minEvaluationThreshold;
  private readonly float _lastNodeBonus;

  private int _lastNodeIndex = -1;
  private IBehaviorNode<TInputData>? _nodeOverride;

  public UtilityNode(IReadOnlyList<IEvaluatableNode<TInputData>> nodes, UtilityRules rules = UtilityRules.None,
    float minEvaluationThreshold = float.MinValue, float lastNodeBonus = 0f) : this(
    nodes,
    nodes,
    rules,
    minEvaluationThreshold,
    lastNodeBonus
  ) { }

  public UtilityNode(IReadOnlyList<IEvaluatable<TInputData>> evaluations,
    IReadOnlyList<IBehaviorNode<TInputData>> nodes, UtilityRules rules = UtilityRules.None,
    float minEvaluationThreshold = float.MinValue, float lastNodeBonus = 0f)
  {
    _nodes = nodes;
    _evaluations = evaluations;
    _rules = rules;
    _minEvaluationThreshold = minEvaluationThreshold;
    _lastNodeBonus = lastNodeBonus;
  }

  public NodeStatus Tick(TInputData input)
  {
    if (_nodeOverride != null) {
      return TickNode(_nodeOverride, _lastNodeIndex, input);
    }

    if (_nodes.Count == 0) {
      var shouldFailIfEmpty = _rules.HasFlag(UtilityRules.IfEmptyFail);
      return shouldFailIfEmpty ? NodeStatus.Failure : NodeStatus.Success;
    }

    var bestEvaluation = float.MinValue;
    var bestNodeIndex = 0;
    var bestNode = default(IBehaviorNode<TInputData>?)!;

    for (var i = 0; i < _nodes.Count; i++) {
      var node = _nodes[i];
      var evaluation = _evaluations[i].Evaluate(input);

      if (_lastNodeIndex == i) {
        evaluation += _lastNodeBonus;
      }

      if (evaluation < bestEvaluation) {
        continue;
      }

      var isEqual = Math.Abs(evaluation - bestEvaluation) <= float.Epsilon;
      var shouldOverrideIfEqual = _rules.HasFlag(UtilityRules.IfEqualSelectLast);
      if (isEqual && !shouldOverrideIfEqual) {
        continue;
      }

      bestEvaluation = evaluation;
      bestNode = node;
      bestNodeIndex = i;
    }

    if (bestEvaluation < _minEvaluationThreshold) {
      _lastNodeIndex = -1;

      var shouldFailIfNoNodeSelected = _rules.HasFlag(UtilityRules.IfNoActionSelectedFail);
      return shouldFailIfNoNodeSelected ? NodeStatus.Failure : NodeStatus.Success;
    }

    var status = TickNode(bestNode, bestNodeIndex, input);
    return status;
  }

  public void Reset(TInputData input)
  {
    foreach (var child in _nodes) {
      child.Reset(input);
    }
  }

  private NodeStatus TickNode(IBehaviorNode<TInputData> bestNode, int bestNodeIndex, TInputData input)
  {
    _lastNodeIndex = bestNodeIndex;
    var status = bestNode.Tick(input);

    _nodeOverride = status switch {
      NodeStatus.Error or NodeStatus.Running => bestNode,
      _ => null,
    };

    switch (status) {
      case NodeStatus.Error:
      case NodeStatus.Running:
      case NodeStatus.Failure when _rules.HasFlag(UtilityRules.ReturnRawStatus):
        return status;

      default:
        return NodeStatus.Success;
    }
  }
}
