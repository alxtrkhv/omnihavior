namespace Omnihavior.State;

public class NullState<TInputData> : IStateNode<TInputData>
{
  public string Key { get; set; } = StateMachineNode<TInputData>.NullStateKey;
  public StateMachineContext<TInputData> Context { get; set; }
}
