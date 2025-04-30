using System;
using Omnihavior.Core;

namespace Omnihavior.Examples;

public enum PorterBotLocation
{
  Ship,
  Dock,
  ChargingBay,
}

public class PorterBotState
{
  public PorterBotLocation Location { get; set; }
  public int BoxesOnShip { get; set; }
  public int BoxesInDock { get; set; }

  public bool HasBoxInHands { get; set; }

  public void Reset()
  {
    HasBoxInHands = false;
    Location = PorterBotLocation.Dock;
  }
}

public class PorterBotBehavior : CustomBehavior<PorterBotState>
{
  public PorterBotBehavior()
  {
    Root = Builder.Selector(
      Builder.Sequence(IsThereBoxInHands(), GoTo(PorterBotLocation.Dock), LeaveBoxInDock()),
      Builder.Sequence(IsThereBoxOnShip(), GoTo(PorterBotLocation.Ship), TakeBoxFromShip()),
      GoTo(PorterBotLocation.ChargingBay).And(Idle().Throttle(3, NodeStatus.Running))
    );
  }

  public IBehaviorNode<PorterBotState> IsThereBoxOnShip() => Builder.Lambda(state => {
      if (state.BoxesOnShip > 0) {
        Log($"There are {state.BoxesOnShip} boxes on the ship.");
        return NodeStatus.Success;
      }

      Log("There are no boxes on the ship.");
      return NodeStatus.Failure;
    }
  );

  public IBehaviorNode<PorterBotState> IsThereBoxInHands() => Builder.Lambda(state => {
      if (state.HasBoxInHands) {
        Log("There is a box in the hands.");
        return NodeStatus.Success;
      }

      Log("There is no box in the hands.");
      return NodeStatus.Failure;
    }
  );

  public IBehaviorNode<PorterBotState> GoTo(PorterBotLocation location) => Builder.Lambda(state => {
      if (state.Location == location) {
        Log($"Already at {location}");
        return NodeStatus.Success;
      }

      Log($"Moving to {location}");
      state.Location = location;
      return NodeStatus.Running;
    }
  );

  public IBehaviorNode<PorterBotState> TakeBoxFromShip() => Builder.Lambda(state => {
      if (state.BoxesOnShip <= 0) {
        Log("Failed to find a box on the ship.");
        return NodeStatus.Failure;
      }

      state.HasBoxInHands = true;
      state.BoxesOnShip--;

      Log($"Taking a box from the ship. There are {state.BoxesOnShip} left on the ship.");
      return NodeStatus.Success;
    }
  );

  public IBehaviorNode<PorterBotState> LeaveBoxInDock() => Builder.Lambda(state => {
      if (!state.HasBoxInHands) {
        Log("Failed to find a box in the hands.");
        return NodeStatus.Failure;
      }

      state.HasBoxInHands = false;
      state.BoxesInDock++;
      Log($"Leaving a box in the dock. There are {state.BoxesInDock} currently in the dock.");
      return NodeStatus.Success;
    }
  );

  public IBehaviorNode<PorterBotState> Idle() => Builder.Lambda(state => {
      Log("Idling");
      return NodeStatus.Success;
    }
  );

  public override void Reset(PorterBotState input)
  {
    input.Reset();
  }

  private void Log(string message)
  {
    Console.WriteLine(message);
  }
}
