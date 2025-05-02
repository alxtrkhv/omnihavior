using Omnihavior.Core;
using Omnihavior.Tests.Mocks;
using Omnihavior.Tests.Tree;

namespace Omnihavior.Tests.Core;

[TestFixture]
public class LambdaNodeTests : BaseNodeTests<LambdaNode<TestInput>>
{
  private bool _lambdaExecuted;
  private bool _resetActionExecuted;
  private NodeStatus _lambdaResultStatus = NodeStatus.Success;
  private LambdaNode<TestInput> _testNode;

  [SetUp]
  public void SetUp()
  {
    _lambdaExecuted = false;
    _resetActionExecuted = false;
    _lambdaResultStatus = NodeStatus.Success;
    _testNode = new(
      _ => {
        _lambdaExecuted = true;
        return _lambdaResultStatus;
      },
      _ => _resetActionExecuted = true
    );
  }

  protected override LambdaNode<TestInput> CreateNodeForResetTests(out int? childrenNumber,
    params IBehaviorNode<TestInput>[] children)
  {
    childrenNumber = 1;
    return new(input => children[0].Tick(input), input => children[0].Reset(input));
  }

  [Test]
  public void Tick_ExecutesLambda()
  {
    var node = _testNode;
    node.Tick(new());
    Assert.That(_lambdaExecuted, Is.True);
  }

  [Test]
  [TestCase(NodeStatus.Success)]
  [TestCase(NodeStatus.Failure)]
  [TestCase(NodeStatus.Running)]
  [TestCase(NodeStatus.Error)]
  public void Tick_ReturnsLambdaResult(NodeStatus expectedStatus)
  {
    _lambdaResultStatus = expectedStatus;
    var node = _testNode;
    var result = node.Tick(new());
    Assert.That(result, Is.EqualTo(expectedStatus));
  }

  [Test]
  public void Reset_ExecutesResetAction_WhenProvided()
  {
    var node = _testNode;
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
}
