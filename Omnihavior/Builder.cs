using Omnihavior.Core;

namespace Omnihavior;

public static class Builder
{
  public static BehaviourBuilder<TInputData> Create<TInputData>()
  {
    return new();
  }
}
