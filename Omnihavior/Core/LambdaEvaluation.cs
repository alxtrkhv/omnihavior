using System;

namespace Omnihavior.Core;

public class LambdaEvaluation<TInputData> : IEvaluatable<TInputData>
{
  private readonly Func<TInputData, float> _evaluate;

  public LambdaEvaluation(Func<TInputData, float> evaluate)
  {
    _evaluate = evaluate;
  }

  public float Evaluate(TInputData inputData)
  {
    return _evaluate(inputData);
  }
}
