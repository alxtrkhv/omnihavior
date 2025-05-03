using Omnihavior.Core;
using Omnihavior.Tests.Mocks;
using Omnihavior.Tree;

namespace Omnihavior.Tests.Tree;

[TestFixture]
public class ParallelNodeTests : BaseNodeTests<ParallelNode<TestInput>>
{
  protected override ParallelNode<TestInput> CreateNodeForResetTests(out int? childrenNumber,
    params IBehaviorNode<TestInput>[] children)
  {
    childrenNumber = null;
    return new(children, 0);
  }

  [Test]
  public void Tick_AllChildrenSucceed_ReturnsSuccess()
  {
    var child1 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var child2 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var parallelNode = new ParallelNode<TestInput>([child1, child2,], 0);

    var testInput = new TestInput();
    var result = parallelNode.Tick(testInput);

    Assert.That(result, Is.EqualTo(NodeStatus.Success));
  }

  [Test]
  public void Tick_FailuresWithinAllowance_ReturnsSuccess()
  {
    var child1 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var child2 = new LambdaNode<TestInput>(_ => NodeStatus.Failure);
    var child3 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var parallelNode = new ParallelNode<TestInput>([child1, child2, child3,], 1);

    var testInput = new TestInput();
    var result = parallelNode.Tick(testInput);

    Assert.That(result, Is.EqualTo(NodeStatus.Success));
  }

  [Test]
  public void Tick_FailuresExceedAllowance_ReturnsFailure()
  {
    var child1 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var child2 = new LambdaNode<TestInput>(_ => NodeStatus.Failure);
    var child3 = new LambdaNode<TestInput>(_ => NodeStatus.Failure);
    var parallelNode = new ParallelNode<TestInput>([child1, child2, child3,], 1);

    var testInput = new TestInput();
    var result = parallelNode.Tick(testInput);

    Assert.That(result, Is.EqualTo(NodeStatus.Failure));
  }

  [Test]
  public void Tick_OneChildRunning_FailuresWithinAllowance_ReturnsRunning()
  {
    var child1 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var child2 = new LambdaNode<TestInput>(_ => NodeStatus.Running);
    var child3 = new LambdaNode<TestInput>(_ => NodeStatus.Failure);
    var parallelNode = new ParallelNode<TestInput>([child1, child2, child3,], 1);

    var testInput = new TestInput();
    var result = parallelNode.Tick(testInput);

    Assert.That(result, Is.EqualTo(NodeStatus.Running));
  }

  [Test]
  public void Tick_OneChildRunning_FailuresExceedAllowance_ReturnsFailure()
  {
    var child1 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var child2 = new LambdaNode<TestInput>(_ => NodeStatus.Running);
    var child3 = new LambdaNode<TestInput>(_ => NodeStatus.Failure);
    var child4 = new LambdaNode<TestInput>(_ => NodeStatus.Failure);
    var parallelNode = new ParallelNode<TestInput>([child1, child2, child3, child4,], 1);

    var testInput = new TestInput();
    var result = parallelNode.Tick(testInput);

    Assert.That(result, Is.EqualTo(NodeStatus.Failure));
  }

  [Test]
  public void Tick_OneChildErrors_ReturnsError()
  {
    var child1 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var child2 = new LambdaNode<TestInput>(_ => NodeStatus.Error);
    var child3 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var parallelNode = new ParallelNode<TestInput>(new[] { child1, child2, child3 }, 0);

    var testInput = new TestInput();
    var result = parallelNode.Tick(testInput);

    Assert.That(result, Is.EqualTo(NodeStatus.Error));
  }

  [Test]
  public void Tick_EmptyChildren_ReturnsSuccess()
  {
    var parallelNode = new ParallelNode<TestInput>([], 0);

    var testInput = new TestInput();
    var result = parallelNode.Tick(testInput);

    Assert.That(result, Is.EqualTo(NodeStatus.Success));
  }

  [Test]
  public void Tick_DefaultFailureAllowance_OneFailureReturnsFailure()
  {
    var child1 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var child2 = new LambdaNode<TestInput>(_ => NodeStatus.Failure);
    var parallelNode = new ParallelNode<TestInput>([child1, child2,], 0);

    var testInput = new TestInput();
    var result = parallelNode.Tick(testInput);

    Assert.That(result, Is.EqualTo(NodeStatus.Failure));
  }

  [Test]
  public void Tick_FailureAllowanceEqualsChildCount_IgnoresFailuresReturnsSuccess()
  {
    var child1 = new LambdaNode<TestInput>(_ => NodeStatus.Failure);
    var child2 = new LambdaNode<TestInput>(_ => NodeStatus.Failure);
    var parallelNode = new ParallelNode<TestInput>([child1, child2,], 2);

    var testInput = new TestInput();
    var result = parallelNode.Tick(testInput);

    Assert.That(result, Is.EqualTo(NodeStatus.Success));
  }

  [Test]
  public void Tick_FailureAllowanceEqualsChildCount_OneRunningReturnsRunning()
  {
    var child1 = new LambdaNode<TestInput>(_ => NodeStatus.Failure);
    var child2 = new LambdaNode<TestInput>(_ => NodeStatus.Running);
    var parallelNode = new ParallelNode<TestInput>([child1, child2,], 2);

    var testInput = new TestInput();
    var result = parallelNode.Tick(testInput);

    Assert.That(result, Is.EqualTo(NodeStatus.Running));
  }


  [Test]
  public void Tick_AllChildrenTicked()
  {
    var child1Ticked = false;
    var child2Ticked = false;
    var child3Ticked = false;
    var child4Ticked = false;

    var child1 = new LambdaNode<TestInput>(_ => {
        child1Ticked = true;
        return NodeStatus.Error;
      }
    );
    var child2 = new LambdaNode<TestInput>(_ => {
        child2Ticked = true;
        return NodeStatus.Failure;
      }
    );
    var child3 = new LambdaNode<TestInput>(_ => {
        child3Ticked = true;
        return NodeStatus.Running;
      }
    );
    var child4 = new LambdaNode<TestInput>(_ => {
        child4Ticked = true;
        return NodeStatus.Success;
      }
    );

    var parallelNode = new ParallelNode<TestInput>([child1, child2, child3, child4,], 1);

    var testInput = new TestInput();
    parallelNode.Tick(testInput);

    Assert.Multiple(() => {
        Assert.That(child1Ticked, Is.True, "Child 1 should be ticked");
        Assert.That(child2Ticked, Is.True, "Child 2 should be ticked");
        Assert.That(child3Ticked, Is.True, "Child 3 should be ticked");
        Assert.That(child4Ticked, Is.True, "Child 4 should be ticked");
      }
    );
  }
}
