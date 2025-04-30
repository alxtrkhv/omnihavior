using System;
using System.Collections.Generic;
using Omnihavior.Tree;

// ReSharper disable once CheckNamespace
namespace Omnihavior.Core;

public partial class BehaviourBuilder<TInputData>
{
  public SequenceNode<TInputData> Sequence(params IBehaviorNode<TInputData>[] children)
  {
    return new(children, Settings.DefaultSequenceRules);
  }

  public SequenceNode<TInputData> Sequence(IReadOnlyList<IBehaviorNode<TInputData>> children, SequenceRules? rules)
  {
    return new(children, rules ?? Settings.DefaultSequenceRules);
  }

  public SequenceNode<TInputData> And(IBehaviorNode<TInputData> firstNode,
    IBehaviorNode<TInputData> secondNode, SequenceRules? rules = null)
  {
    return Sequence([firstNode, secondNode,], rules);
  }

  public SelectorNode<TInputData> Selector(params IBehaviorNode<TInputData>[] children)
  {
    return new(children, Settings.DefaultSelectorRules);
  }

  public SelectorNode<TInputData> Selector(IReadOnlyList<IBehaviorNode<TInputData>> children, SelectorRules? rules)
  {
    return new(children, rules ?? Settings.DefaultSelectorRules);
  }

  public SelectorNode<TInputData> Or(IBehaviorNode<TInputData> firstNode,
    IBehaviorNode<TInputData> secondNode, SelectorRules? rules = null)
  {
    return Selector([firstNode, secondNode,], rules);
  }

  public ParallelNode<TInputData> Parallel(params IBehaviorNode<TInputData>[] children)
  {
    return new(children, Settings.DefaultParallelFailureAllowance);
  }

  public ParallelNode<TInputData> Parallel(IReadOnlyList<IBehaviorNode<TInputData>> children, int? failureAllowance)
  {
    return new(children, failureAllowance ?? Settings.DefaultParallelFailureAllowance);
  }

  public ConditionalNode<TInputData> Conditional(IBehaviorNode<TInputData> condition,
    IBehaviorNode<TInputData> positiveNode, IBehaviorNode<TInputData>? negativeNode = null,
    ConditionRules? rules = null)
  {
    return new(condition, positiveNode, negativeNode, rules ?? Settings.DefaultConditionRules);
  }

  public ConditionalNode<TInputData> If(IBehaviorNode<TInputData> condition,
    IBehaviorNode<TInputData> positiveNode, IBehaviorNode<TInputData>? negativeNode = null,
    ConditionRules? rules = null)
  {
    return Conditional(condition, positiveNode, negativeNode, rules);
  }

  public InterceptorNode<TInputData> Interceptor(IBehaviorNode<TInputData> child, InterceptionRules? rules = null,
    NodeStatus? successStatus = NodeStatus.Success)
  {
    return new(
      child,
      rules ?? Settings.DefaultInterceptionRules,
      successStatus ?? Settings.DefaultInterceptionSuccessStatus
    );
  }

  public InverterNode<TInputData> Inverter(IBehaviorNode<TInputData> child)
  {
    return new(child);
  }

  public ThrottleNode<TInputData> Throttle(IBehaviorNode<TInputData> child, int? runOnceInInterval = null,
    NodeStatus? status = null, ThrottleRules? rules = null, int? offset = null)
  {
    return new(
      child,
      runOnceInInterval ?? Settings.DefaultThrottleOnceInInterval,
      status ?? Settings.DefaultThrottleStatus,
      rules ?? Settings.DefaultThrottleRules,
      offset ?? Settings.DefaultThrottleOffset
    );
  }

  public LimitNode<TInputData> Limit(IBehaviorNode<TInputData> child, int? limit = null)
  {
    return new(child, limit ?? Settings.DefaultLimit);
  }

  public ResetterNode<TInputData> Resetter(IBehaviorNode<TInputData> child, ResetRules? rules = null)
  {
    return new(child, rules ?? Settings.DefaultResetRules);
  }

  public FakeNode<TInputData> Fake(params NodeStatus[] pattern)
  {
    return new(pattern.Length > 0 ? pattern : Settings.DefaultFakePattern);
  }
}
