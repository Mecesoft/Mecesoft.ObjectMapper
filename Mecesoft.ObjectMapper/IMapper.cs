namespace Mecesoft.ObjectMapper;

/// <summary>
/// Maps objects between configured source and destination types.
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Maps a source object to <typeparamref name="TDestination"/> using the source object's
    /// runtime type.
    /// </summary>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <returns>The mapped destination object.</returns>
    /// <exception cref="InvalidOperationException">
    /// No match for the runtime source type and destination type is configured.
    /// </exception>
    /// <exception cref="MappingException">
    /// A member value cannot be converted or a nested mapping fails.
    /// </exception>
    TDestination Map<TDestination>(object source);

    /// <summary>
    /// Maps a source object to <typeparamref name="TDestination"/> using the configured
    /// <typeparamref name="TSource"/> map.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <returns>The mapped destination object.</returns>
    /// <exception cref="InvalidOperationException">
    /// No map from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/> is
    /// configured.
    /// </exception>
    /// <exception cref="MappingException">
    /// A member value cannot be converted or a nested mapping fails.
    /// </exception>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// Maps a source object to a destination type determined at runtime.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="destinationType">The destination type.</param>
    /// <returns>The mapped destination object.</returns>
    /// <exception cref="InvalidOperationException">
    /// No match for the runtime source type and destination type is configured.
    /// </exception>
    /// <exception cref="MappingException">
    /// A member value cannot be converted or a nested mapping fails.
    /// </exception>
    object Map(object source, Type destinationType);
}
