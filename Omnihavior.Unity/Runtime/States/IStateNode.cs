using Omnihavior.Core;

namespace Omnihavior.States
{
  /// <summary>
  /// Represents a state that can be entered and exited.
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the state operates on.</typeparam>
  public interface IState<TInputData>
  {
    /// <summary>
    /// Called when the state is entered.
    /// </summary>
    /// <param name="input">The input data for the state.</param>
    public void Enter(TInputData input) { }

    /// <summary>
    /// Called when the state is exited.
    /// </summary>
    /// <param name="input">The input data for the state.</param>
    public void Exit(TInputData input) { }
  }

  /// <summary>
  /// Represents a behavior node that can also function as a state with enter/exit lifecycle methods.
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the state node operates on.</typeparam>
  public interface IStateNode<TInputData> : IBehaviorNode<TInputData>, IState<TInputData> {}
}
