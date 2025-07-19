using Omnihavior.Utility;

// ReSharper disable once CheckNamespace
namespace Omnihavior.Core
{
  public partial class BehaviourBuilderSettings
  {
    public float DefaultUtilityMinEvaluationThreshold { get; set; } = float.MinValue;
    public float DefaultUtilityLastNodeBonus { get; set; } = 0f;
    public UtilityRules DefaultUtilityRules { get; set; } = UtilityRules.None;
  }
}
