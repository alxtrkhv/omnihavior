using Omnihavior.Core;
using Omnihavior.Tests.Tree.Mocks;
using Omnihavior.Tree;

namespace Omnihavior.Tests.Tree;

[TestFixture]
public abstract class BaseNodeTests<TNode> : BaseNodeTests<TNode, TestInput> where TNode : IBehaviorNode<TestInput>
{
  protected override TestInput CreateInputData()
  {
    return new();
  }
}

[TestFixture]
public abstract class BaseNodeTests<TNode, TInputData> where TNode : IBehaviorNode<TInputData>
{
  protected abstract TNode
    CreateNodeForResetTests(out int? childrenNumber, params IReadOnlyList<IBehaviorNode<TInputData>> children);

  protected abstract TInputData CreateInputData();

  [Test]
  [TestCase(0, Description = "Checks reset after no ticks.")]
  [TestCase(1, Description = "Checks reset after one tick.")]
  [TestCase(5, Description = "Checks reset after five ticks.")]
  public virtual void Reset_AfterNumberOfTicks_ResetsAllChildren(int tickNumber)
  {
    var childrenResets = new List<bool>();
    var children = new List<IBehaviorNode<TInputData>>();
    for (var i = 0; i < 10; i++) {
      var index = i;
      childrenResets.Add(false);
      children.Add(new LambdaNode<TInputData>(_ => NodeStatus.Success, () => childrenResets[index] = true));
    }

    var node = CreateNodeForResetTests(out var childrenCount, children);
    var data = CreateInputData();

    for (var i = 0; i < tickNumber; i++) {
      node.Tick(data);
    }

    node.Reset();

    Assert.That(node, Is.Not.Default, "Node tests should provide an instance of the node to test reset.");
    var actualChildrenCount = Math.Min(childrenCount ?? children.Count, children.Count);

    Assert.Multiple(() => {
        for (var i = 0; i < actualChildrenCount; i++) {
          Assert.That(childrenResets[i], Is.True, $"Child {i} should have been reset.");
        }
      }
    );
  }
}
