using System;

namespace Omnihavior.States
{
  /// <summary>
  /// A transition implementation that uses a lambda function to define its condition.
  /// Provides a convenient way to create transitions without implementing a full class.
  /// </summary>
  /// <typeparam name="TKey">The type used to identify states.</typeparam>
  /// <typeparam name="TInputData">The type of input data the transition operates on.</typeparam>
  public class LambdaTransition<TKey, TInputData> : ITransition<TKey, TInputData>
  {
    private readonly Func<TInputData, bool>? _condition;

    /// <inheritdoc/>
    public TKey? From { get; }

    /// <inheritdoc/>
    public TKey To { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaTransition{TKey, TInputData}"/> class.
    /// </summary>
    /// <param name="from">The source state key. If null, this becomes a global transition.</param>
    /// <param name="to">The target state key to transition to.</param>
    /// <param name="condition">The function to evaluate the transition condition. If null, the transition is always triggered.</param>
    public LambdaTransition(TKey? from, TKey to, Func<TInputData, bool>? condition = null)
    {
      From = from;
      To = to;
      _condition = condition;
    }

    /// <inheritdoc/>
    public bool ConditionFulfilled(TInputData input)
    {
      return _condition?.Invoke(input) ?? true;
    }
  }
}
