using System;
using Omnihavior.Core;

namespace Omnihavior.Utility;

public class LambdaEvaluatableNode<TInputData> : IEvaluatableNode<TInputData>
{
  private readonly Func<TInputData, NodeStatus> _tick;
  private readonly Func<TInputData, float> _evaluate;
  private readonly Action<TInputData>? _reset;

  public LambdaEvaluatableNode(Func<TInputData, NodeStatus> tick, Func<TInputData, float> evaluate,
    Action<TInputData>? reset = null)
  {
    _tick = tick;
    _evaluate = evaluate;
    _reset = reset;
  }

  public NodeStatus Tick(TInputData input)
  {
    return _tick(input);
  }

  public void Reset(TInputData input)
  {
    _reset?.Invoke(input);
  }

  public float Evaluate(TInputData inputData)
  {
    return _evaluate(inputData);
  }
}
