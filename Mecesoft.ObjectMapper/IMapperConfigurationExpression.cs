namespace Mecesoft.ObjectMapper;

/// <summary>
/// Configuration operations available while building a <see cref="MapperConfiguration"/>.
/// </summary>
public interface IMapperConfigurationExpression
{
    /// <summary>
    /// Creates a conventional map from <typeparamref name="TSource"/> to
    /// <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <returns>The expression used to configure member rules or a reverse map.</returns>
    /// <exception cref="MappingConfigurationException">
    /// The same source and destination pair is already configured.
    /// </exception>
    MappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>();

    /// <summary>
    /// Adds all maps declared by a profile as one atomic batch.
    /// </summary>
    /// <param name="profile">The profile to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="profile"/> is <see langword="null"/>.</exception>
    /// <exception cref="MappingConfigurationException">
    /// One or more profile maps conflict with existing maps. No profile maps are added.
    /// </exception>
    void AddProfile(Profile profile);
}
