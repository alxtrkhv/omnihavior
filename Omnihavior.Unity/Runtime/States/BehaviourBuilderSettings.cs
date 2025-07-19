using Omnihavior.States;

// ReSharper disable once CheckNamespace
namespace Omnihavior.Core
{
  public partial class BehaviourBuilderSettings
  {
    /// <summary>
    /// Gets or sets the default rules for state machine nodes.
    /// </summary>
    public StateMachineRules DefaultStateMachineRules { get; set; } = StateMachineRules.None;
  }
}
