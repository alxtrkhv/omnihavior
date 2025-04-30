using NSubstitute;
using Omnihavior.Core;
using Omnihavior.Tests.Mocks;
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
    CreateNodeForResetTests(out int? childrenNumber, params IBehaviorNode<TInputData>[] children);

  protected abstract TInputData CreateInputData();

  [Test]
  [TestCase(0, Description = "Checks reset after no ticks.")]
  [TestCase(1, Description = "Checks reset after one tick.")]
  [TestCase(5, Description = "Checks reset after five ticks.")]
  [TestCase(10, Description = "Checks reset after ten ticks.")]
  public virtual void Reset_AfterNumberOfTicks_ResetsAllChildren(int tickNumber)
  {
    var childrenNumber = 10;
    var children = new IBehaviorNode<TInputData>[childrenNumber];
    for (var i = 0; i < childrenNumber; i++) {
      children[i] = Substitute.For<IBehaviorNode<TInputData>>();
    }

    var node = CreateNodeForResetTests(out var childrenCount, children);
    var data = CreateInputData();

    for (var i = 0; i < tickNumber; i++) {
      node.Tick(data);
    }

    node.Reset(data);

    Assert.That(node, Is.Not.Default, "Node tests should provide an instance of the node to test reset.");
    var actualChildrenCount = Math.Min(childrenCount ?? children.Length, children.Length);

    Assert.Multiple(() => {
        for (var i = 0; i < actualChildrenCount; i++) {
          children[i].Received().Reset(data);
        }
      }
    );
  }
}
