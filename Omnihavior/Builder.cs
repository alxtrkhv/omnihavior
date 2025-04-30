using System;
using System.Collections.Generic;
using Omnihavior.Core;

namespace Omnihavior;

/// <summary>
/// Provides static methods for creating instances of <see cref="BehaviourBuilder{TInputData}"/>.
/// </summary>
public static class Builder
{
  /// <summary>
  /// Stores the default builder instances for each input data type.
  /// </summary>
  private static readonly Dictionary<Type, IBehaviourBuilder> DefaultBuilders = new();

  public static BehaviourBuilderSettings DefaultSettings { get; } = new();

  /// <summary>
  /// Gets the default <see cref="BehaviourBuilder{TInputData}"/> for the specified input data type.
  /// If a default builder does not exist for the type, a new one is created and stored.
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the behavior will handle.</typeparam>
  /// <returns>The default <see cref="BehaviourBuilder{TInputData}"/> instance.</returns>
  public static BehaviourBuilder<TInputData> Default<TInputData>()
  {
    if (DefaultBuilders.TryGetValue(typeof(TInputData), out var existingBuilder)) {
      return (BehaviourBuilder<TInputData>)existingBuilder;
    }

    var newBuilder = Create<TInputData>();
    DefaultBuilders[typeof(TInputData)] = newBuilder;
    return newBuilder;
  }

  /// <summary>
  /// Creates a new instance of <see cref="BehaviourBuilder{TInputData}"/>.
  /// </summary>
  /// <typeparam name="TInputData">The type of input data the behavior will handle.</typeparam>
  /// <returns>A new <see cref="BehaviourBuilder{TInputData}"/> instance.</returns>
  public static BehaviourBuilder<TInputData> Create<TInputData>()
  {
    return new();
  }
}
