using Omnihavior.States;

// ReSharper disable once CheckNamespace
namespace Omnihavior.Core;

public partial class BehaviourBuilderSettings
{
  public StateMachineRules DefaultStateMachineRules { get; set; } = StateMachineRules.None;
}
