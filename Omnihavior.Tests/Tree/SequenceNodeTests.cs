using Omnihavior.Core;
using Omnihavior.Tests.Mocks;
using Omnihavior.Tree;

namespace Omnihavior.Tests.Tree;

[TestFixture]
public class SequenceNodeTests : BaseNodeTests<SequenceNode<TestInput>>
{
  protected override SequenceNode<TestInput> CreateNodeForResetTests(out int? childrenNumber,
    params IBehaviorNode<TestInput>[] children)
  {
    childrenNumber = null;
    return new(children);
  }

  [Test]
  public void Tick_AllChildrenSucceed_ReturnsSuccess()
  {
    var child1 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var child2 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var sequence = new SequenceNode<TestInput>(child1, child2);

    var result = sequence.Tick(new());

    Assert.That(result, Is.EqualTo(NodeStatus.Success));
  }

  [Test]
  public void Tick_OneChildFails_ReturnsFailure()
  {
    var child1 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var child2 = new LambdaNode<TestInput>(_ => NodeStatus.Failure);
    var child3 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var sequence = new SequenceNode<TestInput>(child1, child2, child3);

    var result = sequence.Tick(new());

    Assert.That(result, Is.EqualTo(NodeStatus.Failure));
  }

  [Test]
  public void Tick_OneChildRunning_ReturnsRunning()
  {
    var child1 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var child2 = new LambdaNode<TestInput>(_ => NodeStatus.Running);
    var child3 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var sequence = new SequenceNode<TestInput>(child1, child2, child3);

    var result = sequence.Tick(new());

    Assert.That(result, Is.EqualTo(NodeStatus.Running));
  }

  [Test]
  public void Tick_OneChildErrors_ReturnsError()
  {
    var child1 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var child2 = new LambdaNode<TestInput>(_ => NodeStatus.Error);
    var child3 = new LambdaNode<TestInput>(_ => NodeStatus.Success);
    var sequence = new SequenceNode<TestInput>(child1, child2, child3);

    var result = sequence.Tick(new());

    Assert.That(result, Is.EqualTo(NodeStatus.Error));
  }

  [Test]
  public void Tick_AllChildrenSucceed_AllChildrenRun()
  {
    var child1Run = false;
    var child2Run = false;
    var child1 = new LambdaNode<TestInput>(_ => {
        child1Run = true;
        return NodeStatus.Success;
      }
    );
    var child2 = new LambdaNode<TestInput>(_ => {
        child2Run = true;
        return NodeStatus.Success;
      }
    );
    var sequence = new SequenceNode<TestInput>(child1, child2);

    sequence.Tick(new());

    Assert.Multiple(() => {
        Assert.That(child1Run, Is.True, "Child 1 should have run");
        Assert.That(child2Run, Is.True, "Child 2 should have run");
      }
    );
  }

  [Test]
  public void Tick_Failure_FollowingChildrenNotRun()
  {
    var child1Run = false;
    var child2Run = false;
    var child3Run = false;
    var child1 = new LambdaNode<TestInput>(_ => {
        child1Run = true;
        return NodeStatus.Success;
      }
    );
    var child2 = new LambdaNode<TestInput>(_ => {
        child2Run = true;
        return NodeStatus.Failure;
      }
    );
    var child3 = new LambdaNode<TestInput>(_ => {
        child3Run = true;
        return NodeStatus.Success;
      }
    );
    var sequence = new SequenceNode<TestInput>(child1, child2, child3);

    sequence.Tick(new());

    Assert.Multiple(() => {
        Assert.That(child1Run, Is.True, "Child 1 should have run");
        Assert.That(child2Run, Is.True, "Child 2 should have run");
        Assert.That(child3Run, Is.False, "Child 3 should not have run after failure");
      }
    );
  }

  [Test]
  public void Tick_Running_FollowingChildrenNotRun()
  {
    var child1Run = false;
    var child2Run = false;
    var child3Run = false;
    var child1 = new LambdaNode<TestInput>(_ => {
        child1Run = true;
        return NodeStatus.Success;
      }
    );
    var child2 = new LambdaNode<TestInput>(_ => {
        child2Run = true;
        return NodeStatus.Running;
      }
    );
    var child3 = new LambdaNode<TestInput>(_ => {
        child3Run = true;
        return NodeStatus.Success;
      }
    );
    var sequence = new SequenceNode<TestInput>(child1, child2, child3);

    sequence.Tick(new());

    Assert.Multiple(() => {
        Assert.That(child1Run, Is.True, "Child 1 should have run");
        Assert.That(child2Run, Is.True, "Child 2 should have run");
        Assert.That(child3Run, Is.False, "Child 3 should not have run after Running");
      }
    );
  }

