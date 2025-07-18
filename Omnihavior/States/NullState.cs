namespace Omnihavior.States;

/// <summary>
/// A null state implementation that does nothing and always returns <see cref="Omnihavior.Core.NodeStatus.Success"/>.
/// Used as a placeholder when no valid state is set in a state machine.
/// </summary>
/// <typeparam name="TInputData">The type of input data the state operates on.</typeparam>
public class NullState<TInputData> : IStateNode<TInputData>;
