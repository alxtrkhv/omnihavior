namespace Omnihavior.States
{
  /// <summary>
  /// Represents a transition between states in a state machine.
  /// </summary>
  /// <typeparam name="TKey">The type used to identify states.</typeparam>
  /// <typeparam name="TInputData">The type of input data the transition operates on.</typeparam>
  public interface ITransition<TKey, TInputData>
  {
    /// <summary>
    /// Gets the source state key. If null, this is a global transition that can be triggered from any state.
    /// </summary>
    public TKey? From { get; }

    /// <summary>
    /// Gets the target state key to transition to.
    /// </summary>
    public TKey To { get; }

    /// <summary>
    /// Determines whether the transition condition is fulfilled.
    /// </summary>
    /// <param name="input">The input data to evaluate the condition against.</param>
    /// <returns>True if the transition should be triggered; otherwise, false.</returns>
    public bool ConditionFulfilled(TInputData input);
  }
}
