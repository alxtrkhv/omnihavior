using System.Collections.Generic;

namespace Omnihavior.State;

public struct StateDefinition<TInputData>
{
  public readonly string Key;
  public IStateNode<TInputData> Value;
  public readonly List<ITransition<TInputData>> Transitions;

  public StateDefinition(string key, IStateNode<TInputData> value)
  {
    Key = key;
    Value = value;
    Transitions = [];
  }
}
