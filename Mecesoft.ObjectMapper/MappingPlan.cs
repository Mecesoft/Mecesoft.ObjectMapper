using System.Collections.Concurrent;
using System.Reflection;

namespace Mecesoft.ObjectMapper;

internal sealed class MappingPlanCache
{
    private readonly Dictionary<(Type SourceType, Type DestinationType), TypeMapSnapshot> _typeMaps;
    private readonly ConcurrentDictionary<(Type SourceType, Type DestinationType), MappingPlan> _plans = new();
    private ConcurrentDictionary<(Type SourceType, Type DestinationType), ResolvedTypeMap>? _resolvedTypeMaps;

    public MappingPlanCache(
        Dictionary<(Type SourceType, Type DestinationType), TypeMapSnapshot> typeMaps)
    {
        _typeMaps = typeMaps;
    }

    public bool TryGetTypeMap(
        Type sourceType,
        Type destinationType,
        out TypeMapSnapshot typeMap)
    {
        if (_typeMaps.TryGetValue((sourceType, destinationType), out typeMap!))
        {
            return true;
        }

        var resolvedTypeMaps = LazyInitializer.EnsureInitialized(
            ref _resolvedTypeMaps,
            static () => new ConcurrentDictionary<(Type SourceType, Type DestinationType), ResolvedTypeMap>());

        var resolved = resolvedTypeMaps.GetOrAdd(
            (sourceType, destinationType),
            static (key, cache) => new ResolvedTypeMap(
                TypeMapResolver.ResolveSourceType(
                    cache._typeMaps.Keys,
                    key.SourceType,
                    key.DestinationType)),
            this);

        if (resolved.SourceType is null)
        {
            typeMap = null!;
            return false;
        }

        typeMap = _typeMaps[(resolved.SourceType, destinationType)];
        return true;
    }

    public MappingPlan GetPlan(TypeMapSnapshot typeMap)
    {
        var key = (typeMap.SourceType, typeMap.DestinationType);

        if (!typeMap.HasMemberMaps)
        {
            return ConventionalPlanCache.GetOrAdd(
                key,
                static planKey => MappingPlan.CreateConventional(
                    planKey.SourceType,
                    planKey.DestinationType));
        }

        return _plans.GetOrAdd(
            key,
            static (_, snapshot) => MappingPlan.Create(snapshot),
            typeMap);
    }

    private static readonly ConcurrentDictionary<(Type SourceType, Type DestinationType), MappingPlan>
        ConventionalPlanCache = new();

    private sealed class ResolvedTypeMap
    {
        public ResolvedTypeMap(Type? sourceType)
        {
            SourceType = sourceType;
        }

        public Type? SourceType { get; }
    }
}

internal sealed class TypeMapSnapshot
{
    private readonly Dictionary<string, MemberMap> _memberMaps;

    public TypeMapSnapshot(
        Type sourceType,
        Type destinationType,
        Dictionary<string, MemberMap> memberMaps)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        _memberMaps = new Dictionary<string, MemberMap>(memberMaps, StringComparer.Ordinal);
    }

    public Type SourceType { get; }

    public Type DestinationType { get; }

    public bool HasMemberMaps => _memberMaps.Count > 0;

    public bool TryGetMemberMap(string memberName, out MemberMap memberMap)
    {
        return _memberMaps.TryGetValue(memberName, out memberMap!);
    }
}

internal sealed class MappingPlan
{
    private MappingPlan(
        Type sourceType,
        Type destinationType,
        Func<object> createDestination,
        MemberPlan[] members)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        CreateDestination = createDestination;
        Members = members;
    }

    public Type SourceType { get; }

    public Type DestinationType { get; }

    public Func<object> CreateDestination { get; }

    public MemberPlan[] Members { get; }

    public static MappingPlan Create(TypeMapSnapshot typeMap)
    {
        return Create(typeMap.SourceType, typeMap.DestinationType, typeMap);
    }

    public static MappingPlan CreateConventional(Type sourceType, Type destinationType)
    {
        return Create(
            sourceType,
            destinationType,
            new TypeMapSnapshot(
                sourceType,
                destinationType,
                EmptyMemberMaps));
    }

    private static MappingPlan Create(
        Type sourceType,
        Type destinationType,
        TypeMapSnapshot typeMap)
    {
        var destinationProperties = destinationType.GetProperties(
            BindingFlags.Public | BindingFlags.Instance);
        var members = new List<MemberPlan>(destinationProperties.Length);

        foreach (var destinationProperty in destinationProperties)
        {
            if (!destinationProperty.CanWrite ||
                destinationProperty.GetIndexParameters().Length != 0)
            {
                continue;
            }

            typeMap.TryGetMemberMap(destinationProperty.Name, out var memberMap);

            if (memberMap is not null && memberMap.Ignore)
            {
                continue;
            }

            var mapFrom = memberMap?.MapFrom;
            Func<object, object?>? sourceGetter = null;

            if (mapFrom is null)
            {
                var sourceProperty = GetReadableProperty(sourceType, destinationProperty.Name);
                if (sourceProperty is null)
                {
                    continue;
                }

                sourceGetter = PropertyAccessor.CreateGetter(sourceProperty);
            }

            members.Add(new MemberPlan(
                destinationProperty.Name,
                destinationProperty.PropertyType,
                sourceGetter,
                PropertyAccessor.CreateSetter(destinationProperty),
                mapFrom));
        }

        return new MappingPlan(
            sourceType,
            destinationType,
            PropertyAccessor.CreateFactory(destinationType),
            members.ToArray());
    }

    private static readonly Dictionary<string, MemberMap> EmptyMemberMaps = new(StringComparer.Ordinal);

    private static PropertyInfo? GetReadableProperty(Type sourceType, string memberName)
    {
        var properties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.Name == memberName &&
                property.CanRead &&
                property.GetIndexParameters().Length == 0)
            {
                return property;
            }
        }

        return null;
    }
}

internal sealed class MemberPlan
{
    public MemberPlan(
        string name,
        Type propertyType,
        Func<object, object?>? sourceGetter,
        Action<object, object?> setter,
        Func<object, object?>? mapFrom)
    {
        Name = name;
        PropertyType = propertyType;
        SourceGetter = sourceGetter;
        Setter = setter;
        MapFrom = mapFrom;
    }

    public string Name { get; }

    public Type PropertyType { get; }

    public Func<object, object?>? SourceGetter { get; }

    public Action<object, object?> Setter { get; }

    public Func<object, object?>? MapFrom { get; }
}
