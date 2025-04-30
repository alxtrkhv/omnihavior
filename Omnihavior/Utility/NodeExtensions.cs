using System.Collections.Generic;
using Omnihavior.Utility;

// ReSharper disable once CheckNamespace
namespace Omnihavior.Core;

public static partial class NodeExtensions
{
  public static UtilityNode<TInputData> AsUtility<TInputData>(this IReadOnlyList<IBehaviorNode<TInputData>> nodes,
    IReadOnlyList<IEvaluatable<TInputData>> evaluations, UtilityRules? rules = null,
    float? minEvaluationThreshold = null, float? lastNodeBonus = null)
  {
    return Builder.Default<TInputData>().Utility(
      evaluations,
      nodes,
      rules,
      minEvaluationThreshold,
      lastNodeBonus
    );
  }

  public static UtilityNode<TInputData> AsUtility<TInputData>(this IReadOnlyList<IEvaluatableNode<TInputData>> nodes,
    UtilityRules? rules = null, float? minEvaluationThreshold = null, float? lastNodeBonus = null)
  {
    return Builder.Default<TInputData>().Utility(
      nodes,
      rules,
      minEvaluationThreshold,
      lastNodeBonus
    );
  }
}
