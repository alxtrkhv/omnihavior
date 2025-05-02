using NSubstitute;
using Omnihavior.Core;
using Omnihavior.State;
using Omnihavior.Tests.Mocks;
using Omnihavior.Tests.Tree;

namespace Omnihavior.Tests.State;

[TestFixture]
public class StateMachineNodeTests : BaseNodeTests<StateMachineNode<TestInput>>
{
  protected override StateMachineNode<TestInput> CreateNodeForResetTests(out int? childrenNumber,
    params IBehaviorNode<TestInput>[] children)
  {
    childrenNumber = null;
    var states = children.Select(IStateNode<TestInput> (x, index) =>
      new LambdaStateNode<TestInput>(index.ToString(), reset: (input, _) => x.Reset(input))
    ).ToList();

    var stateMachine = new StateMachineNode<TestInput>("Root", states);
    stateMachine.InitializeRoot(CreateInputData());

    return stateMachine;
  }

  [Test]
  public void Exit_EntersNullState()
  {
    var input = CreateInputData();
    var state = new LambdaStateNode<TestInput>("State1");
    var stateMachine = new StateMachineNode<TestInput>("Root", [state,]);
    stateMachine.SetDefaultState("State1");
    stateMachine.InitializeRoot(input);

    stateMachine.Exit(input);

    Assert.That(stateMachine.CurrentState, Is.EqualTo(StateMachineNode<TestInput>.NullState));
  }

  [Test]
  public void Exit_ExitsSubState()
  {
    var input = CreateInputData();
    var mockState = Substitute.For<IStateNode<TestInput>>();
    mockState.Key.Returns("MockState");

    var stateMachine = new StateMachineNode<TestInput>("Root", [mockState,]);
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
    deepMockState.Key.Returns("DeepMockState");

    var innerStateMachine = new StateMachineNode<TestInput>("InnerSM", [deepMockState,]);
    innerStateMachine.SetDefaultState("DeepMockState");

    var outerStateMachine = new StateMachineNode<TestInput>("OuterSM", [innerStateMachine,]);
    outerStateMachine.SetDefaultState("InnerSM");

    var rootStateMachine = new StateMachineNode<TestInput>("Root", [outerStateMachine,]);
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
    deepMockState.Key.Returns("DeepMockState");

    var innerStateMachine = new StateMachineNode<TestInput>("InnerSM", [deepMockState,]);
    innerStateMachine.SetDefaultState("DeepMockState");

    var outerStateMachine = new StateMachineNode<TestInput>("OuterSM", [innerStateMachine,]);
    outerStateMachine.SetDefaultState("InnerSM");

    var rootStateMachine = new StateMachineNode<TestInput>("Root", [outerStateMachine,]);
    rootStateMachine.SetDefaultState("OuterSM");
    rootStateMachine.InitializeRoot(input);

    deepMockState.Received(1).Enter(input);
  }

  [Test]
  public void SetState_WhileInOneSubMachine_CanEnterAnotherSubMachinesStateDirectly()
  {
    var input = CreateInputData();
    var state1A = Substitute.For<IStateNode<TestInput>>();
    state1A.Key.Returns("State1A");
    var state2A = Substitute.For<IStateNode<TestInput>>();
    state2A.Key.Returns("State2A");
    var state2B = Substitute.For<IStateNode<TestInput>>();
    state2B.Key.Returns("State2B");

    var subSm1 = new StateMachineNode<TestInput>("SubSM1", [state1A,]);
    subSm1.SetDefaultState("State1A");

    var subSm2 = new StateMachineNode<TestInput>("SubSM2", [state2A, state2B,]);
    subSm2.SetDefaultState("State2A");

    var rootSm = new StateMachineNode<TestInput>("Root", [subSm1, subSm2,]);
    rootSm.SetDefaultState("SubSM1");

    rootSm.InitializeRoot(input);

    rootSm.SetState("State2B");
    rootSm.Exit(input);
    rootSm.Enter(input);

    Assert.Multiple(() => {
        state1A.Received(1).Enter(input);
        state1A.Received(1).Exit(input);
        state2A.DidNotReceive().Enter(input);
        state2B.Received(1).Enter(input);
        Assert.That(rootSm.CurrentState, Is.EqualTo(subSm2), "RootSM should be in SubSM2");
        Assert.That(subSm1.CurrentState, Is.EqualTo(StateMachineNode<TestInput>.NullState), "SubSM1 should be exited");
        Assert.That(subSm2.CurrentState, Is.EqualTo(state2B), "SubSM2 should be in State2B");
      }
    );
  }

  [Test]
  public void InitializeRoot_EntersDefaultState()
  {
    var input = CreateInputData();
    var defaultState = Substitute.For<IStateNode<TestInput>>();
    defaultState.Key.Returns("DefaultState");
    var otherState = Substitute.For<IStateNode<TestInput>>();
    otherState.Key.Returns("OtherState");

    var stateMachine = new StateMachineNode<TestInput>("Root", [otherState, defaultState,]);
    stateMachine.SetDefaultState("DefaultState");

    stateMachine.InitializeRoot(input);

    Assert.Multiple(() => {
        Assert.That(
          stateMachine.CurrentState,
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
    state1.Key.Returns("State1");
    var state2 = Substitute.For<IStateNode<TestInput>>();
    state2.Key.Returns("State2");

    var stateMachine = new StateMachineNode<TestInput>("Root", [state1, state2,]);

    stateMachine.InitializeRoot(input);

    Assert.Multiple(() => {
        Assert.That(
          stateMachine.CurrentState,
          Is.EqualTo(StateMachineNode<TestInput>.NullState),
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
    stateA.Key.Returns("StateA");
    var stateB = Substitute.For<IStateNode<TestInput>>();
    stateB.Key.Returns("StateB");

    var transition = new LambdaTransition<TestInput>("StateA", "StateB", _ => true);

    var stateMachine = new StateMachineNode<TestInput>("Root", [stateA, stateB,], [transition,]);
    stateMachine.SetDefaultState("StateA");

    stateMachine.InitializeRoot(input);

    var preTickState = stateMachine.CurrentState;

    stateMachine.Tick(input);

    Assert.Multiple(() => {
        Assert.That(preTickState, Is.EqualTo(stateA), "StateMachine should be in StateA before tick.");
        Assert.That(stateMachine.CurrentState, Is.EqualTo(stateB), "StateMachine should have transitioned to StateB.");
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
    stateA.Key.Returns("StateA");
    stateA.Tick(input).Returns(NodeStatus.Running);

    var stateB = Substitute.For<IStateNode<TestInput>>();
    stateB.Key.Returns("StateB");

    var transition = new LambdaTransition<TestInput>("StateA", "StateB", _ => false);

    var stateMachine = new StateMachineNode<TestInput>("Root", [stateA, stateB,], [transition,]);
    stateMachine.SetDefaultState("StateA");

    stateMachine.InitializeRoot(input);

    var preTickState = stateMachine.CurrentState;

    stateMachine.Tick(input);

    Assert.Multiple(() => {
        Assert.That(preTickState, Is.EqualTo(stateA), "StateMachine should be in StateA before tick.");
        Assert.That(stateMachine.CurrentState, Is.EqualTo(stateA), "StateMachine should remain in StateA.");
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
    mockState.Key.Returns("MockState");
    mockState.Tick(input).Returns(expectedStatus);

    var stateMachine = new StateMachineNode<TestInput>("Root", [mockState,]);
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
