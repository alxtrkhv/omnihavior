namespace Omnihavior.Core;

public enum NodeStatus
{
  Success = 0,
  Running = 1,
  Failure = 2,
  Error = 3,
}

public interface IBehaviorNode<TInputData>
{
  public NodeStatus Tick(TInputData input);
  public void Reset(TInputData input);
}
