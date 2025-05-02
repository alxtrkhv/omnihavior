namespace Omnihavior.State;

public class NullState<TInputData> : IStateNode<TInputData>
{
  public string Key => "Null";
  public StateMachineContext<TInputData> Context { get; set; }
}
