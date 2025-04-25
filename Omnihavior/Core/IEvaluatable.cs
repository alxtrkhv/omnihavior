namespace Omnihavior.Core;

public interface IEvaluatable<TInputData>
{
  float Evaluate(TInputData inputData);
}
