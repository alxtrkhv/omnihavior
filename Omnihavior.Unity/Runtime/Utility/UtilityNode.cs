using System;
using System.Collections.Generic;
using Omnihavior.Core;

namespace Omnihavior.Utility
{
  /// <summary>
  /// Defines rules for customizing the behavior of a <see cref="UtilityNode{TInputData}"/>.
  /// </summary>
  [Flags]
  public enum UtilityRules : short
  {
    /// <summary>
    /// Default behavior.
    /// </summary>
    None = 0,

    /// <summary>
    /// If set, and multiple nodes have the same highest evaluation score, the one that runs last (if any) is selected. Otherwise, the first one encountered is selected.
    /// </summary>
    IfEqualSelectLast = 1 << 0,

    /// <summary>
    /// If set, the node returns Success if it has no children. Otherwise, it returns Failure.
    /// </summary>
    InterceptFlowsFailureIfEmpty = 1 << 1,

    /// <summary>
    /// If set, the node returns Success if no child's evaluation score meets the minimum threshold. Otherwise, it returns Failure.
    /// </summary>
    InterceptFlowsFailureIfNoActionPassesThreshold = 1 << 2,

    /// <summary>
    /// If set, the node returns Success even if the selected child node returns Failure. Otherwise, it returns the child's Failure status.
    /// </summary>
    InterceptChildsFailure = 1 << 3,
  }

  /// <summary>
  /// A composite node that selects and ticks a single child based on utility scores provided by evaluators.
  /// Each tick, it evaluates all children, selects the one with the highest score (considering rules and thresholds), and ticks it.
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the node operates on.</typeparam>
  public class UtilityNode<TInputData> : IBehaviorNode<TInputData>
  {
    private readonly IReadOnlyList<IEvaluatable<TInputData>> _evaluations;
    private readonly IReadOnlyList<IBehaviorNode<TInputData>> _nodes;
    private readonly UtilityRules _rules;

    private readonly float _minEvaluationThreshold;
    private readonly float _lastNodeBonus;

    private int _lastNodeIndex = -1;
    private IBehaviorNode<TInputData>? _nodeOverride;

    /// <summary>
    /// Initializes a new instance of the <see cref="UtilityNode{TInputData}"/> class where nodes are also evaluators.
    /// </summary>
    /// <param name="nodes">The list of child nodes, which must implement <see cref="IEvaluatableNode{TInputData}"/>.</param>
    /// <param name="rules">The rules governing the utility node's behavior.</param>
    /// <param name="minEvaluationThreshold">The minimum evaluation score a node must have to be considered for execution.</param>
    /// <param name="lastNodeBonus">A bonus score added to the node that ran last tick, potentially increasing its chance of running again.</param>
    public UtilityNode(IReadOnlyList<IEvaluatableNode<TInputData>> nodes, UtilityRules rules = UtilityRules.None,
      float minEvaluationThreshold = float.MinValue, float lastNodeBonus = 0f) : this(
      nodes,
      nodes,
      rules,
      minEvaluationThreshold,
      lastNodeBonus
    ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UtilityNode{TInputData}"/> class with separate evaluators and nodes.
    /// </summary>
    /// <param name="evaluations">The list of evaluators corresponding to each node.</param>
    /// <param name="nodes">The list of child behavior nodes.</param>
    /// <param name="rules">The rules governing the utility node's behavior.</param>
    /// <param name="minEvaluationThreshold">The minimum evaluation score a node must have to be considered for execution.</param>
    /// <param name="lastNodeBonus">A bonus score added to the node that ran last tick, potentially increasing its chance of running again.</param>
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

    /// <inheritdoc/>
    public NodeStatus Tick(TInputData input)
    {
      if (_nodeOverride != null) {
        return TickNode(_nodeOverride, _lastNodeIndex, input);
      }

      if (_nodes.Count == 0) {
        var shouldSucceedIfEmpty = _rules.HasFlag(UtilityRules.InterceptFlowsFailureIfEmpty);
        return shouldSucceedIfEmpty ? NodeStatus.Success : NodeStatus.Failure;
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

        var shouldSucceedIfNoActionPassesThreshold =
          _rules.HasFlag(UtilityRules.InterceptFlowsFailureIfNoActionPassesThreshold);
        return shouldSucceedIfNoActionPassesThreshold ? NodeStatus.Success : NodeStatus.Failure;
      }

      var status = TickNode(bestNode, bestNodeIndex, input);
      return status;
    }

    /// <inheritdoc/>
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
        case NodeStatus.Failure when !_rules.HasFlag(UtilityRules.InterceptChildsFailure):
          return status;

        default:
          return NodeStatus.Success;
      }
    }
  }
}
