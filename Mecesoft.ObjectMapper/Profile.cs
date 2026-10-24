namespace Mecesoft.ObjectMapper;

/// <summary>
/// Base class for grouping related map definitions.
/// </summary>
/// <remarks>
/// Profile instances are configured when they are constructed. Adding a profile to a
/// configuration is atomic: if any profile map conflicts, none of its maps are added.
/// </remarks>
public abstract class Profile
{
    private readonly List<TypeMap> _typeMaps = new();
    private TypeMapRegistrar? _registrar;

    private TypeMapRegistrar Registrar => _registrar ??= new TypeMapRegistrar(this);

    /// <summary>
    /// Creates a conventional map from <typeparamref name="TSource"/> to
    /// <typeparamref name="TDestination"/> in this profile.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <returns>The expression used to configure member rules or a reverse map.</returns>
    /// <exception cref="MappingConfigurationException">
    /// The same source and destination pair is already configured in this profile.
    /// </exception>
    protected MappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        var typeMap = new TypeMap(typeof(TSource), typeof(TDestination));
        RegisterTypeMap(typeMap);
        return new MappingExpression<TSource, TDestination>(
            typeMap,
            Registrar,
            canRollbackForwardMap: true);
    }

    internal IReadOnlyList<TypeMap> TypeMaps => _typeMaps;

    private void RegisterTypeMap(TypeMap typeMap)
    {
        if (_typeMaps.Any(map =>
                map.SourceType == typeMap.SourceType &&
                map.DestinationType == typeMap.DestinationType))
        {
            throw new MappingConfigurationException(
                $"A map from '{typeMap.SourceType.FullName}' to '{typeMap.DestinationType.FullName}' " +
                "is already configured in this profile. Duplicate CreateMap/ReverseMap definitions are not allowed.");
        }

        _typeMaps.Add(typeMap);
    }

    private void UnregisterTypeMap(TypeMap typeMap)
    {
        _typeMaps.Remove(typeMap);
    }

    private sealed class TypeMapRegistrar : ITypeMapRegistrar
    {
        private readonly Profile _owner;

        public TypeMapRegistrar(Profile owner)
        {
            _owner = owner;
        }

        public void RegisterTypeMap(TypeMap typeMap)
        {
            _owner.RegisterTypeMap(typeMap);
        }

        public void UnregisterTypeMap(TypeMap typeMap)
        {
            _owner.UnregisterTypeMap(typeMap);
        }
    }
}
