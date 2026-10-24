namespace Mecesoft.ObjectMapper;

internal sealed class TypeMap
{
    private readonly Dictionary<string, MemberMap> _memberMaps = new(StringComparer.Ordinal);

    public TypeMap(Type sourceType, Type destinationType, bool isReverseMap = false)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        IsReverseMap = isReverseMap;
    }

    public Type SourceType { get; }
    public Type DestinationType { get; }
    public bool IsReverseMap { get; }

    public void SetMemberMap(string memberName, MemberMap memberMap)
    {
        _memberMaps[memberName] = memberMap;
    }

    public bool TryGetMemberMap(string memberName, out MemberMap memberMap)
    {
        return _memberMaps.TryGetValue(memberName, out memberMap!);
    }

    internal IReadOnlyDictionary<string, MemberMap> MemberMaps => _memberMaps;

    public TypeMapSnapshot CreateSnapshot()
    {
        return new TypeMapSnapshot(SourceType, DestinationType, _memberMaps);
    }
}

internal sealed class MemberMap
{
    public MemberMap(bool ignore, Func<object, object?>? mapFrom)
    {
        Ignore = ignore;
        MapFrom = mapFrom;
    }

    public bool Ignore { get; }
    public Func<object, object?>? MapFrom { get; }
}
