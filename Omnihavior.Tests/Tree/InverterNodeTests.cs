using Omnihavior.Core;
using Omnihavior.Tree;
using Omnihavior.Tests.Tree.Mocks;

namespace Omnihavior.Tests.Tree;

[TestFixture]
public class InverterNodeTests : BaseNodeTests<InverterNode<TestInput>>
{
  protected override InverterNode<TestInput> CreateNodeForResetTests(out int? childrenNumber,
    params IReadOnlyList<IBehaviorNode<TestInput>> children)
  {
    childrenNumber = 1;
    return new(children[0]);
  }

  [Test]
  public void Tick_ChildSucceeds_ReturnsFailure()
  {
    var child = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var invertedNode = new InverterNode<TestInput>(child);

    var result = invertedNode.Tick(new());

    Assert.That(result, Is.EqualTo(NodeStatus.Failure));
  }

  [Test]
  public void Tick_ChildFails_ReturnsSuccess()
  {
    var child = new LambdaNode<TestInput>(_ => NodeStatus.Failure);
    var invertedNode = new InverterNode<TestInput>(child);

    var result = invertedNode.Tick(new());

    Assert.That(result, Is.EqualTo(NodeStatus.Success));
  }

  [Test]
  public void Tick_ChildRunning_ReturnsRunning()
  {
    var child = new LambdaNode<TestInput>(_ => NodeStatus.Running);
    var invertedNode = new InverterNode<TestInput>(child);

    var result = invertedNode.Tick(new());

    Assert.That(result, Is.EqualTo(NodeStatus.Running));
  }

  [Test]
  public void Tick_ChildErrors_ReturnsError()
  {
    var child = new LambdaNode<TestInput>(_ => NodeStatus.Error);
    var invertedNode = new InverterNode<TestInput>(child);

    var result = invertedNode.Tick(new());

    Assert.That(result, Is.EqualTo(NodeStatus.Error));
  }
}
