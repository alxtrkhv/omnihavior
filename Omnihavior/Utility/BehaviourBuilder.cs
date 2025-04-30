using System;
using System.Collections.Generic;
using Omnihavior.Utility;

// ReSharper disable once CheckNamespace
namespace Omnihavior.Core;

public partial class BehaviourBuilder<TInputData>
{
  public UtilityNode<TInputData> Utility(IReadOnlyList<IEvaluatable<TInputData>> evaluations,
    IReadOnlyList<IBehaviorNode<TInputData>> nodes, UtilityRules? rules = null,
    float? minEvaluationThreshold = null, float? lastNodeBonus = null)
  {
    return new(
      evaluations,
      nodes,
      rules ?? Settings.DefaultUtilityRules,
      minEvaluationThreshold ?? Settings.DefaultUtilityMinEvaluationThreshold,
      lastNodeBonus ?? Settings.DefaultUtilityLastNodeBonus
    );
  }

  public UtilityNode<TInputData> Utility(IReadOnlyList<IEvaluatableNode<TInputData>> nodes,
    UtilityRules? rules = null,
    float? minEvaluationThreshold = null, float? lastNodeBonus = null)
  {
    return new(
      nodes,
      rules ?? Settings.DefaultUtilityRules,
      minEvaluationThreshold ?? Settings.DefaultUtilityMinEvaluationThreshold,
      lastNodeBonus ?? Settings.DefaultUtilityLastNodeBonus
    );
  }

  public LambdaEvaluatableNode<TInputData> LambdaEvaluatableNode(Func<TInputData, NodeStatus> tick,
    Func<TInputData, float> evaluate, Action<TInputData>? reset = null)
  {
    return new(tick, evaluate, reset);
  }
}
