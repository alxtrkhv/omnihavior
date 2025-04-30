using System;

namespace Omnihavior.Core;

/// <summary>
/// A behavior node that executes a lambda function for its tick logic.
/// </summary>
/// <typeparam name="TInputData">The type of input data the node operates on.</typeparam>
public class LambdaNode<TInputData> : IBehaviorNode<TInputData>
{
  private readonly Func<TInputData, NodeStatus> _lambda;
  private readonly Action<TInputData>? _reset;

  /// <summary>
  /// Initializes a new instance of the <see cref="LambdaNode{TInputData}"/> class.
  /// </summary>
  /// <param name="lambda">The function to execute when the node is ticked.</param>
  /// <param name="reset">An optional function to execute when the node is reset.</param>
  public LambdaNode(Func<TInputData, NodeStatus> lambda, Action<TInputData>? reset = null)
  {
    _lambda = lambda;
    _reset = reset;
  }

  /// <inheritdoc/>
  public NodeStatus Tick(TInputData input)
  {
    return _lambda(input);
  }

  /// <inheritdoc/>
  public void Reset(TInputData input)
  {
    _reset?.Invoke(input);
  }
}
