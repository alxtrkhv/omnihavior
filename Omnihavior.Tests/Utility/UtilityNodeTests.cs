using System.Diagnostics.CodeAnalysis;
using Omnihavior.Core;
using Omnihavior.Tests.Mocks;
using Omnihavior.Tests.Tree;
using Omnihavior.Utility;

namespace Omnihavior.Tests.Utility;

public class UtilityNodeTests : BaseNodeTests<UtilityNode<TestInput>>
{
  protected override UtilityNode<TestInput> CreateNodeForResetTests(out int? childrenNumber,
    params IBehaviorNode<TestInput>[] children)
  {
    childrenNumber = null;

    var evaluatableChildren = children.Select(x =>
      new LambdaEvaluatableNode<TestInput>(x.Tick, _ => 1f, x.Reset)
    ).ToList();

    return new(evaluatableChildren);
  }

  [Test]
  public void Tick_ExecutesNodeWithHighestEvaluation()
  {
    var node1Executed = false;
    var node2Executed = false;
    var node3Executed = false;

    var node1 = new LambdaEvaluatableNode<TestInput>(
      _ => {
        node1Executed = true;
        return NodeStatus.Success;
      },
      _ => 1.0f
    );
    var node2 = new LambdaEvaluatableNode<TestInput>(
      _ => {
        node2Executed = true;
        return NodeStatus.Failure;
      },
      _ => 2.0f
    );
    var node3 = new LambdaEvaluatableNode<TestInput>(
      _ => {
        node3Executed = true;
        return NodeStatus.Success;
      },
      _ => 1.5f
    );

    var utilityNode = new UtilityNode<TestInput>(new[] { node1, node2, node3 });
    var testInput = new TestInput();
    utilityNode.Tick(testInput);

    Assert.Multiple(() => {
        Assert.That(node1Executed, Is.False);
        Assert.That(node2Executed, Is.True);
        Assert.That(node3Executed, Is.False);
      }
    );
  }

  [Test]
  public void Tick_WhenMinimalThresholdNotMet_ExecutesNoNode()
  {
    var node1Executed = false;
    var node2Executed = false;

    var node1 = new LambdaEvaluatableNode<TestInput>(
      _ => {
        node1Executed = true;
        return NodeStatus.Success;
      },
      _ => 0.5f
    );
    var node2 = new LambdaEvaluatableNode<TestInput>(
      _ => {
        node2Executed = true;
        return NodeStatus.Failure;
      },
      _ => 0.8f
    );

    var utilityNode = new UtilityNode<TestInput>([node1, node2,], minEvaluationThreshold: 1.0f);
    var testInput = new TestInput();
    utilityNode.Tick(testInput);

    Assert.Multiple(() => {
        Assert.That(node1Executed, Is.False);
        Assert.That(node2Executed, Is.False);
      }
    );
  }

  [Test]
  public void Tick_EqualEvaluation_SelectsFirstByDefault()
  {
    var node1Executed = false;
    var node2Executed = false;
    var node3Executed = false;

    var node1 = new LambdaEvaluatableNode<TestInput>(
      _ => {
        node1Executed = true;
        return NodeStatus.Success;
      },
      _ => 1.0f
    );
    var node2 = new LambdaEvaluatableNode<TestInput>(
      _ => {
        node2Executed = true;
        return NodeStatus.Failure;
      },
      _ => 2.0f
    );
    var node3 = new LambdaEvaluatableNode<TestInput>(
      _ => {
        node3Executed = true;
        return NodeStatus.Success;
      },
      _ => 2.0f
    );

    var utilityNode = new UtilityNode<TestInput>([node1, node2, node3,]);
    var testInput = new TestInput();
    utilityNode.Tick(testInput);

    Assert.Multiple(() => {
        Assert.That(node1Executed, Is.False);
        Assert.That(node2Executed, Is.True);
        Assert.That(node3Executed, Is.False);
      }
    );
  }

