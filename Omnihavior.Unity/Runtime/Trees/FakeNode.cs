using System.Collections.Generic;
using Omnihavior.Core;

namespace Omnihavior.Trees
{
  /// <summary>
  /// A node that cycles through a predefined sequence of return statuses.
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the node operates on (ignored).</typeparam>
  public class FakeNode<TInputData> : IBehaviorNode<TInputData>
  {
    private readonly IReadOnlyList<NodeStatus> _children;

    private int _counter;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeNode{TInputData}"/> class that always returns Success.
    /// </summary>
    public FakeNode() : this(NodeStatus.Success) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeNode{TInputData}"/> class with a specific sequence of statuses.
    /// </summary>
    /// <param name="children">The sequence of <see cref="NodeStatus"/> to return on successive ticks.</param>
    public FakeNode(params NodeStatus[] children)
    {
      _children = children;
    }

    /// <inheritdoc/>
    public NodeStatus Tick(TInputData input)
    {
      var status = _children[_counter];

      _counter = (_counter + 1) % _children.Count;

      return status;
    }

    /// <inheritdoc/>
    public void Reset(TInputData input)
    {
      _counter = 0;
    }
  }
}
