using System.Diagnostics.CodeAnalysis;
using Omnihavior.Core;
using Omnihavior.Tests.Mocks;
using Omnihavior.Tree;

namespace Omnihavior.Tests.Tree;

[TestFixture]
public class ResetterNodeTests : BaseNodeTests<ResetterNode<TestInput>>
{
  private IBehaviorNode<TestInput> _mockChild = null!;
  private bool _childResetCalled;

  [SetUp]
  public void SetUp()
  {
    _childResetCalled = false;
    _mockChild = new LambdaNode<TestInput>(_ => NodeStatus.Success, _ => _childResetCalled = true);
  }

  protected override ResetterNode<TestInput> CreateNodeForResetTests(out int? childrenNumber,
    params IReadOnlyList<IBehaviorNode<TestInput>> children)
  {
    childrenNumber = 1;
    _mockChild = children[0];
    return new(_mockChild, ResetRules.Always);
  }

  private ResetterNode<TestInput> CreateSpecificNode(NodeStatus childStatus, ResetRules rules)
  {
    _mockChild = new LambdaNode<TestInput>(_ => childStatus, _ => _childResetCalled = true);
    return new(_mockChild, rules);
  }

  [Test]
  [TestCase(NodeStatus.Success)]
  [TestCase(NodeStatus.Failure)]
  [TestCase(NodeStatus.Running)]
  [TestCase(NodeStatus.Error)]
  [SuppressMessage("Structure", "NUnit1003:The TestCaseAttribute provided too few arguments")]
  public void Tick_ReturnsChildStatus(NodeStatus expectedStatus)
  {
    var node = CreateSpecificNode(expectedStatus, ResetRules.Always);
    var result = node.Tick(new());
    Assert.That(result, Is.EqualTo(expectedStatus));
  }

  [Test]
  [TestCase(NodeStatus.Success, ResetRules.Always, true)]
  [TestCase(NodeStatus.Failure, ResetRules.Always, true)]
  [TestCase(NodeStatus.Running, ResetRules.Always, true)]
  [TestCase(NodeStatus.Error, ResetRules.Always, true)]
  [TestCase(NodeStatus.Success, ResetRules.OnSuccess, true)]
  [TestCase(NodeStatus.Failure, ResetRules.OnSuccess, false)]
  [TestCase(NodeStatus.Running, ResetRules.OnSuccess, false)]
  [TestCase(NodeStatus.Error, ResetRules.OnSuccess, false)]
  [TestCase(NodeStatus.Success, ResetRules.OnFailure, false)]
  [TestCase(NodeStatus.Failure, ResetRules.OnFailure, true)]
  [TestCase(NodeStatus.Running, ResetRules.OnFailure, false)]
  [TestCase(NodeStatus.Error, ResetRules.OnFailure, false)]
  [TestCase(NodeStatus.Success, ResetRules.OnRunning, false)]
  [TestCase(NodeStatus.Failure, ResetRules.OnRunning, false)]
  [TestCase(NodeStatus.Running, ResetRules.OnRunning, true)]
  [TestCase(NodeStatus.Error, ResetRules.OnRunning, false)]
  [TestCase(NodeStatus.Success, ResetRules.OnError, false)]
  [TestCase(NodeStatus.Failure, ResetRules.OnError, false)]
  [TestCase(NodeStatus.Running, ResetRules.OnError, false)]
  [TestCase(NodeStatus.Error, ResetRules.OnError, true)]
  [SuppressMessage("Structure", "NUnit1003:The TestCaseAttribute provided too few arguments")]
  public void Tick_ResetsChildBasedOnRules(NodeStatus childStatus, ResetRules rules, bool shouldReset)
  {
    var node = CreateSpecificNode(childStatus, rules);
    node.Tick(new());
    Assert.That(_childResetCalled, Is.EqualTo(shouldReset));
  }
}
