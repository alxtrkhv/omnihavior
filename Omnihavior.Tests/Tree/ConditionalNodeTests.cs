using System.Diagnostics.CodeAnalysis;
using Omnihavior.Core;
using Omnihavior.Tree;
using Omnihavior.Tests.Tree.Mocks;

namespace Omnihavior.Tests.Tree;

[TestFixture]
public class ConditionalNodeTests : BaseNodeTests<ConditionalNode<TestInput>>
{
  protected override ConditionalNode<TestInput> CreateNodeForResetTests(out int? childrenNumber,
    params IReadOnlyList<IBehaviorNode<TestInput>> children)
  {
    childrenNumber = 3;
    return new(children[0], children[1], children[2]);
  }

  [Test]
  public void Tick_WhenNotRunningOrErrorWithoutRawStatus_ReturnsSuccess(
    [Values(NodeStatus.Success, NodeStatus.Failure)]
    NodeStatus conditionStatus,
    [Values(NodeStatus.Success, NodeStatus.Failure)]
    NodeStatus positiveStatus,
    [Values(NodeStatus.Success, NodeStatus.Failure, null)]
    NodeStatus? negativeStatus)
  {
    var condition = new LambdaNode<TestInput>(_ => conditionStatus);
    var positiveBody = new LambdaNode<TestInput>(_ => positiveStatus);
    var negativeBody = negativeStatus is null ? null : new LambdaNode<TestInput>(_ => negativeStatus.Value);
    var conditionalNode = new ConditionalNode<TestInput>(condition, positiveBody, negativeBody);

    var result = conditionalNode.Tick(new());

    Assert.That(result, Is.EqualTo(NodeStatus.Success));
  }

  [Test]
  [TestCase(NodeStatus.Running, NodeStatus.Success, NodeStatus.Success, NodeStatus.Running)]
  [TestCase(NodeStatus.Error, NodeStatus.Success, NodeStatus.Success, NodeStatus.Error)]
  [TestCase(NodeStatus.Success, NodeStatus.Running, NodeStatus.Success, NodeStatus.Running)]
  [TestCase(NodeStatus.Success, NodeStatus.Error, NodeStatus.Success, NodeStatus.Error)]
  [TestCase(NodeStatus.Failure, NodeStatus.Success, NodeStatus.Running, NodeStatus.Running)]
  [TestCase(NodeStatus.Failure, NodeStatus.Success, NodeStatus.Error, NodeStatus.Error)]
  public void Tick_WithRunningOrError_ReturnsRunningOrError(
    NodeStatus conditionStatus, NodeStatus positiveStatus, NodeStatus? negativeStatus, NodeStatus expectedStatus)
  {
    var condition = new LambdaNode<TestInput>(_ => conditionStatus);
    var positiveBody = new LambdaNode<TestInput>(_ => positiveStatus);
    var negativeBody = negativeStatus is null ? null : new LambdaNode<TestInput>(_ => negativeStatus.Value);
    var conditionalNode = new ConditionalNode<TestInput>(condition, positiveBody, negativeBody);

    var result = conditionalNode.Tick(new());

    Assert.That(result, Is.EqualTo(expectedStatus));
  }

  [Test]
  [TestCase(NodeStatus.Success, true, true, false)]
  [TestCase(NodeStatus.Success, false, true, false)]
  [TestCase(NodeStatus.Failure, true, false, true)]
  [TestCase(NodeStatus.Failure, false, false, false)]
  [TestCase(NodeStatus.Running, true, false, false)]
  [TestCase(NodeStatus.Running, false, false, false)]
  [TestCase(NodeStatus.Error, true, false, false)]
  [TestCase(NodeStatus.Error, false, false, false)]
  public void Tick_Always_ExecutesOnlyRequiredNodes(NodeStatus conditionStatus, bool negativeNodeExists,
    bool positiveNodeExpectedToTick,
    bool negativeNodeExpectedToTick)
  {
    var positiveNodeTicked = false;
    var negativeNodeTicked = false;

    var condition = new LambdaNode<TestInput>(_ => conditionStatus);
    var positiveBody = new LambdaNode<TestInput>(_ => {
        positiveNodeTicked = true;
        return NodeStatus.Success;
      }
    );
    var negativeBody = negativeNodeExists
      ? new LambdaNode<TestInput>(_ => {
          negativeNodeTicked = true;
          return NodeStatus.Success;
        }
      )
      : null;

    var conditionalNode = new ConditionalNode<TestInput>(condition, positiveBody, negativeBody);

    conditionalNode.Tick(new());

    Assert.Multiple(() => {
        Assert.That(positiveNodeTicked, Is.EqualTo(positiveNodeExpectedToTick));
        Assert.That(negativeNodeTicked, Is.EqualTo(negativeNodeExpectedToTick));
      }
    );
  }

