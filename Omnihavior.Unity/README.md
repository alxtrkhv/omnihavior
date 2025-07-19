# Omnihavior

Omnihavior is a C# library that allows you to combine different approaches to building AI behaviors popular in game
development. It's built around behavior trees but also contains utility AI and state machine functionality that can
be combined with behavior trees and each other in different ways.

## Features

- **Behavior Trees**: Complete implementation with composite, decorator, and leaf nodes
- **Utility AI**: Score-based decision-making
- **State Machines**: Hierarchical state management with transitions
- **Tree Node System**: All nodes implement `IBehaviorNode<TInputData>`
- **Configurable Rules**: Customizable behavior through rule enums with meaningful defaults

## Core Concepts

### Node Status

Every node returns one of four possible statuses:

- `Success`: Node completed successfully
- `Failure`: Node failed to complete
- `Running`: Node is still executing (requires more ticks)
- `Error`: Node encountered an error

### Node Types

#### Composite Nodes

- **SequenceNode**: Executes children sequentially until one fails or all succeed
- **SelectorNode**: Executes children sequentially until one succeeds
- **ParallelNode**: Executes all children simultaneously with configurable failure tolerance
- **ConditionalNode**: Executes different branches based on a condition

#### Decorator Nodes

- **InterceptorNode**: Intercepts and modifies child node results based on rules
- **InverterNode**: Inverts Success/Failure results
- **ThrottleNode**: Limits child execution frequency
- **LimitNode**: Restricts the number of successful executions
- **ResetterNode**: Automatically resets child nodes based on rules

#### Leaf Nodes

- **LambdaNode**: Executes custom lambda functions
- **FakeNode**: Returns predefined status sequences (useful for testing)

#### Utility AI Nodes

- **UtilityNode**: Selects and executes the child with the highest utility score
- **LambdaEvaluatableNode**: Combines behavior execution with utility evaluation
- **LambdaEvaluation**: Provides utility scoring through lambda functions

#### State Machine Nodes

- **StateMachineNode**: Manages multiple states with transitions between them
- **LambdaStateNode**: State implementation using lambda functions for behavior
- **LambdaTransition**: Transition implementation using lambda functions for conditions

## Usage

### Direct Node Creation

```csharp
// Create nodes directly
var condition = new LambdaNode<GameState>(state => 
    state.Health > 50 ? NodeStatus.Success : NodeStatus.Failure);

var attack = new LambdaNode<GameState>(state => {
    state.Attack();
    return NodeStatus.Success;
});

var flee = new LambdaNode<GameState>(state => {
    state.Flee();
    return NodeStatus.Success;
});

// Compose into a conditional behavior
var behavior = new ConditionalNode<GameState>(condition, attack, flee);
```

### Using BehaviourBuilder

```csharp
// Using default instance (one instance per input type)
var defaultBuilder = Omnihavior.Builder.Default<GameState>();

// Using specific instance 
var builder = Omnihavior.Builder.Create<GameState>();

var behavior = builder.Conditional(
    builder.Lambda(state => state.Health > 50 ? NodeStatus.Success : NodeStatus.Failure),
    builder.Lambda(state => { state.Attack(); return NodeStatus.Success; }),
    builder.Lambda(state => { state.Flee(); return NodeStatus.Success; })
);
```

### Using Extension Methods

```csharp
var behavior = healthCheck
    .AsCondition(attackBehavior, fleeBehavior)
    .Throttle(runOnceInInterval: 5)
    .WithLimit(3);
```

### Custom Behaviors

```csharp
public class GuardBehavior : CustomBehavior<GameState>
{
  public GuardBehavior()
  {
    Root = Builder.Selector(
      Builder.Sequence(
        Builder.Lambda(state => state.EnemyInSight ? NodeStatus.Success : NodeStatus.Failure),
        Builder.Lambda(state => {
            state.Attack();
            return NodeStatus.Success;
          }
        )
      ),
      Builder.Lambda(state => {
          state.Patrol();
          return NodeStatus.Success;
        }
      )
    );
  }
}
```

### Behavior Trees

