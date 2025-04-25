using System;
using System.Collections.Generic;
using Omnihavior.Tree;

// ReSharper disable once CheckNamespace
namespace Omnihavior.Core;

public partial struct BehaviourBuilder<TInputData>
{
  public IBehaviorNode<TInputData> Sequence(params IReadOnlyList<IBehaviorNode<TInputData>> children)
  {
    return new SequenceNode<TInputData>(children);
  }

  public IBehaviorNode<TInputData> Selector(params IReadOnlyList<IBehaviorNode<TInputData>> children)
  {
    return new SelectorNode<TInputData>(children);
  }

  public IBehaviorNode<TInputData> Parallel(params IReadOnlyList<IBehaviorNode<TInputData>> children)
  {
    return new ParallelNode<TInputData>(children);
  }

  public IBehaviorNode<TInputData> Lambda(Func<TInputData, NodeStatus> action, Action<TInputData>? reset = null)
  {
    return new LambdaNode<TInputData>(action, reset);
  }

  public IBehaviorNode<TInputData> Inverter(IBehaviorNode<TInputData> child)
  {
    return new InverterNode<TInputData>(child);
  }

  public IBehaviorNode<TInputData> Interceptor(IBehaviorNode<TInputData> child,
    InterceptionRules rules = InterceptionRules.OnFailure)
  {
    return new InterceptorNode<TInputData>(child, rules);
  }

  public IBehaviorNode<TInputData> Resetter(IBehaviorNode<TInputData> child,
    ResetRules rules = ResetRules.Always)
  {
    return new ResetterNode<TInputData>(child, rules);
  }

  public IBehaviorNode<TInputData> If(IBehaviorNode<TInputData> condition,
    IBehaviorNode<TInputData> positiveBody, IBehaviorNode<TInputData>? negativeBody = null,
    ConditionRules rules = ConditionRules.None
  )
  {
    return new ConditionalNode<TInputData>(condition, positiveBody, negativeBody, rules);
  }

  public IBehaviorNode<TInputData> And(IBehaviorNode<TInputData> first, IBehaviorNode<TInputData> second)
  {
    return new SequenceNode<TInputData>(first, second);
  }

  public IBehaviorNode<TInputData> Or(IBehaviorNode<TInputData> first, IBehaviorNode<TInputData> second)
  {
    return new SelectorNode<TInputData>(first, second);
  }
}
