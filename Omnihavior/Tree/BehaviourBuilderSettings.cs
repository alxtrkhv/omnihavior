using System.Collections.Generic;
using Omnihavior.Tree;

// ReSharper disable once CheckNamespace
namespace Omnihavior.Core;

public partial class BehaviourBuilderSettings
{
  public SequenceRules DefaultSequenceRules { get; set; } = SequenceRules.None;
  public SelectorRules DefaultSelectorRules { get; set; } = SelectorRules.None;
  public ConditionRules DefaultConditionRules { get; set; } = ConditionRules.None;

  public int DefaultParallelFailureAllowance { get; set; } = 0;

  public NodeStatus DefaultInterceptionSuccessStatus { get; set; } = NodeStatus.Success;
  public InterceptionRules DefaultInterceptionRules { get; set; } = InterceptionRules.OnFailure;

  public int DefaultThrottleOnceInInterval { get; set; } = 2;
  public int DefaultThrottleOffset { get; set; } = 0;
  public NodeStatus DefaultThrottleStatus { get; set; } = NodeStatus.Success;
  public ThrottleRules DefaultThrottleRules { get; set; } = ThrottleRules.None;

  public int DefaultLimit { get; set; } = 1;

  public ResetRules DefaultResetRules { get; set; } = ResetRules.OnResult;

  public NodeStatus[] DefaultFakePattern { get; set; } = [NodeStatus.Success,];
}
