namespace Omnihavior.Examples;

public static class Scenarios
{
  public static void RunPorterBotScenario()
  {
    var state = new PorterBotState {
      BoxesOnShip = 5,
      BoxesInDock = 0,
      HasBoxInHands = false,
      Location = PorterBotLocation.ChargingBay,
    };
    var behavior = new PorterBotBehavior();

    for (var i = 0; i < 25; i++) {
      behavior.Tick(state);
    }
  }
}
