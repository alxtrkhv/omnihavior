using NSubstitute;
using Omnihavior.Core;
using Omnihavior.States;
using Omnihavior.Tests.Mocks;
using Omnihavior.Tests.Trees;

namespace Omnihavior.Tests.States;

[TestFixture]
public class StateMachineNodeTests : BaseNodeTests<StateMachineNode<string, TestInput>>
{
  protected override StateMachineNode<string, TestInput> CreateNodeForResetTests(out int? childrenNumber,
    params IBehaviorNode<TestInput>[] children)
  {
    childrenNumber = null;
    var stateNodes = children.Select(childNode =>
      new LambdaStateNode<TestInput>(reset: childNode.Reset)
    ).ToList();

    var stateMachine = new StateMachineNode<string, TestInput>();
    for (var i = 0; i < stateNodes.Count; i++) {
      var stateNode = stateNodes[i];
      stateMachine.AddState($"{i}", stateNode);
    }

    stateMachine.InitializeRoot(CreateInputData());

    return stateMachine;
  }

  [Test]
  public void Exit_WithoutDefaultState_EntersNullState()
  {
    var input = CreateInputData();
    var state = new LambdaStateNode<TestInput>();
    var stateMachine = new StateMachineNode<string, TestInput>();
    stateMachine.AddState("State1", state);
    stateMachine.InitializeRoot(input);

    stateMachine.Exit(input);

    Assert.That(stateMachine.CurrentState, Is.EqualTo(StateMachineNode<string, TestInput>.NullState));
  }

  [Test]
  public void Exit_ExitsSubState()
  {
    var input = CreateInputData();
    var mockState = Substitute.For<IStateNode<TestInput>>();

    var stateMachine = new StateMachineNode<string, TestInput>();
    stateMachine.AddState("MockState", mockState);
    stateMachine.SetDefaultState("MockState");
    stateMachine.InitializeRoot(input);

    stateMachine.Exit(input);

    mockState.Received(1).Exit(input);
  }

  [Test]
  public void Exit_ExitsAllSubStatesRecursively()
  {
    var input = CreateInputData();
    var deepMockState = Substitute.For<IStateNode<TestInput>>();

    var innerStateMachine = new StateMachineNode<string, TestInput>();
    innerStateMachine.AddState("DeepMockState", deepMockState);
    innerStateMachine.SetDefaultState("DeepMockState");

    var outerStateMachine = new StateMachineNode<string, TestInput>();
    outerStateMachine.AddState("InnerSM", innerStateMachine);
    outerStateMachine.SetDefaultState("InnerSM");

    var rootStateMachine = new StateMachineNode<string, TestInput>();
    rootStateMachine.AddState("OuterSM", outerStateMachine);
    rootStateMachine.SetDefaultState("OuterSM");
    rootStateMachine.InitializeRoot(input);

    rootStateMachine.Exit(input);

    deepMockState.Received(1).Exit(input);
  }

  [Test]
  public void Enter_EntersAllSubStatesRecursively()
  {
    var input = CreateInputData();
    var deepMockState = Substitute.For<IStateNode<TestInput>>();

    var innerStateMachine = new StateMachineNode<string, TestInput>();
    innerStateMachine.AddState("DeepMockState", deepMockState);
    innerStateMachine.SetDefaultState("DeepMockState");

    var outerStateMachine = new StateMachineNode<string, TestInput>();
    outerStateMachine.AddState("InnerSM", innerStateMachine);
    outerStateMachine.SetDefaultState("InnerSM");

    var rootStateMachine = new StateMachineNode<string, TestInput>();
    rootStateMachine.AddState("OuterSM", outerStateMachine);
    rootStateMachine.SetDefaultState("OuterSM");
    rootStateMachine.InitializeRoot(input);

    deepMockState.Received(1).Enter(input);
  }

  [Test]
  public void InitializeRoot_EntersDefaultState()
  {
    var input = CreateInputData();
    var defaultState = Substitute.For<IStateNode<TestInput>>();
    var otherState = Substitute.For<IStateNode<TestInput>>();

    var stateMachine = new StateMachineNode<string, TestInput>();
    stateMachine.AddState("DefaultState", defaultState);
    stateMachine.AddState("OtherState", otherState);
    stateMachine.SetDefaultState("DefaultState");

    stateMachine.InitializeRoot(input);

    Assert.Multiple(() => {
        Assert.That(
          stateMachine.CurrentState.Value,
          Is.EqualTo(defaultState),
          "StateMachine should be in the default state."
        );
        defaultState.Received(1).Enter(input);
        otherState.DidNotReceive().Enter(input);
      }
    );
  }

