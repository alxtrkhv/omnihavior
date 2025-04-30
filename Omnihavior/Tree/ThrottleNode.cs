using System;
using Omnihavior.Core;

namespace Omnihavior.Tree;

/// <summary>
/// Defines rules for customizing the behavior of a <see cref="ThrottleNode{TInputData}"/>.
/// </summary>
[Flags]
public enum ThrottleRules
{
  /// <summary>
  /// Default behavior.
  /// </summary>
  None = 0,
  /// <summary>
  /// If set, the status returned by the child on its last actual run is cached and returned during throttled ticks.
  /// Otherwise, the default cached status provided in the constructor is used.
  /// </summary>
  CacheLastRunResult = 1 << 0,
}

/// <summary>
/// A decorator node that allows its child node to run only once every N ticks.
/// On other ticks, it returns a cached status.
/// </summary>
/// <typeparam name="TInputData">The type of input data the node operates on.</typeparam>
public class ThrottleNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly IBehaviorNode<TInputData> _child;
  private readonly ThrottleRules _rules;
  private readonly int _onceIn;
  private readonly int _offset;

  private int _counter;
  private NodeStatus _cachedStatus;

  /// <summary>
  /// Initializes a new instance of the <see cref="ThrottleNode{TInputData}"/> class.
  /// </summary>
  /// <param name="child">The child node to throttle.</param>
  /// <param name="runOnceInInterval">The frequency of execution (e.g., 5 means run once every 5 ticks).</param>
  /// <param name="cachedStatus">The status to return on ticks when the child is not executed. Defaults to Success.</param>
  /// <param name="rules">The rules governing the throttle's behavior.</param>
  /// <param name="offset">An initial offset for the counter, allowing synchronization between multiple throttles.</param>
  public ThrottleNode(IBehaviorNode<TInputData> child, int runOnceInInterval, NodeStatus cachedStatus = NodeStatus.Success,
    ThrottleRules rules = ThrottleRules.None,
    int offset = 0)
  {
    _child = child;
    _rules = rules;
    _onceIn = Math.Max(runOnceInInterval, 1);
    _cachedStatus = cachedStatus;
    _offset = Math.Max(offset, 0) % _onceIn;
    _counter = _offset;
  }

  /// <inheritdoc/>
  public NodeStatus Tick(TInputData input)
  {
    if (_counter != 0) {
      IncrementCounter();
      return _cachedStatus;
    }

    var status = _child.Tick(input);
    switch (status) {
      case NodeStatus.Running:
      case NodeStatus.Error:
        return status;
    }

    var shouldCacheChildsStatus = _rules.HasFlag(ThrottleRules.CacheLastRunResult);
    if (shouldCacheChildsStatus) {
      _cachedStatus = status;
    }

    IncrementCounter();
    return status;
  }

  /// <inheritdoc/>
  public void Reset(TInputData input)
  {
    _counter = _offset;
    _child.Reset(input);
  }

  private void IncrementCounter()
  {
    _counter = (_counter + 1) % _onceIn;
  }
}
