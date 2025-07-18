using System.Collections.Generic;

namespace Omnihavior.States;

/// <summary>
/// Represents a state definition containing the state key, node, and associated transitions.
/// </summary>
/// <typeparam name="TKey">The type used to identify states.</typeparam>
/// <typeparam name="TInputData">The type of input data the state operates on.</typeparam>
public struct StateDefinition<TKey, TInputData>
{
  /// <summary>
  /// The unique key identifying this state.
  /// </summary>
  public readonly TKey Key;

  /// <summary>
  /// The state node that implements the behavior for this state.
  /// </summary>
  public IStateNode<TInputData> Value;

  /// <summary>
  /// The list of transitions that can be triggered from this state.
  /// </summary>
  public readonly List<ITransition<TKey, TInputData>> Transitions;

  /// <summary>
  /// Initializes a new instance of the <see cref="StateDefinition{TKey, TInputData}"/> struct.
  /// </summary>
  /// <param name="key">The unique key identifying this state.</param>
  /// <param name="value">The state node that implements the behavior for this state.</param>
  public StateDefinition(TKey key, IStateNode<TInputData> value)
  {
    Key = key;
    Value = value;
    Transitions = [];
  }
}