  [Test]
  public void InitializeRoot_WhenNoDefaultStateSet_EntersNullState()
  {
    var input = CreateInputData();
    var state1 = Substitute.For<IStateNode<TestInput>>();
    var state2 = Substitute.For<IStateNode<TestInput>>();

    var stateMachine = new StateMachineNode<string, TestInput>();
    stateMachine.AddState("State1", state1);
    stateMachine.AddState("State2", state2);

    stateMachine.InitializeRoot(input);

    Assert.Multiple(() => {
        Assert.That(
          stateMachine.CurrentState,
          Is.EqualTo(StateMachineNode<string, TestInput>.NullState),
          "StateMachine should be in NullState when no default is set."
        );
        state1.DidNotReceive().Enter(input);
        state2.DidNotReceive().Enter(input);
      }
    );
  }

  [Test]
  public void Tick_WhenValidTransitionIsAvailable_PerformsTransition()
  {
    var input = CreateInputData();
    var stateA = Substitute.For<IStateNode<TestInput>>();
    var stateB = Substitute.For<IStateNode<TestInput>>();

    var transition = new LambdaTransition<string, TestInput>("StateA", "StateB", _ => true);

    var stateMachine = new StateMachineNode<string, TestInput>();
    stateMachine.AddState("StateA", stateA);
    stateMachine.AddState("StateB", stateB);
    stateMachine.AddTransition(transition);
    stateMachine.SetDefaultState("StateA");

    stateMachine.InitializeRoot(input);

    var preTickState = stateMachine.CurrentState;

    stateMachine.Tick(input);

    Assert.Multiple(() => {
        Assert.That(preTickState.Value, Is.EqualTo(stateA), "StateMachine should be in StateA before tick.");
        Assert.That(
          stateMachine.CurrentState.Value,
          Is.EqualTo(stateB),
          "StateMachine should have transitioned to StateB."
        );
        stateA.Received(1).Exit(input);
        stateB.Received(1).Enter(input);
      }
    );
  }

  [Test]
  public void Tick_WhenNoValidTransitionIsAvailable_StaysInCurrentStateAndTicks()
  {
    var input = CreateInputData();
    var stateA = Substitute.For<IStateNode<TestInput>>();
    stateA.Tick(input).Returns(NodeStatus.Running);

    var stateB = Substitute.For<IStateNode<TestInput>>();

    var transition = new LambdaTransition<string, TestInput>("StateA", "StateB", _ => false);

    var stateMachine = new StateMachineNode<string, TestInput>();
    stateMachine.AddState("StateA", stateA);
    stateMachine.AddState("StateB", stateB);
    stateMachine.AddTransition(transition);
    stateMachine.SetDefaultState("StateA");

    stateMachine.InitializeRoot(input);

    var preTickState = stateMachine.CurrentState;

    stateMachine.Tick(input);

    Assert.Multiple(() => {
        Assert.That(preTickState.Value, Is.EqualTo(stateA), "StateMachine should be in StateA before tick.");
        Assert.That(stateMachine.CurrentState.Value, Is.EqualTo(stateA), "StateMachine should remain in StateA.");
        stateA.Received(1).Tick(input);
        stateA.DidNotReceive().Exit(input);
        stateB.DidNotReceive().Enter(input);
      }
    );
  }

  [TestCase(NodeStatus.Success)]
  [TestCase(NodeStatus.Failure)]
  [TestCase(NodeStatus.Running)]
  [TestCase(NodeStatus.Error)]
  public void Tick_ReturnsStatesStatus(NodeStatus expectedStatus)
  {
    var input = CreateInputData();
    var mockState = Substitute.For<IStateNode<TestInput>>();
    mockState.Tick(input).Returns(expectedStatus);

    var stateMachine = new StateMachineNode<string, TestInput>();
    stateMachine.AddState("MockState", mockState);
    stateMachine.SetDefaultState("MockState");

    stateMachine.InitializeRoot(input);

    var actualStatus = stateMachine.Tick(input);

    Assert.Multiple(() => {
        Assert.That(
          actualStatus,
          Is.EqualTo(expectedStatus),
          $"StateMachine should return {expectedStatus} when the current state returns it."
        );
        mockState.Received(1).Tick(input);
      }
    );
  }
}
