namespace Omnihavior.Core
{
  /// <summary>
  /// Represents the possible execution states of a behavior node.
  /// </summary>
  public enum NodeStatus
  {
    /// <summary>
    /// The node completed its execution successfully.
    /// </summary>
    Success = 0,

    /// <summary>
    /// The node completed its execution unsuccessfully.
    /// </summary>
    Failure = 1,

    /// <summary>
    /// The node is still executing and requires further ticks.
    /// </summary>
    Running = 2,

    /// <summary>
    /// The node encountered an error during execution.
    /// </summary>
    Error = 3,
  }

  /// <summary>
  /// Represents a node within a behavior tree.
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the node operates on.</typeparam>
  public interface IBehaviorNode<TInputData>
  {
    /// <summary>
    /// Executes the behavior associated with this node.
    /// </summary>
    /// <param name="input">The input data for the node's execution.</param>
    /// <returns>The execution status of the node (<see cref="NodeStatus"/>).</returns>
    public NodeStatus Tick(TInputData input) => NodeStatus.Success;

    /// <summary>
    /// Resets the internal state of the node.
    /// </summary>
    /// <param name="input">The input data, potentially used for state reset logic.</param>
    public void Reset(TInputData input) { }
  }
}
