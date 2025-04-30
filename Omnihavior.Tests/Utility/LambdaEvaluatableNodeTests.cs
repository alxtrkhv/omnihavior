using Omnihavior.Core;
using Omnihavior.Tests.Mocks;
using Omnihavior.Tests.Tree;
using Omnihavior.Utility;

namespace Omnihavior.Tests.Utility;

[TestFixture]
public class LambdaEvaluatableNodeTests : BaseNodeTests<LambdaEvaluatableNode<TestInput>>
{
  private bool _lambdaExecuted;
  private bool _resetActionExecuted;
  private NodeStatus _lambdaResultStatus;
  private float _lambdaResultValue;
  private LambdaEvaluatableNode<TestInput> _testNode;

  protected override LambdaEvaluatableNode<TestInput> CreateNodeForResetTests(out int? childrenNumber,
    params IBehaviorNode<TestInput>[] children)
  {
    childrenNumber = 1;

    return new(
      input => children[0].Tick(input),
      _ => 2f,
      input => children[0].Reset(input)
    );
  }

  [SetUp]
  public void SetUp()
  {
    _lambdaExecuted = false;
    _resetActionExecuted = false;
    _lambdaResultStatus = NodeStatus.Success;
    _lambdaResultValue = 42.0f;
    _testNode = new(
      _ => {
        _lambdaExecuted = true;
        return _lambdaResultStatus;
      },
      _ => _lambdaResultValue,
      _ => _resetActionExecuted = true
    );
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
  public void Tick_ReturnsLambdaResultStatus(NodeStatus expectedStatus)
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
    var data = new TestInput();
    node.Reset(data);
    Assert.That(_resetActionExecuted, Is.True);
  }

  [Test]
  public void Reset_DoesNothing_WhenNoResetActionProvided()
  {
    var node = new LambdaEvaluatableNode<TestInput>(
      _ => NodeStatus.Success,
      null!,
      null
    );
    Assert.DoesNotThrow(() => node.Reset(new()));
    Assert.That(_resetActionExecuted, Is.False);
  }

  [Test]
  public void Evaluate_ReturnsResultOfProvidedFunction()
  {
    var actualResult = _testNode.Evaluate(new());

    Assert.That(actualResult, Is.EqualTo(_lambdaResultValue));
  }
}
