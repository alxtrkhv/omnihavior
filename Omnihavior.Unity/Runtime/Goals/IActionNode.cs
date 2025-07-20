using System.Collections.Generic;
using Omnihavior.Core;

namespace Omnihavior.Goals
{
  public interface IActionNode<TInputData> : IBehaviorNode<TInputData>
  {
    public IReadOnlyList<ICondition<TInputData>> Conditions { get; }
    public IReadOnlyList<IEffect<TInputData>> Effects { get; }
  }
}
