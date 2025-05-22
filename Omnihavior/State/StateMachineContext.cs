using System;
using System.Collections.Generic;
using System.Linq;

namespace Omnihavior.State;

public readonly struct StateMachineContext<TInputData>
{
  public int Layer { get; }

  public int Index { get; }

  private readonly Dictionary<string, int[]> _stateMap;

  private readonly List<int> _state;

  public StateMachineContext(int layer, int index, Dictionary<string, int[]>? stateMap, List<int>? state = null)
  {
    Layer = layer;
    Index = index;
    _stateMap = stateMap ?? [];
    _state = state ?? [];
  }

  internal void SetState(string? key)
  {
    var state = _stateMap[key ?? StateMachineNode<TInputData>.NullStateKey];
    var minLength = Math.Min(_state.Count, state.Length);
    var defaultState = key is not null ? -1 : int.MinValue;

    for (var i = 0; i < minLength; i++) {
      _state[i] = state[i];
    }

    for (var i = minLength; i < _state.Count; i++) {
      _state[i] = defaultState;
    }
  }

  public int GetSelfState()
  {
    return _state[Layer + 1];
  }

  public void Reset()
  {
    for (var i = 0; i < _state.Count; i++) {
      _state[i] = -1;
    }
  }

  public StateMachineContext<TInputData> GetChildContext(int index)
  {
    var newLayer = Layer + 1;

    return new(newLayer, index, _stateMap, _state);
  }

  public void RegisterStateInMap(string parentKey, string childKey, int index)
  {
    _stateMap[childKey] = _stateMap[parentKey].Concat([index,]).ToArray();
  }

  public void RegisterChildLayer()
  {
    while (_state.Count <= Layer + 1) {
      _state.Add(-1);
    }
  }
}
