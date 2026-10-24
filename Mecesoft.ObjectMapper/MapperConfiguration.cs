namespace Mecesoft.ObjectMapper;

/// <summary>
/// Builds a mapping configuration and creates <see cref="IMapper"/> instances from it.
/// </summary>
/// <remarks>
/// Configuration is mutable until <see cref="CreateMapper"/> is called. Each mapper receives
/// an immutable snapshot; later configuration changes only affect mappers created afterwards.
/// Configuration setup is not thread-safe, while mapping through a created mapper is.
/// </remarks>
public sealed class MapperConfiguration : IMapperConfigurationExpression
{
    private readonly List<TypeMap> _typeMaps = new();
    private readonly int _maxDepth;
    private readonly int _maxCollectionSize;
    private TypeMapRegistrar? _registrar;

    private TypeMapRegistrar Registrar => _registrar ??= new TypeMapRegistrar(this);

    /// <summary>
    /// Initializes a configuration with default mapper options.
    /// </summary>
    /// <param name="config">The configuration callback.</param>
    /// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
    public MapperConfiguration(Action<IMapperConfigurationExpression> config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _maxDepth = MapperConfigurationOptions.DefaultMaxDepth;
        _maxCollectionSize = MapperConfigurationOptions.DefaultMaxCollectionSize;
        config(this);
    }

    /// <summary>
    /// Initializes a configuration with explicit mapper options.
    /// </summary>
    /// <param name="config">The configuration callback.</param>
    /// <param name="options">The mapper options. Values are captured by this configuration.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="config"/> or <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <see cref="MapperConfigurationOptions.MaxDepth"/> or
    /// <see cref="MapperConfigurationOptions.MaxCollectionSize"/> is not positive.
    /// </exception>
    public MapperConfiguration(
        Action<IMapperConfigurationExpression> config,
        MapperConfigurationOptions options)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(options);

        if (options.MaxDepth <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.MaxDepth,
                "MaxDepth must be greater than zero.");
        }

        if (options.MaxCollectionSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.MaxCollectionSize,
                "MaxCollectionSize must be greater than zero. Use int.MaxValue to disable the limit.");
        }

        _maxDepth = options.MaxDepth;
        _maxCollectionSize = options.MaxCollectionSize;
        config(this);
    }

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
    public MappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        var typeMap = new TypeMap(typeof(TSource), typeof(TDestination));
        RegisterTypeMap(typeMap);
        return new MappingExpression<TSource, TDestination>(
            typeMap,
            Registrar,
            canRollbackForwardMap: true);
    }

    /// <summary>
    /// Adds all maps declared by a profile as one atomic batch.
    /// </summary>
    /// <param name="profile">The profile to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="profile"/> is <see langword="null"/>.</exception>
    /// <exception cref="MappingConfigurationException">
    /// One or more profile maps conflict with existing maps. No profile maps are added.
    /// </exception>
    public void AddProfile(Profile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        AddTypeMaps(profile.TypeMaps);
    }

    /// <summary>
    /// Captures the current configuration and creates an immutable mapper.
    /// </summary>
    /// <returns>A mapper whose plans use the configuration snapshot taken by this call.</returns>
    public IMapper CreateMapper()
    {
        var typeMaps = new Dictionary<(Type SourceType, Type DestinationType), TypeMapSnapshot>(_typeMaps.Count);

        foreach (var typeMap in _typeMaps)
        {
            typeMaps.Add(
                (typeMap.SourceType, typeMap.DestinationType),
                typeMap.CreateSnapshot());
        }

        return new Mapper(new MappingPlanCache(typeMaps), _maxDepth, _maxCollectionSize);
    }

    /// <summary>
    /// Validates the configured maps for unmapped destination members, missing nested maps
    /// and unsupported collection mappings. Reverse maps are included and marked in
    /// diagnostics. Validation is opt-in and does not run during Map.
    /// </summary>
    /// <exception cref="MappingConfigurationException">One or more configuration errors were found.</exception>
    public void AssertConfigurationIsValid()
    {
        MappingConfigurationValidator.AssertConfigurationIsValid(_typeMaps);
    }

    private void RegisterTypeMap(TypeMap typeMap)
    {
        EnsureMapIsNotRegistered(typeMap);
        _typeMaps.Add(typeMap);
    }

    private void UnregisterTypeMap(TypeMap typeMap)
    {
        _typeMaps.Remove(typeMap);
    }

    private void AddTypeMaps(IReadOnlyList<TypeMap> typeMaps)
    {
        if (typeMaps.Count == 0)
        {
            return;
        }

        var registeredPairs = new HashSet<(Type SourceType, Type DestinationType)>(_typeMaps.Count + typeMaps.Count);

        foreach (var registeredMap in _typeMaps)
        {
            registeredPairs.Add((registeredMap.SourceType, registeredMap.DestinationType));
        }

        foreach (var typeMap in typeMaps)
        {
            if (!registeredPairs.Add((typeMap.SourceType, typeMap.DestinationType)))
            {
                throw CreateDuplicateMapException(typeMap);
            }
        }

        _typeMaps.AddRange(typeMaps);
    }

    private static MappingConfigurationException CreateDuplicateMapException(TypeMap typeMap)
    {
        return new MappingConfigurationException(
            $"A map from '{typeMap.SourceType.FullName}' to '{typeMap.DestinationType.FullName}' " +
            "is already configured. Duplicate CreateMap/ReverseMap definitions are not allowed.");
    }

    private void EnsureMapIsNotRegistered(TypeMap typeMap)
    {
        if (_typeMaps.Any(map =>
                map.SourceType == typeMap.SourceType &&
                map.DestinationType == typeMap.DestinationType))
        {
            throw CreateDuplicateMapException(typeMap);
        }
    }

    private sealed class TypeMapRegistrar : ITypeMapRegistrar
    {
        private readonly MapperConfiguration _owner;

        public TypeMapRegistrar(MapperConfiguration owner)
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
