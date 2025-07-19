using Omnihavior.Core;

namespace Omnihavior.Utility
{
  /// <summary>
  /// Represents a behavior node that can also be evaluated for utility scoring.
  /// Combines the functionality of <see cref="IBehaviorNode{TInputData}"/> and <see cref="IEvaluatable{TInputData}"/>.
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the node operates on.</typeparam>
  public interface IEvaluatableNode<TInputData> : IBehaviorNode<TInputData>, IEvaluatable<TInputData> {}
}
