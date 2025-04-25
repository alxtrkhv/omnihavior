using Omnihavior.Utility;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Omnihavior.Core;

public partial struct BehaviourBuilder<TInputData>
{
  public static IBehaviorNode<TInputData> UtilityAi(IReadOnlyList<IEvaluatable<TInputData>> evaluations,
    IReadOnlyList<IBehaviorNode<TInputData>> nodes, UtilityRules rules = UtilityRules.None,
    float minEvaluationThreshold = float.MinValue, float lastNodeBonus = 0f)
  {
    return new UtilityNode<TInputData>(evaluations, nodes, rules, minEvaluationThreshold, lastNodeBonus);
  }

  public static IBehaviorNode<TInputData> UtilityAi(IReadOnlyList<IEvaluatableNode<TInputData>> nodes,
    UtilityRules rules = UtilityRules.None, float minEvaluationThreshold = float.MinValue, float lastNodeBonus = 0f)
  {
    return new UtilityNode<TInputData>(nodes, rules, minEvaluationThreshold, lastNodeBonus);
  }
}