  [TestCase(NodeStatus.Success, NodeStatus.Success, NodeStatus.Failure, NodeStatus.Success)]
  [TestCase(NodeStatus.Success, NodeStatus.Failure, NodeStatus.Failure, NodeStatus.Failure)]
  [TestCase(NodeStatus.Failure, NodeStatus.Success, NodeStatus.Success, NodeStatus.Success)]
  [TestCase(NodeStatus.Failure, NodeStatus.Success, NodeStatus.Failure, NodeStatus.Failure)]
  [TestCase(NodeStatus.Failure, NodeStatus.Success, null, NodeStatus.Failure)]
  [TestCase(NodeStatus.Running, NodeStatus.Success, NodeStatus.Success, NodeStatus.Running)]
  [TestCase(NodeStatus.Error, NodeStatus.Success, NodeStatus.Success, NodeStatus.Error)]
  [TestCase(NodeStatus.Success, NodeStatus.Running, NodeStatus.Success, NodeStatus.Running)]
  [TestCase(NodeStatus.Success, NodeStatus.Error, NodeStatus.Success, NodeStatus.Error)]
  [TestCase(NodeStatus.Failure, NodeStatus.Success, NodeStatus.Running, NodeStatus.Running)]
  [TestCase(NodeStatus.Failure, NodeStatus.Success, NodeStatus.Error, NodeStatus.Error)]
  public void Tick_WithReturnRawStatus_ReturnsActualStatusOfLastExecutedNode(
    NodeStatus conditionStatus, NodeStatus positiveStatus, NodeStatus? negativeStatus, NodeStatus expectedStatus)
  {
    var condition = new LambdaNode<TestInput>(_ => conditionStatus);
    var positiveBody = new LambdaNode<TestInput>(_ => positiveStatus);
    var negativeBody = negativeStatus is null ? null : new LambdaNode<TestInput>(_ => negativeStatus.Value);
    var conditionalNode = new ConditionalNode<TestInput>(
      condition,
      positiveBody,
      negativeBody,
      ConditionRules.ReturnRawStatus
    );

    var result = conditionalNode.Tick(new());

    Assert.That(result, Is.EqualTo(expectedStatus));
  }

  [TestCase(NodeStatus.Success, NodeStatus.Success, null, true, 3, 1)]
  [TestCase(NodeStatus.Failure, NodeStatus.Success, NodeStatus.Success, true, 3, 1)]
  [TestCase(NodeStatus.Success, NodeStatus.Success, null, false, 3, 3)]
  [TestCase(NodeStatus.Failure, NodeStatus.Success, NodeStatus.Success, false, 3, 3)]
  [TestCase(NodeStatus.Success, NodeStatus.Running, null, true, 3, 1)]
  [TestCase(NodeStatus.Success, NodeStatus.Running, null, false, 3, 1)]
  [TestCase(NodeStatus.Failure, NodeStatus.Success, NodeStatus.Running, true, 3, 1)]
  [TestCase(NodeStatus.Failure, NodeStatus.Success, NodeStatus.Running, false, 3, 1)]
  [TestCase(NodeStatus.Success, NodeStatus.Error, null, true, 3, 1)]
  [TestCase(NodeStatus.Success, NodeStatus.Error, null, false, 3, 1)]
  [TestCase(NodeStatus.Failure, NodeStatus.Success, NodeStatus.Error, true, 3, 1)]
  [TestCase(NodeStatus.Failure, NodeStatus.Success, NodeStatus.Error, false, 3, 1)]
  [TestCase(NodeStatus.Running, NodeStatus.Success, null, true, 3, 3)]
  [TestCase(NodeStatus.Running, NodeStatus.Success, null, false, 3, 3)]
  [TestCase(NodeStatus.Error, NodeStatus.Success, null, true, 3, 3)]
  [TestCase(NodeStatus.Error, NodeStatus.Success, null, false, 3, 3)]
  public void Tick_ConditionExecutionCount_MatchesCheckOnceRule(
    NodeStatus conditionStatus, NodeStatus positiveStatus, NodeStatus? negativeStatus,
    bool checkConditionOnlyOnce, int tickCount, int expectedConditionTicks)
  {
    var conditionTickCount = 0;
    var condition = new LambdaNode<TestInput>(_ => {
        conditionTickCount++;
        return conditionStatus;
      }
    );
    var positiveBody = new LambdaNode<TestInput>(_ => positiveStatus);
    var negativeBody = negativeStatus is null ? null : new LambdaNode<TestInput>(_ => negativeStatus.Value);

    var rules = checkConditionOnlyOnce ? ConditionRules.CheckConditionOnlyOnce : ConditionRules.None;
    var conditionalNode = new ConditionalNode<TestInput>(condition, positiveBody, negativeBody, rules);

    for (var i = 0; i < tickCount; i++) {
      conditionalNode.Tick(new());
    }

    Assert.That(conditionTickCount, Is.EqualTo(expectedConditionTicks));
  }
}
