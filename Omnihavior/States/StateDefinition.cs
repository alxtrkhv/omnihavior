using System.Collections.Generic;

namespace Omnihavior.States;

public struct StateDefinition<TKey, TInputData>
{
  public readonly TKey Key;
  public IStateNode<TInputData> Value;
  public readonly List<ITransition<TKey, TInputData>> Transitions;

  public StateDefinition(TKey key, IStateNode<TInputData> value)
  {
    Key = key;
    Value = value;
    Transitions = [];
  }
}
