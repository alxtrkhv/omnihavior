using System;
using Omnihavior.Core;

namespace Omnihavior.States
{
  /// <summary>
  /// A state node implementation that uses lambda functions to define its behavior.
  /// Provides a convenient way to create states without implementing a full class.
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the state operates on.</typeparam>
  public class LambdaStateNode<TInputData> : IStateNode<TInputData>
  {
    private readonly Func<TInputData, NodeStatus>? _tick;
    private readonly Action<TInputData>? _reset;
    private readonly Action<TInputData>? _enter;
    private readonly Action<TInputData>? _exit;

    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaStateNode{TInputData}"/> class.
    /// </summary>
    /// <param name="tick">The function to execute when the state is ticked. If null, returns <see cref="NodeStatus.Success"/>.</param>
    /// <param name="enter">The action to execute when the state is entered. If null, does nothing.</param>
    /// <param name="exit">The action to execute when the state is exited. If null, does nothing.</param>
    /// <param name="reset">The action to execute when the state is reset. If null, does nothing.</param>
    public LambdaStateNode(Func<TInputData, NodeStatus>? tick = null,
      Action<TInputData>? enter = null,
      Action<TInputData>? exit = null,
      Action<TInputData>? reset = null)
    {
      _tick = tick;
      _enter = enter;
      _exit = exit;
      _reset = reset;
    }

    /// <inheritdoc/>
    public NodeStatus Tick(TInputData input)
    {
      return _tick?.Invoke(input) ?? NodeStatus.Success;
    }

    /// <inheritdoc/>
    public void Reset(TInputData input)
    {
      _reset?.Invoke(input);
    }

    /// <inheritdoc/>
    public void Enter(TInputData input)
    {
      _enter?.Invoke(input);
    }

    /// <inheritdoc/>
    public void Exit(TInputData input)
    {
      _exit?.Invoke(input);
    }
  }
}