  [Test]
  public void Tick_EqualEvaluation_SelectsLastWhenRuleSet()
  {
    var node1Executed = false;
    var node2Executed = false;
    var node3Executed = false;

    var node1 = new LambdaEvaluatableNode<TestInput>(
      _ => {
        node1Executed = true;
        return NodeStatus.Success;
      },
      _ => 1.0f
    );
    var node2 = new LambdaEvaluatableNode<TestInput>(
      _ => {
        node2Executed = true;
        return NodeStatus.Failure;
      },
      _ => 2.0f
    );
    var node3 = new LambdaEvaluatableNode<TestInput>(
      _ => {
        node3Executed = true;
        return NodeStatus.Success;
      },
      _ => 2.0f
    );

    var utilityNode = new UtilityNode<TestInput>([node1, node2, node3,], UtilityRules.IfEqualSelectLast);
    var testInput = new TestInput();
    utilityNode.Tick(testInput);

    Assert.Multiple(() => {
        Assert.That(node1Executed, Is.False);
        Assert.That(node2Executed, Is.False);
        Assert.That(node3Executed, Is.True);
      }
    );
  }

  [Test]
  public void Tick_EmptyNode_ReturnsFailureByDefault()
  {
    var utilityNode = new UtilityNode<TestInput>([]);
    var testInput = new TestInput();
    var result = utilityNode.Tick(testInput);
    Assert.That(result, Is.EqualTo(NodeStatus.Failure));
  }

  [Test]
  public void Tick_EmptyNode_ReturnsSuccessWhenRuleSet()
  {
    var utilityNode = new UtilityNode<TestInput>([], UtilityRules.InterceptFlowsFailureIfEmpty);
    var testInput = new TestInput();
    var result = utilityNode.Tick(testInput);
    Assert.That(result, Is.EqualTo(NodeStatus.Success));
  }

  [Test]
  public void Tick_NoNodePassesThreshold_ReturnsFailureByDefault()
  {
    var node1 = new LambdaEvaluatableNode<TestInput>(_ => NodeStatus.Success, _ => 0.5f);
    var node2 = new LambdaEvaluatableNode<TestInput>(_ => NodeStatus.Success, _ => 0.8f);

    var utilityNode = new UtilityNode<TestInput>([node1, node2,], minEvaluationThreshold: 1.0f);
    var testInput = new TestInput();
    var result = utilityNode.Tick(testInput);

    Assert.That(result, Is.EqualTo(NodeStatus.Failure));
  }

  [Test]
  public void Tick_NoNodePassesThreshold_ReturnsSuccessWhenRuleSet()
  {
    var node1 = new LambdaEvaluatableNode<TestInput>(_ => NodeStatus.Success, _ => 0.5f);
    var node2 = new LambdaEvaluatableNode<TestInput>(_ => NodeStatus.Success, _ => 0.8f);

    var utilityNode = new UtilityNode<TestInput>(
      [node1, node2,],
      UtilityRules.InterceptFlowsFailureIfNoActionPassesThreshold,
      minEvaluationThreshold: 1.0f
    );
    var testInput = new TestInput();
    var result = utilityNode.Tick(testInput);

    Assert.That(result, Is.EqualTo(NodeStatus.Success));
  }

  [TestCase(NodeStatus.Success, NodeStatus.Success)]
  [TestCase(NodeStatus.Failure, NodeStatus.Failure)]
  [TestCase(NodeStatus.Running, NodeStatus.Running)]
  [TestCase(NodeStatus.Error, NodeStatus.Error)]
  public void Tick_WithDefaultRules_ReturnsCorrectStatus(NodeStatus childStatus, NodeStatus expectedStatus)
  {
    var node = new LambdaEvaluatableNode<TestInput>(_ => childStatus, _ => 1.0f);
    var utilityNode = new UtilityNode<TestInput>([node,]);
    var testInput = new TestInput();
    var status = utilityNode.Tick(testInput);

    Assert.That(status, Is.EqualTo(expectedStatus));
  }

