using System.Collections.Generic;
using Omnihavior.Tree;

// ReSharper disable once CheckNamespace
namespace Omnihavior.Core;

public static partial class NodeExtensions
{
  public static SequenceNode<TInputData> AsSequence<TInputData>(this IReadOnlyList<IBehaviorNode<TInputData>> children,
    SequenceRules? rules = null)
  {
    return Builder.Default<TInputData>().Sequence(children, rules);
  }

  public static SequenceNode<TInputData> And<TInputData>(this IBehaviorNode<TInputData> firstNode,
    IBehaviorNode<TInputData> secondNode, SequenceRules? rules = null)
  {
    return Builder.Default<TInputData>().Sequence([firstNode, secondNode,], rules);
  }

  public static SelectorNode<TInputData> AsSelector<TInputData>(this IReadOnlyList<IBehaviorNode<TInputData>> children,
    SelectorRules? rules = null)
  {
    return Builder.Default<TInputData>().Selector(children, rules);
  }

  public static SelectorNode<TInputData> Or<TInputData>(this IBehaviorNode<TInputData> firstNode,
    IBehaviorNode<TInputData> secondNode, SelectorRules? rules = null)
  {
    return Builder.Default<TInputData>().Selector([firstNode, secondNode,], rules);
  }

  public static ParallelNode<TInputData> InParallel<TInputData>(this IReadOnlyList<IBehaviorNode<TInputData>> children,
    int? failureAllowance = null)
  {
    return Builder.Default<TInputData>().Parallel(children, failureAllowance);
  }

  public static ConditionalNode<TInputData> AsCondition<TInputData>(this IBehaviorNode<TInputData> condition,
    IBehaviorNode<TInputData> positiveNode, IBehaviorNode<TInputData>? negativeNode = null,
    ConditionRules? rules = null)
  {
    return Builder.Default<TInputData>().Conditional(condition, positiveNode, negativeNode, rules);
  }

  public static InterceptorNode<TInputData> Intercept<TInputData>(this IBehaviorNode<TInputData> condition,
    InterceptionRules? rules = null, NodeStatus? status = null)
  {
    return Builder.Default<TInputData>().Interceptor(
      condition,
      rules,
      status
    );
  }

  public static InverterNode<TInputData> Invert<TInputData>(this IBehaviorNode<TInputData> child)
  {
    return Builder.Default<TInputData>().Inverter(child);
  }

  public static ThrottleNode<TInputData> Throttle<TInputData>(this IBehaviorNode<TInputData> child,
    int? runOnceInInterval = null, NodeStatus? status = null, ThrottleRules? rules = null, int? offset = null)
  {
    return Builder.Default<TInputData>().Throttle(
      child,
      runOnceInInterval,
      status,
      rules,
      offset
    );
  }

  public static LimitNode<TInputData> WithLimit<TInputData>(this IBehaviorNode<TInputData> child, int? limit = null)
  {
    return Builder.Default<TInputData>().Limit(child, limit);
  }

  public static LimitNode<TInputData> Once<TInputData>(this IBehaviorNode<TInputData> child)
  {
    return Builder.Default<TInputData>().Limit(child, 1);
  }

  public static ResetterNode<TInputData> Reset<TInputData>(this IBehaviorNode<TInputData> child,
    ResetRules? rules = null)
  {
    return Builder.Default<TInputData>().Resetter(child, rules);
  }

  public static FakeNode<TInputData> SubstituteWithFake<TInputData>(this IBehaviorNode<TInputData> _,
    NodeStatus[]? pattern = null)
  {
    return Builder.Default<TInputData>().Fake(pattern ?? []);
  }

  public static FakeNode<TInputData> SubstituteWithFake<TInputData>(this IBehaviorNode<TInputData> _,
    NodeStatus? status = null)
  {
    return Builder.Default<TInputData>().Fake(status is not null ? [status.Value,] : []);
  }
}