Behavior trees are the core of Omnihavior, providing a hierarchical structure for organizing AI logic. They consist of
composite nodes (that manage multiple children), decorator nodes (that modify a single child's behavior), and leaf
nodes (that perform actual actions).

#### Composite Nodes

Composite nodes manage multiple child nodes and determine execution flow:

```csharp
// Sequence: Execute children in order until one fails or all succeed
var attackSequence = builder.Sequence(
    builder.Lambda(state => state.HasTarget ? NodeStatus.Success : NodeStatus.Failure),
    builder.Lambda(state => state.MoveToTarget()),
    builder.Lambda(state => state.Attack())
);

// Selector: Execute children in order until one succeeds
var combatSelector = builder.Selector(
    builder.Lambda(state => state.CanAttack ? NodeStatus.Success : NodeStatus.Failure),
    builder.Lambda(state => state.CanDefend ? NodeStatus.Success : NodeStatus.Failure),
    builder.Lambda(state => { state.Flee(); return NodeStatus.Success; })
);

// Parallel: Execute all children simultaneously
var parallelBehavior = builder.Parallel(
    builder.Lambda(state => { state.Move(); return NodeStatus.Running; }),
    builder.Lambda(state => { state.Scan(); return NodeStatus.Success; }),
    builder.Lambda(state => { state.Communicate(); return NodeStatus.Success; })
);

// Conditional: Execute different branches based on condition
var healthCheck = builder.Conditional(
    condition: builder.Lambda(state => state.Health > 50 ? NodeStatus.Success : NodeStatus.Failure),
    positiveNode: attackBehavior,
    negativeNode: healBehavior
);
```

#### Using Extension Methods for Composites

```csharp
// Convert arrays to composite nodes
var patrolPoints = new[] { point1, point2, point3 }
    .AsSequence(SequenceRules.IgnoreChildsFailure);

var attackOptions = new[] { meleeAttack, rangedAttack, specialAttack }
    .AsSelector(SelectorRules.InterceptFlowsFailure);

// Extensions for combining nodes
var behavior = moveToTarget.And(aimAtTarget); // Creates two nodes sequence, equal to logical AND

var fallbackBehavior = primaryAttack.Or(secondaryAttack); // Creates two nodes selector, equal to logical OR
```

#### Composite Node Rules

Composite nodes support rules to customize their behavior:

```csharp
// Sequence that continues even if children fail
var resilientSequence = builder.Sequence(children, 
    SequenceRules.InterceptChildsFailure | SequenceRules.IgnoreChildsFailure);

// Selector that always succeeds (never returns Failure)
var guaranteedSelector = builder.Selector(children, 
    SelectorRules.InterceptFlowsFailure);

// Parallel with failure tolerance (allows 2 children to fail)
var tolerantParallel = builder.Parallel(children, failureAllowance: 2);

// Conditional with cached condition and failure interception
var smartConditional = builder.Conditional(condition, positive, negative,
    ConditionRules.CacheCondition | ConditionRules.InterceptChildsFailure);
```

#### Decorator Nodes

Decorator nodes wrap a single child and modify its behavior:

```csharp
// Interceptor: Change child's return status based on rules
var alwaysSucceed = builder.Interceptor(
    child: riskyOperation,
    rules: InterceptionRules.OnFailure,
    successStatus: NodeStatus.Success
);

// Inverter: Flip Success/Failure results
var notCondition = builder.Inverter(
    builder.Lambda(state => state.HasAmmo ? NodeStatus.Success : NodeStatus.Failure)
);

// Throttle: Limit execution frequency
var expensiveCheck = builder.Throttle(
    child: pathfindingNode,
    runOnceInInterval: 5,
    status: NodeStatus.Success,
    rules: ThrottleRules.CacheLastRunResult
);

// Limit: Restrict number of successful executions
var consumableAction = builder.Limit(
    child: useHealthPotion,
    limit: 3
);

// Resetter: Automatically reset child based on rules
var autoResetBehavior = builder.Resetter(
    child: statefulBehavior,
    rules: ResetRules.OnResult
);
```

#### Using Extension Methods for Decorators

```csharp
// for decorating nodes
var robustBehavior = dangerousOperation
    .Intercept(InterceptionRules.OnFailure, NodeStatus.Success)
    .Throttle(runOnceInInterval: 3)
    .WithLimit(5)
    .WithReset(ResetRules.OnSuccess);

// Common patterns
var oneTimeAction = initialization.Once();  // Limit to 1 execution
var invertedCondition = condition.Invert(); // Flip Success/Failure
var cachedExpensiveCheck = expensiveOperation
    .Throttle(10, NodeStatus.Success, ThrottleRules.CacheLastRunResult);
```

#### Decorator Rules and Options

```csharp
// Interceptor rules - when to change child's status
InterceptionRules.OnSuccess     // Intercept Success results
InterceptionRules.OnFailure     // Intercept Failure results  
InterceptionRules.OnRunning     // Intercept Running results
InterceptionRules.OnError       // Intercept Error results
InterceptionRules.Always        // Intercept all results
InterceptionRules.SkipChildTick // Return new status without ticking child

// Throttle rules - how to handle cached results
ThrottleRules.CacheLastRunResult // Use child's last result when throttled
ThrottleRules.None              // Use provided cached status when throttled

// Reset rules - when to reset the child
ResetRules.OnSuccess    // Reset after Success
ResetRules.OnFailure    // Reset after Failure
ResetRules.OnResult     // Reset after Success or Failure (default)
ResetRules.OnRunning    // Reset after Running
ResetRules.OnError      // Reset after Error
ResetRules.Always       // Reset after any status
```

#### Complex Behavior Tree Example

```csharp
var guardAI = builder.Selector(
    // Combat behavior - high priority
    builder.Sequence(
        builder.Lambda(state => state.EnemyInSight ? NodeStatus.Success : NodeStatus.Failure),
        builder.Selector(
            // Try ranged attack first
            builder.Sequence(
                builder.Lambda(state => state.HasAmmo ? NodeStatus.Success : NodeStatus.Failure),
                builder.Lambda(state => state.InRange ? NodeStatus.Success : NodeStatus.Failure),
                builder.Lambda(state => { state.RangedAttack(); return NodeStatus.Success; })
            ).Throttle(2), // Don't spam attacks
            
            // Fall back to melee
            builder.Sequence(
                builder.Lambda(state => { state.MoveToEnemy(); return NodeStatus.Running; }),
                builder.Lambda(state => { state.MeleeAttack(); return NodeStatus.Success; })
            )
        )
    ),
    
    // Patrol behavior - medium priority  
    builder.Sequence(
        builder.Lambda(state => !state.AtPatrolPoint ? NodeStatus.Success : NodeStatus.Failure),
        builder.Lambda(state => { state.MoveToNextPatrolPoint(); return NodeStatus.Running; })
    ).WithReset(ResetRules.OnSuccess), // Reset when reaching patrol point
    
    // Idle behavior - lowest priority (always succeeds)
    builder.Lambda(state => { state.LookAround(); return NodeStatus.Success; })
        .Throttle(5) // Don't look around every tick
);
```

### Utility AI

Utility AI allows nodes to be selected based on numerical scores, enabling dynamic decision-making:

```csharp
// Create evaluatable nodes that can both execute and be scored
var attackNode = new LambdaEvaluatableNode<GameState>(
    tick: state => { state.Attack(); return NodeStatus.Success; },
    evaluate: state => state.EnemyDistance < 5 ? 0.9f : 0.1f
);

var defendNode = new LambdaEvaluatableNode<GameState>(
    tick: state => { state.Defend(); return NodeStatus.Success; },
    evaluate: state => state.Health < 30 ? 0.8f : 0.2f
);

var fleeNode = new LambdaEvaluatableNode<GameState>(
    tick: state => { state.Flee(); return NodeStatus.Success; },
    evaluate: state => state.Health < 20 ? 1.0f : 0.0f
);

// Create utility node that selects highest scoring child each tick
var utilityBehavior = new UtilityNode<GameState>(
    new[] { attackNode, defendNode, fleeNode },
    UtilityRules.IfEqualSelectLast,
    minEvaluationThreshold: 0.1f,
    lastNodeBonus: 0.1f
);
```

#### Using Builder for Utility AI

```csharp
var builder = Builder.Create<GameState>();

var utilityBehavior = builder.Utility(
    new[] {
        builder.LambdaEvaluatableNode(
            state => { state.Attack(); return NodeStatus.Success; },
            state => state.EnemyDistance < 5 ? 0.9f : 0.1f
        ),
        builder.LambdaEvaluatableNode(
            state => { state.Defend(); return NodeStatus.Success; },
            state => state.Health < 30 ? 0.8f : 0.2f
        )
    },
    UtilityRules.IfEqualSelectLast,
    minEvaluationThreshold: 0.1f,
    lastNodeBonus: 0.1f
);
```

#### Separate Evaluations and Nodes

```csharp
// Create separate evaluation functions and behavior nodes
var evaluations = new IEvaluatable<GameState>[] {
    builder.LambdaEvaluation(state => state.Hunger > 0.7f ? 0.9f : 0.1f),
    builder.LambdaEvaluation(state => state.Energy < 0.3f ? 0.8f : 0.2f),
    builder.LambdaEvaluation(state => state.EnemyNear ? 0.7f : 0.0f)
};

var behaviors = new IBehaviorNode<GameState>[] {
    builder.Lambda(state => { state.FindFood(); return NodeStatus.Success; }),
    builder.Lambda(state => { state.Rest(); return NodeStatus.Success; }),
    builder.Lambda(state => { state.Fight(); return NodeStatus.Success; })
};

var utilityNode = builder.Utility(evaluations, behaviors);
```

#### Extension Methods for Utility AI

```csharp
// Convert existing nodes to utility-based selection
var utilityBehavior = new[] { attackNode, defendNode, fleeNode }
    .AsUtility(
        new[] { attackEval, defendEval, fleeEval },
        UtilityRules.IfEqualSelectLast,
        minEvaluationThreshold: 0.1f,
        lastNodeBonus: 0.1f
    );
```

#### Utility AI Rules
********
```csharp
// If multiple nodes have equal scores, select the last one
UtilityRules.IfEqualSelectLast

// Return Success if no children exist (instead of Failure)
UtilityRules.InterceptFlowsFailureIfEmpty

// Return Success if no child meets threshold (instead of Failure)  
UtilityRules.InterceptFlowsFailureIfNoActionPassesThreshold

// Always return Success even if selected child fails
UtilityRules.InterceptChildsFailure

// Combine multiple rules
var rules = UtilityRules.IfEqualSelectLast | UtilityRules.InterceptChildsFailure;
```

### State Machines

State machines provide structured state management with explicit transitions and lifecycle events. Each state can
contain its own behavior logic, and transitions define when and how to move between states.

#### Basic State Machine

```csharp
// Define state keys (can be any type - enum, string, int, etc.)
enum AIState { Idle, Patrol, Chase, Attack }

// Create state machine
var stateMachine = new StateMachineNode<AIState, GameState>();

// Add states with lambda-based behavior
stateMachine.AddState(AIState.Idle, new LambdaStateNode<GameState>(
    tick: state => { state.LookAround(); return NodeStatus.Success; },
    enter: state => Console.WriteLine("Entering idle state"),
    exit: state => Console.WriteLine("Leaving idle state")
));

stateMachine.AddState(AIState.Patrol, new LambdaStateNode<GameState>(
    tick: state => { state.Patrol(); return NodeStatus.Running; }
));

stateMachine.AddState(AIState.Chase, new LambdaStateNode<GameState>(
    tick: state => { 
        state.ChasePlayer(); 
        return state.PlayerInRange ? NodeStatus.Success : NodeStatus.Running; 
    }
));

// Add transitions
stateMachine.AddTransition(new LambdaTransition<AIState, GameState>(
    from: AIState.Idle, 
    to: AIState.Patrol, 
    condition: state => state.TimeSinceLastAction > 3.0f
));

stateMachine.AddTransition(new LambdaTransition<AIState, GameState>(
    from: AIState.Patrol, 
    to: AIState.Chase, 
    condition: state => state.PlayerSpotted
));

// Global transition (can trigger from any state)
stateMachine.AddTransition(new LambdaTransition<AIState, GameState>(
    from: null, 
    to: AIState.Idle, 
    condition: state => state.Health <= 0
));

// Set default starting state
stateMachine.SetDefaultState(AIState.Idle);
```

#### Using BehaviourBuilder for State Machines

```csharp
var builder = Builder.Create<GameState>();

var stateMachine = builder.StateMachine<AIState>();

// Add states using builder methods
stateMachine.AddState(AIState.Idle, 
    builder.LambdaState(
        tick: state => { state.Rest(); return NodeStatus.Success; },
        enter: state => state.StartResting()
    )
);

stateMachine.AddState(AIState.Attack, 
    builder.LambdaState(
        tick: state => { 
            state.AttackPlayer(); 
            return state.AttackComplete ? NodeStatus.Success : NodeStatus.Running; 
        }
    )
);

// Add transitions using builder methods
stateMachine.AddTransition(
    builder.LambdaTransition(AIState.Idle, AIState.Attack, 
        condition: state => state.PlayerInRange)
);

// Global transition using builder
stateMachine.AddTransition(
    builder.LambdaTransition(AIState.Idle, 
        condition: state => state.ShouldReset)
);

stateMachine.SetDefaultState(AIState.Idle);
```

#### Using Extension Methods

```csharp
var stateMachine = builder.StateMachine<AIState>();

// Adding states and transitions
stateMachine.AddState(AIState.Idle, 
    tick: state => { state.Idle(); return NodeStatus.Success; },
    enter: state => state.ResetTimers()
);

stateMachine.AddTransition(AIState.Idle, AIState.Patrol, 
    condition: state => state.ShouldStartPatrol);

stateMachine.AddTransition(AIState.Patrol, AIState.Chase, 
    condition: state => state.EnemyDetected);

// Global transition (from any state)
stateMachine.AddTransition(AIState.Idle, 
    condition: state => state.EmergencyStop);
```

#### Hierarchical State Machines

State machines can contain other behavior nodes, including other state machines:

```csharp
// Create sub-state machine for combat behavior
var combatStateMachine = builder.StateMachine<CombatState>();
combatStateMachine.AddState(CombatState.Melee, meleeAttackBehavior);
combatStateMachine.AddState(CombatState.Ranged, rangedAttackBehavior);
combatStateMachine.SetDefaultState(CombatState.Melee);

// Use combat state machine as a state in main state machine
var mainStateMachine = builder.StateMachine<AIState>();
mainStateMachine.AddState(AIState.Combat, combatStateMachine);
mainStateMachine.AddState(AIState.Patrol, patrolBehavior);

// Transition to combat state
mainStateMachine.AddTransition(AIState.Patrol, AIState.Combat, 
    condition: state => state.EnemyNear);
```

#### State Machine Rules

State machines support various rules to customize their behavior:

```csharp
// Create state machine with custom rules
var stateMachine = builder.StateMachine<AIState>(
    StateMachineRules.InterceptChildsFailure | StateMachineRules.NonBlockingErrors
);

// Available rules:
// - InterceptChildsFailure: Return Success even if current state fails
// - InterceptChildsSuccess: Return Running when current state succeeds  
// - NonBlockingErrors: Allow transitions even when current state has errors
```

#### State Lifecycle Events

States support Enter, Exit, Tick, and Reset lifecycle events:

```csharp
var guardState = new LambdaStateNode<GameState>(
    tick: state => {
        state.ScanForEnemies();
        return state.EnemyFound ? NodeStatus.Success : NodeStatus.Running;
    },
    enter: state => {
        state.SetAlertLevel(AlertLevel.Normal);
        state.StartScanning();
    },
    exit: state => {
        state.StopScanning();
        state.SaveLastKnownPosition();
    },
    reset: state => {
        state.ClearMemory();
        state.ResetPosition();
    }
);
```

## Configuration

### Rules and Defaults

Many nodes accept rule enums that modify their behavior:

```csharp
// Sequence that continues even if children fail
var sequence = builder.Sequence(children, SequenceRules.IgnoreChildsFailure);

// Selector that always succeeds
var selector = builder.Selector(children, SelectorRules.InterceptFlowsFailure);

// Conditional with cached condition evaluation
var conditional = builder.Conditional(condition, positive, negative, 
    ConditionRules.CacheCondition | ConditionRules.InterceptChildsFailure);
```

### Builder Settings

Configure default behaviors through `BehaviourBuilderSettings`:

```csharp
var builder = Builder.Create<GameState>();
builder.Settings.DefaultSequenceRules = SequenceRules.InterceptChildsFailure;
builder.Settings.DefaultThrottleOnceInInterval = 3;
builder.Settings.DefaultLimit = 5;

// Utility AI defaults
builder.Settings.DefaultUtilityRules = UtilityRules.IfEqualSelectLast;
builder.Settings.DefaultUtilityMinEvaluationThreshold = 0.1f;
builder.Settings.DefaultUtilityLastNodeBonus = 0.05f;

// State Machine defaults
builder.Settings.DefaultStateMachineRules = StateMachineRules.InterceptChildsFailure;
```
