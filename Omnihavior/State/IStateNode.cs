using Omnihavior.Core;

namespace Omnihavior.State;

public interface IState<TInputData>
{
  public string Key { get; }
  public StateMachineContext<TInputData> Context { get; set; }

  public void Enter(TInputData input) { }
  public void Exit(TInputData input) { }
}

public interface IStateNode<TInputData> : IBehaviorNode<TInputData>, IState<TInputData>;
