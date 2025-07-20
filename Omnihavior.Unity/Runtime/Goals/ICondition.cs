namespace Omnihavior.Goals
{
  public interface ICondition<TInputData>
  {
    public bool IsFulfilled(TInputData input);
  }
}