  [Test]
  public void Tick_Error_FollowingChildrenNotRun()
  {
    var child1Run = false;
    var child2Run = false;
    var child3Run = false;
    var child1 = new LambdaNode<TestInput>(_ => {
        child1Run = true;
        return NodeStatus.Success;
      }
    );
    var child2 = new LambdaNode<TestInput>(_ => {
        child2Run = true;
        return NodeStatus.Error;
      }
    );
    var child3 = new LambdaNode<TestInput>(_ => {
        child3Run = true;
        return NodeStatus.Success;
      }
    );
    var sequence = new SequenceNode<TestInput>(child1, child2, child3);

    sequence.Tick(new());

    Assert.Multiple(() => {
        Assert.That(child1Run, Is.True, "Child 1 should have run");
        Assert.That(child2Run, Is.True, "Child 2 should have run");
        Assert.That(child3Run, Is.False, "Child 3 should not have run after error");
      }
    );
  }

  [Test]
  public void Tick_ResumeAfterRunning_ContinuesFromRunningNode()
  {
    var child1RunCount = 0;
    var child2RunCount = 0;
    var child3RunCount = 0;

    var child1 = new LambdaNode<TestInput>(_ => {
        child1RunCount++;
        return NodeStatus.Success;
      }
    );
    var child2 = new LambdaNode<TestInput>(_ => {
        child2RunCount++;
        return child2RunCount == 1 ? NodeStatus.Running : NodeStatus.Success;
      }
    );
    var child3 = new LambdaNode<TestInput>(_ => {
        child3RunCount++;
        return NodeStatus.Success;
      }
    );
    var sequence = new SequenceNode<TestInput>(child1, child2, child3);

    sequence.Tick(new());

    Assert.Multiple(() => {
        Assert.That(child1RunCount, Is.EqualTo(1), "Child 1 should run once on first run");
        Assert.That(child2RunCount, Is.EqualTo(1), "Child 2 should run once on first run");
        Assert.That(child3RunCount, Is.EqualTo(0), "Child 3 should not run on first run");
      }
    );

    sequence.Tick(new());

    Assert.Multiple(() => {
        Assert.That(child1RunCount, Is.EqualTo(1), "Child 1 should not run again on second run");
        Assert.That(child2RunCount, Is.EqualTo(2), "Child 2 should run again on second run");
        Assert.That(child3RunCount, Is.EqualTo(1), "Child 3 should run on second run");
      }
    );
  }

  [Test]
  public void Tick_ResumeAfterError_ContinuesFromErrorNode()
  {
    var child1RunCount = 0;
    var child2RunCount = 0;
    var child3RunCount = 0;

    var child1 = new LambdaNode<TestInput>(_ => {
        child1RunCount++;
        return NodeStatus.Success;
      }
    );
    var child2 = new LambdaNode<TestInput>(_ => {
        child2RunCount++;
        return child2RunCount == 1 ? NodeStatus.Error : NodeStatus.Success;
      }
    );
    var child3 = new LambdaNode<TestInput>(_ => {
        child3RunCount++;
        return NodeStatus.Success;
      }
    );
    var sequence = new SequenceNode<TestInput>(child1, child2, child3);

    sequence.Tick(new());

    Assert.Multiple(() => {
        Assert.That(child1RunCount, Is.EqualTo(1), "Child 1 should run once on first run");
        Assert.That(child2RunCount, Is.EqualTo(1), "Child 2 should run once on first run");
        Assert.That(child3RunCount, Is.EqualTo(0), "Child 3 should not run on first run");
      }
    );

    sequence.Tick(new());

    Assert.Multiple(() => {
        Assert.That(child1RunCount, Is.EqualTo(1), "Child 1 should not run again on second run");
        Assert.That(child2RunCount, Is.EqualTo(2), "Child 2 should run again on second run");
        Assert.That(child3RunCount, Is.EqualTo(1), "Child 3 should run on second run");
      }
    );
  }

  [Test]
  public void Tick_EmptySequence_ReturnsSuccess()
  {
    var sequence = new SequenceNode<TestInput>();

    var result = sequence.Tick(new());

    Assert.That(result, Is.EqualTo(NodeStatus.Success));
  }
}
