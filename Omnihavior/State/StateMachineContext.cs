using System;
using System.Collections.Generic;
using System.Linq;

namespace Omnihavior.State;

public readonly struct StateMachineContext<TInputData>
{
  public StateMachineNode<TInputData> Root { get; }
  public StateMachineNode<TInputData>? Parent { get; }

  public int Layer { get; }

  public int Index { get; }

  private readonly Dictionary<string, int[]> _stateMap;

  private readonly List<int> _state;

  public StateMachineContext(StateMachineNode<TInputData> root, StateMachineNode<TInputData>? parent, int layer,
    int index, Dictionary<string, int[]>? stateMap, List<int>? state = null)
  {
    Root = root;
    Parent = parent;
    Layer = layer;
    Index = index;
    _stateMap = stateMap ?? [];
    _state = state ?? [];
  }

  internal void SetState(string? key)
  {
    var state = _stateMap[key ?? StateMachineNode<TInputData>.NullStateKey];
    var minLength = Math.Min(_state.Count, state.Length);

    for (var i = 0; i < minLength; i++) {
      _state[i] = state[i];
    }

    for (var i = minLength; i < _state.Count; i++) {
      _state[i] = -1;
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

  public StateMachineContext<TInputData> GetChildContext(StateMachineNode<TInputData> parent, int index)
  {
    var newLayer = Layer + 1;
    while (_state.Count <= newLayer) {
      _state.Add(-1);
    }

    return new(Root, parent, newLayer, index, _stateMap, _state);
  }

  public void RegisterStateInMap(StateMachineNode<TInputData> parent, IStateNode<TInputData> child, int index)
  {
    _stateMap[child.Key] = _stateMap[parent.Key].Concat([index,]).ToArray();
  }
}