  [TestCase(NodeStatus.Success, NodeStatus.Success)]
  [TestCase(NodeStatus.Failure, NodeStatus.Success)]
  [TestCase(NodeStatus.Running, NodeStatus.Running)]
  [TestCase(NodeStatus.Error, NodeStatus.Error)]
  public void Tick_WithInterceptChildsFailure_ReturnsCorrectStatus(NodeStatus childStatus, NodeStatus expectedStatus)
  {
    var node = new LambdaEvaluatableNode<TestInput>(_ => childStatus, _ => 1.0f);
    var utilityNode = new UtilityNode<TestInput>([node,], UtilityRules.InterceptChildsFailure);
    var testInput = new TestInput();
    var status = utilityNode.Tick(testInput);
    Assert.That(status, Is.EqualTo(expectedStatus));
  }

  [TestCase(NodeStatus.Running)]
  [TestCase(NodeStatus.Error)]
  public void Tick_SkipsEvaluationWhenNodeIsRunningOrError(NodeStatus intermediateStatus)
  {
    var eval1Count = 0;
    var eval2Count = 0;
    var tick1Count = 0;
    var tick2Count = 0;

    var node1 = new LambdaEvaluatableNode<TestInput>(
      _ => {
        tick1Count++;
        return NodeStatus.Success;
      },
      _ => {
        eval1Count++;
        return 1.0f;
      }
    );
    var node2 = new LambdaEvaluatableNode<TestInput>(
      _ => {
        tick2Count++;
        return tick2Count == 1 ? intermediateStatus : NodeStatus.Success;
      },
      _ => {
        eval2Count++;
        return 2.0f;
      }
    );

    var utilityNode = new UtilityNode<TestInput>([node1, node2,]);

    var testInput = new TestInput();
    utilityNode.Tick(testInput);
    Assert.Multiple(() => {
        Assert.That(eval1Count, Is.EqualTo(1), "Eval 1 count after first tick");
        Assert.That(eval2Count, Is.EqualTo(1), "Eval 2 count after first tick");
        Assert.That(tick1Count, Is.EqualTo(0), "Tick 1 count after first tick");
        Assert.That(tick2Count, Is.EqualTo(1), "Tick 2 count after first tick");
      }
    );


    utilityNode.Tick(testInput);
    Assert.Multiple(() => {
        Assert.That(eval1Count, Is.EqualTo(1), "Eval 1 count after second tick - should not change");
        Assert.That(eval2Count, Is.EqualTo(1), "Eval 2 count after second tick - should not change");
        Assert.That(tick1Count, Is.EqualTo(0), "Tick 1 count after second tick - should not change");
        Assert.That(tick2Count, Is.EqualTo(2), "Tick 2 count after second tick");
      }
    );
  }

  [Test]
  [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
  public void Tick_AppliesLastNodeBonus()
  {
    var lastNodeBonus = 0.5f;

    var node1ExecutedCount = 0;
    var node2ExecutedCount = 0;

    var node1 = new LambdaEvaluatableNode<TestInput>(
      _ => { node1ExecutedCount++; return NodeStatus.Success; },
      _ => node1ExecutedCount == 0 ? 1.2f : 0.8f
    );
    var node2 = new LambdaEvaluatableNode<TestInput>(
      _ => { node2ExecutedCount++; return NodeStatus.Success; },
      _ => 1.0f
    );

    var utilityNode = new UtilityNode<TestInput>([node1, node2,], lastNodeBonus: lastNodeBonus);

    var testInput = new TestInput();
    utilityNode.Tick(testInput);
    Assert.Multiple(() => {
        Assert.That(node1ExecutedCount, Is.EqualTo(1), "Node 1 count after first tick");
        Assert.That(node2ExecutedCount, Is.EqualTo(0), "Node 2 count after first tick");
      }
    );

    utilityNode.Tick(testInput);
    Assert.Multiple(() => {
        Assert.That(node1ExecutedCount, Is.EqualTo(2), "Node 1 count after second tick");
        Assert.That(node2ExecutedCount, Is.EqualTo(0), "Node 2 count after second tick");
      }
    );
  }
}
