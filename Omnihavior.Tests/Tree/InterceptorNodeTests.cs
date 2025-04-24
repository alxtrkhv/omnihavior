using System.Diagnostics.CodeAnalysis;
using Omnihavior.Core;
using Omnihavior.Tree;
using Omnihavior.Tests.Tree.Mocks;

namespace Omnihavior.Tests.Tree;

[TestFixture]
public class InterceptorNodeTests : BaseNodeTests<InterceptorNode<TestInput>>
{
  private bool _childTicked;
  private NodeStatus _childStatus = NodeStatus.Success;

  [SetUp]
  public void SetUp()
  {
    _childTicked = false;
    _childStatus = NodeStatus.Success;
  }

  protected override InterceptorNode<TestInput> CreateNodeForResetTests(out int? childrenNumber,
    params IReadOnlyList<IBehaviorNode<TestInput>> children)
  {
    childrenNumber = 1;
    return new(children[0], InterceptionRules.OnSuccess);
  }

  private LambdaNode<TestInput> CreateMockChild()
  {
    return new(_ => {
        _childTicked = true;
        return _childStatus;
      }
    );
  }

  [Test]
  [TestCase(NodeStatus.Success, InterceptionRules.OnSuccess, NodeStatus.Success)]
  [TestCase(NodeStatus.Failure, InterceptionRules.OnFailure, NodeStatus.Success)]
  [TestCase(NodeStatus.Running, InterceptionRules.OnRunning, NodeStatus.Success)]
  [TestCase(NodeStatus.Error, InterceptionRules.OnError, NodeStatus.Success)]
  [SuppressMessage("Structure", "NUnit1003:The TestCaseAttribute provided too few arguments")]
  public void Tick_RuleMatchesChildStatus_ReturnsSuccess(NodeStatus childStatus, InterceptionRules rule,
    NodeStatus expectedStatus)
  {
    _childStatus = childStatus;
    var child = CreateMockChild();
    var node = new InterceptorNode<TestInput>(child, rule);

    var result = node.Tick(new());

    Assert.Multiple(() => {
        Assert.That(result, Is.EqualTo(expectedStatus), "Node should return Success when rule matches.");
        Assert.That(_childTicked, Is.True, "Child should have been ticked.");
      }
    );
  }

  [Test]
  [TestCase(NodeStatus.Success, InterceptionRules.OnSuccess | InterceptionRules.Negative, NodeStatus.Failure)]
  [TestCase(NodeStatus.Failure, InterceptionRules.OnFailure | InterceptionRules.Negative, NodeStatus.Failure)]
  [TestCase(NodeStatus.Running, InterceptionRules.OnRunning | InterceptionRules.Negative, NodeStatus.Failure)]
  [TestCase(NodeStatus.Error, InterceptionRules.OnError | InterceptionRules.Negative, NodeStatus.Failure)]
  [SuppressMessage("Structure", "NUnit1003:The TestCaseAttribute provided too few arguments")]
  public void Tick_NegativeRuleMatchesChildStatus_ReturnsFailure(NodeStatus childStatus, InterceptionRules rule,
    NodeStatus expectedStatus)
  {
    _childStatus = childStatus;
    var child = CreateMockChild();
    var node = new InterceptorNode<TestInput>(child, rule);

    var result = node.Tick(new());

    Assert.Multiple(() => {
        Assert.That(result, Is.EqualTo(expectedStatus), "Node should return Failure when negative rule matches.");
        Assert.That(_childTicked, Is.True, "Child should have been ticked.");
      }
    );
  }

  [Test]
  [TestCase(NodeStatus.Success, InterceptionRules.OnFailure)]
  [TestCase(NodeStatus.Failure, InterceptionRules.OnSuccess)]
  [TestCase(NodeStatus.Running, InterceptionRules.OnSuccess)]
  [TestCase(NodeStatus.Error, InterceptionRules.OnSuccess)]
  [TestCase(NodeStatus.Success, InterceptionRules.OnFailure | InterceptionRules.Negative)]
  [SuppressMessage("Structure", "NUnit1003:The TestCaseAttribute provided too few arguments")]
  public void Tick_RuleDoesNotMatchChildStatus_ReturnsChildStatus(NodeStatus childStatus, InterceptionRules rule)
  {
    _childStatus = childStatus;
    var child = CreateMockChild();
    var node = new InterceptorNode<TestInput>(child, rule);

    var result = node.Tick(new());

    Assert.Multiple(() => {
        Assert.That(result, Is.EqualTo(childStatus), "Node should return child status when rule does not match.");
        Assert.That(_childTicked, Is.True, "Child should have been ticked.");
      }
    );
  }

  [Test]
  [TestCase(InterceptionRules.Placeholder, NodeStatus.Success)]
  [TestCase(InterceptionRules.Placeholder | InterceptionRules.Negative, NodeStatus.Failure)]
  [SuppressMessage("Structure", "NUnit1003:The TestCaseAttribute provided too few arguments")]
  public void Tick_PlaceholderRule_ReturnsStatusWithoutTickingChild(InterceptionRules rule, NodeStatus expectedStatus)
  {
    var child = CreateMockChild();
    var node = new InterceptorNode<TestInput>(child, rule);

    var result = node.Tick(new());

    Assert.Multiple(() => {
        Assert.That(result, Is.EqualTo(expectedStatus), "Node should return expected status for Placeholder rule.");
        Assert.That(_childTicked, Is.False, "Child should NOT have been ticked with Placeholder rule.");
      }
    );
  }
}
