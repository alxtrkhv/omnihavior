using NSubstitute;
using Omnihavior.Core;
using Omnihavior.Tests.Mocks;
using Omnihavior.Tree;

namespace Omnihavior.Tests.Tree;

public class ThrottleNodeTests : BaseNodeTests<ThrottleNode<TestInput>>
{
  protected override ThrottleNode<TestInput> CreateNodeForResetTests(out int? childrenNumber,
    params IReadOnlyList<IBehaviorNode<TestInput>> children)
  {
    childrenNumber = 1;
    return new(children[0], 1, 0);
  }

  [Test]
  [TestCase(1, 0, 5, 5)]
  [TestCase(2, 0, 5, 3)]
  [TestCase(3, 0, 10, 4)]
  [TestCase(5, 0, 9, 2)]
  public void Tick_WithoutOffset_TicksChildCorrectNumberOfTimes(int onceIn, int offset, int runs, int expectedTicks)
  {
    var child = Substitute.For<IBehaviorNode<TestInput>>();
    var node = new ThrottleNode<TestInput>(child, onceIn, offset);
    var input = new TestInput();

    for (var i = 0; i < runs; i++) {
      node.Tick(input);
    }

    child.Received(expectedTicks).Tick(Arg.Any<TestInput>());
  }

  [Test]
  [TestCase(2, 1, 5, 2)]
  [TestCase(3, 1, 10, 3)]
  [TestCase(3, 2, 10, 3)]
  [TestCase(5, 3, 14, 3)]
  public void Tick_WithOffset_TicksChildCorrectNumberOfTimes(int onceIn, int offset, int runs, int expectedTicks)
  {
    var child = Substitute.For<IBehaviorNode<TestInput>>();
    var node = new ThrottleNode<TestInput>(child, onceIn, offset);
    var input = new TestInput();

    for (var i = 0; i < runs; i++) {
      node.Tick(input);
    }

    child.Received(expectedTicks).Tick(Arg.Any<TestInput>());
  }

  [Test]
  public void Tick_WhenThrottled_ReturnsSuccess()
  {
    var child = Substitute.For<IBehaviorNode<TestInput>>();
    var node = new ThrottleNode<TestInput>(child, 3, 0);
    var input = new TestInput();

    child.Tick(Arg.Any<TestInput>()).Returns(NodeStatus.Failure);

    var status1 = node.Tick(input);
    var status2 = node.Tick(input);
    var status3 = node.Tick(input);
    var status4 = node.Tick(input);

    Assert.Multiple(() => {
        Assert.That(status1, Is.EqualTo(NodeStatus.Failure));
        Assert.That(status2, Is.EqualTo(NodeStatus.Success));
        Assert.That(status3, Is.EqualTo(NodeStatus.Success));
        Assert.That(status4, Is.EqualTo(NodeStatus.Failure));
      }
    );
  }

  [Test]
  public void Tick_WhenChildTicked_ReturnsChildStatus()
  {
    var child = Substitute.For<IBehaviorNode<TestInput>>();
    var node = new ThrottleNode<TestInput>(child, 1, 0);
    var input = new TestInput();

    child.Tick(Arg.Any<TestInput>()).Returns(NodeStatus.Failure, NodeStatus.Success, NodeStatus.Running);

    var status1 = node.Tick(input);
    var status2 = node.Tick(input);
    var status3 = node.Tick(input);

    Assert.Multiple(() => {
        Assert.That(status1, Is.EqualTo(NodeStatus.Failure));
        Assert.That(status2, Is.EqualTo(NodeStatus.Success));
        Assert.That(status3, Is.EqualTo(NodeStatus.Running));
      }
    );
  }

  [Test]
  public void Constructor_OnceInLessThanOne_DefaultsToOne()
  {
    var child = Substitute.For<IBehaviorNode<TestInput>>();
    var node = new ThrottleNode<TestInput>(child, 0);
    var input = new TestInput();

    node.Tick(input);
    node.Tick(input);

    child.Received(2).Tick(Arg.Any<TestInput>());
  }

  [Test]
  public void Constructor_OffsetNegative_DefaultsToZero()
  {
    var child = Substitute.For<IBehaviorNode<TestInput>>();
    var node = new ThrottleNode<TestInput>(child, 3, -1);
    var input = new TestInput();

    node.Tick(input);
    node.Tick(input);
    node.Tick(input);
    node.Tick(input);

    child.Received(2).Tick(Arg.Any<TestInput>());
  }
}
