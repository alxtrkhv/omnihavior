using System;

namespace Omnihavior.State;

public class LambdaTransition<TInputData> : ITransition<TInputData>
{
  private readonly Func<TInputData, bool>? _condition;
  public string? From { get; }
  public string To { get; }

  public LambdaTransition(string? from, string to, Func<TInputData, bool>? condition = null)
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
