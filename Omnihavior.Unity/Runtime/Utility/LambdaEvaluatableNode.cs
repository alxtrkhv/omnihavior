using System;
using Omnihavior.Core;

namespace Omnihavior.Utility
{
  /// <summary>
  /// An evaluatable behavior node that uses lambda functions for its core logic.
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the node operates on.</typeparam>
  public class LambdaEvaluatableNode<TInputData> : IEvaluatableNode<TInputData>
  {
    private readonly Func<TInputData, NodeStatus> _tick;
    private readonly Func<TInputData, float> _evaluate;
    private readonly Action<TInputData>? _reset;

    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaEvaluatableNode{TInputData}"/> class.
    /// </summary>
    /// <param name="tick">The function to execute when the node is ticked.</param>
    /// <param name="evaluate">The function to execute to evaluate the node's utility.</param>
    /// <param name="reset">An optional function to execute when the node is reset.</param>
    public LambdaEvaluatableNode(Func<TInputData, NodeStatus> tick, Func<TInputData, float> evaluate,
      Action<TInputData>? reset = null)
    {
      _tick = tick;
      _evaluate = evaluate;
      _reset = reset;
    }

    /// <inheritdoc/>
    public NodeStatus Tick(TInputData input)
    {
      return _tick(input);
    }

    /// <inheritdoc/>
    public void Reset(TInputData input)
    {
      _reset?.Invoke(input);
    }

    /// <inheritdoc/>
    public float Evaluate(TInputData inputData)
    {
      return _evaluate(inputData);
    }
  }
}
