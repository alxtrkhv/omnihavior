namespace Omnihavior.Core;

public partial struct BehaviourBuilder<TInputData>;

public static class Omnihavior
{
  public static BehaviourBuilder<TInputData> Builder<TInputData>()
  {
    return new();
  }
}
