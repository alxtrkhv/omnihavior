using System;

namespace Omnihavior.Core;

/// <summary>
/// An implementation of <see cref="IEvaluatable{TInputData}"/> that uses a lambda function for evaluation.
/// </summary>
/// <typeparam name="TInputData">The type of input data used for evaluation.</typeparam>
public class LambdaEvaluation<TInputData> : IEvaluatable<TInputData>
{
  private readonly Func<TInputData, float> _evaluate;

  /// <summary>
  /// Initializes a new instance of the <see cref="LambdaEvaluation{TInputData}"/> class.
  /// </summary>
  /// <param name="evaluate">The lambda function used for evaluation.</param>
  public LambdaEvaluation(Func<TInputData, float> evaluate)
  {
    _evaluate = evaluate;
  }

  /// <inheritdoc/>
  public float Evaluate(TInputData inputData)
  {
    return _evaluate(inputData);
  }
}
