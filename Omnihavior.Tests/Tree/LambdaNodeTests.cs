using System.Diagnostics.CodeAnalysis;
using Omnihavior.Core;
using Omnihavior.Tree;
using Omnihavior.Tests.Tree.Mocks;

namespace Omnihavior.Tests.Tree;

[TestFixture]
public class LambdaNodeTests : BaseNodeTests<LambdaNode<TestInput>>
{
  private bool _lambdaExecuted;
  private bool _resetActionExecuted;
  private NodeStatus _lambdaResultStatus = NodeStatus.Success;

  [SetUp]
  public void SetUp()
  {
    _lambdaExecuted = false;
    _resetActionExecuted = false;
    _lambdaResultStatus = NodeStatus.Success;
  }

  protected override LambdaNode<TestInput> CreateNodeForResetTests(out int? childrenNumber,
    params IReadOnlyList<IBehaviorNode<TestInput>> children)
  {
    childrenNumber = 1;
    return new(_ => NodeStatus.Success, () => _resetActionExecuted = true);
  }

  private LambdaNode<TestInput> CreateTestNode(Action? resetAction = null)
  {
    return new(
      _ => {
        _lambdaExecuted = true;
        return _lambdaResultStatus;
      },
      resetAction ?? (() => _resetActionExecuted = true)
    );
  }

  [Test]
  public void Tick_ExecutesLambda()
  {
    var node = CreateTestNode();
    node.Tick(new());
    Assert.That(_lambdaExecuted, Is.True);
  }

  [Test]
  [TestCase(NodeStatus.Success)]
  [TestCase(NodeStatus.Failure)]
  [TestCase(NodeStatus.Running)]
  [TestCase(NodeStatus.Error)]
  [SuppressMessage("Structure", "NUnit1003:The TestCaseAttribute provided too few arguments")]
  public void Tick_ReturnsLambdaResult(NodeStatus expectedStatus)
  {
    _lambdaResultStatus = expectedStatus;
    var node = CreateTestNode();
    var result = node.Tick(new());
    Assert.That(result, Is.EqualTo(expectedStatus));
  }

  [Test]
  public void Reset_ExecutesResetAction_WhenProvided()
  {
    var node = CreateTestNode();
    var data = CreateInputData();
    node.Reset(data);
    Assert.That(_resetActionExecuted, Is.True);
  }

  [Test]
  public void Reset_DoesNothing_WhenNoResetActionProvided()
  {
    var node = new LambdaNode<TestInput>(_ => NodeStatus.Success, null);
    Assert.DoesNotThrow(() => node.Reset(new()));
    Assert.That(_resetActionExecuted, Is.False);
  }

  [Test]
  [TestCase(0, Description = "Checks reset after no ticks.")]
  [TestCase(1, Description = "Checks reset after one tick.")]
  [TestCase(5, Description = "Checks reset after five ticks.")]
  public override void Reset_AfterNumberOfTicks_ResetsAllChildren(int tickNumber)
  {
    var resetCalled = false;
    var node = new LambdaNode<TestInput>(_ => NodeStatus.Success, () => resetCalled = true);
    var data = CreateInputData();

    for (var i = 0; i < tickNumber; i++) {
      node.Tick(data);
    }

    node.Reset(data);

    Assert.That(resetCalled, Is.True, "Reset action should have been called.");
  }
}
