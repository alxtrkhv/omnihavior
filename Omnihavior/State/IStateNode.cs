using Omnihavior.Core;

namespace Omnihavior.State;

public interface IState<TInputData>
{
  public void Enter(TInputData input) { }
  public void Exit(TInputData input) { }
}

public interface IStateNode<TInputData> : IBehaviorNode<TInputData>, IState<TInputData>;
