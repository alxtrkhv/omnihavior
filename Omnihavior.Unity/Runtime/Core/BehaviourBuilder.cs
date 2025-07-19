using System;

namespace Omnihavior.Core
{
  /// <summary>
  /// Base interface for behavior builders.
  /// </summary>
  public interface IBehaviourBuilder
  {
    /// <summary>
    /// Gets the type of input data the behavior nodes built by this builder will operate on.
    /// </summary>
    public Type InputType { get; }

    public BehaviourBuilderSettings Settings { get; }
  }

  /// <summary>
  /// Provides a way to build behaviors conveniently with default parameters and implicit input type.
  /// This class uses partial definitions to separate builder methods for different node types (e.g., Tree, Utility).
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the behavior nodes will operate on.</typeparam>
  public partial class BehaviourBuilder<TInputData> : IBehaviourBuilder
  {
    /// <inheritdoc/>
    public Type InputType => typeof(TInputData);

    /// <inheritdoc/>
    public BehaviourBuilderSettings Settings { get; } = new();

    public LambdaNode<TInputData> Lambda(Func<TInputData, NodeStatus> lambda, Action<TInputData>? reset = null)
    {
      return new(lambda, reset);
    }

    public LambdaEvaluation<TInputData> LambdaEvaluation(Func<TInputData, float> evaluate)
    {
      return new(evaluate);
    }
  }
}
