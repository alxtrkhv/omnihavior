namespace Omnihavior.States;

public interface ITransition<TKey, TInputData>
{
  public TKey? From { get; }
  public TKey To { get; }

  public bool ConditionFulfilled(TInputData input);
}
