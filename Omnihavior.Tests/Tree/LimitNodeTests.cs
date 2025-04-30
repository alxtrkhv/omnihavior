using NSubstitute;
using Omnihavior.Core;
using Omnihavior.Tests.Mocks;
using Omnihavior.Tree;

namespace Omnihavior.Tests.Tree;

public class LimitNodeTests : BaseNodeTests<LimitNode<TestInput>>
{
  protected override LimitNode<TestInput> CreateNodeForResetTests(out int? childrenNumber,
    params IBehaviorNode<TestInput>[] children)
  {
    childrenNumber = 1;
    return new(children[0], 1);
  }

  [Test]
  [TestCase(5, 0, 1)]
  [TestCase(2, 1, 1)]
  [TestCase(3, 2, 2)]
  [TestCase(4, 2, 2)]
  [TestCase(5, 3, 3)]
  [TestCase(10, 5, 5)]
  public void Tick_AfterLimit_WontTickChild(int runs, int limit, int expected)
  {
    var child = Substitute.For<IBehaviorNode<TestInput>>();
    var node = new LimitNode<TestInput>(child, limit);
    var input = new TestInput();

    for (var i = 0; i < runs; i++) {
      node.Tick(input);
    }

    child.Received(expected).Tick(Arg.Any<TestInput>());
  }

  [Test]
  public void Tick_AfterLimit_ReturnsLastCachedStatus()
  {
    var child = Substitute.For<IBehaviorNode<TestInput>>();
    var node = new LimitNode<TestInput>(child, 3);
    var input = new TestInput();

    child.Tick(Arg.Any<TestInput>()).Returns(NodeStatus.Success);
    var firstResult = node.Tick(input);

    child.Tick(Arg.Any<TestInput>()).Returns(NodeStatus.Failure);
    var secondResult = node.Tick(input);

    child.Tick(Arg.Any<TestInput>()).Returns(NodeStatus.Success);
    var thirdResult = node.Tick(input);

    child.Tick(Arg.Any<TestInput>()).Returns(NodeStatus.Failure);
    var fourthResult = node.Tick(input);

    Assert.Multiple(() => {
        Assert.That(firstResult, Is.EqualTo(NodeStatus.Success));
        Assert.That(secondResult, Is.EqualTo(NodeStatus.Failure));
        Assert.That(thirdResult, Is.EqualTo(NodeStatus.Success));
        Assert.That(fourthResult, Is.EqualTo(NodeStatus.Success));
      }
    );
  }

  [Test]
  [TestCase(NodeStatus.Running)]
  [TestCase(NodeStatus.Error)]
  public void Tick_NotRunningOrFailureBeforeLimit_WontUseLimit(NodeStatus status)
  {
    var child = Substitute.For<IBehaviorNode<TestInput>>();
    var node = new LimitNode<TestInput>(child, 3);
    var input = new TestInput();

    child.Tick(Arg.Any<TestInput>()).Returns(status);

    node.Tick(input);
    node.Tick(input);
    node.Tick(input);
    node.Tick(input);

    child.Tick(Arg.Any<TestInput>()).Returns(NodeStatus.Success);
    var result = node.Tick(input);

    Assert.That(result, Is.EqualTo(NodeStatus.Success));
    child.Received(5).Tick(Arg.Any<TestInput>());
  }
}
