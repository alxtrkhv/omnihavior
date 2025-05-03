namespace Omnihavior.Core;

public abstract partial class CustomBehavior<TInputData> : IBehaviorNode<TInputData>
{
  protected IBehaviorNode<TInputData> Root
  {
    get => _root;
    set => _root = value;
  }

  protected BehaviourBuilder<TInputData> Builder
  {
    get => _builder ??= Omnihavior.Builder.Default<TInputData>();
    set => _builder = value;
  }

  private IBehaviorNode<TInputData> _root = null!;
  private BehaviourBuilder<TInputData>? _builder;

  public NodeStatus Tick(TInputData input)
  {
    return _root.Tick(input);
  }

  public virtual void Reset(TInputData input)
  {
    _root.Reset(input);
  }
}
