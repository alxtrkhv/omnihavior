namespace Omnihavior.Goals
{
  public interface IEffect<TInputData>
  {
    public void Apply(TInputData input);
  }
}
