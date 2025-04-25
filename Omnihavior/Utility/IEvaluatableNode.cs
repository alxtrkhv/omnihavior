using Omnihavior.Core;

namespace Omnihavior.Utility;

public interface IEvaluatableNode<TInputData> : IBehaviorNode<TInputData>, IEvaluatable<TInputData>;
