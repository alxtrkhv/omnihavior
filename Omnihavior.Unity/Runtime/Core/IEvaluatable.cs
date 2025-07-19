namespace Omnihavior.Core
{
  /// <summary>
  /// Represents an object that can be evaluated to produce a numerical score, typically used in utility systems.
  /// </summary>
  /// <typeparam name="TInputData">The type of input data used for evaluation.</typeparam>
  public interface IEvaluatable<TInputData>
  {
    /// <summary>
    /// Evaluates the object based on the provided input data.
    /// </summary>
    /// <param name="inputData">The input data used for evaluation.</param>
    /// <returns>A float representing the evaluation score.</returns>
    float Evaluate(TInputData inputData);
  }
}
