using System.Collections.Generic;

namespace Omnihavior.State;

public struct StateEntry<TInputData>
{
  public readonly string Key;
  public IStateNode<TInputData> Value;
  public readonly List<ITransition<TInputData>> Transitions;

  public StateEntry(string key, IStateNode<TInputData> value)
  {
    Key = key;
    Value = value;
    Transitions = [];
  }
}
