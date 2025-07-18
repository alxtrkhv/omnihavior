using System;

namespace Omnihavior.States;

public class LambdaTransition<TKey, TInputData> : ITransition<TKey, TInputData>
{
  private readonly Func<TInputData, bool>? _condition;
  public TKey? From { get; }
  public TKey To { get; }

  public LambdaTransition(TKey? from, TKey to, Func<TInputData, bool>? condition = null)
  {
    From = from;
    To = to;
    _condition = condition;
  }

  public bool ConditionFulfilled(TInputData input)
  {
    return _condition?.Invoke(input) ?? true;
  }
}
