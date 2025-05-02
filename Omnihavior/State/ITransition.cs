namespace Omnihavior.State;

public interface ITransition<TInputData>
{
  public string? From { get; }
  public string To { get; }

  public bool ConditionFulfilled(TInputData input);
}
