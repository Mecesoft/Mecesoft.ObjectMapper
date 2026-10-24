namespace Mecesoft.ObjectMapper;

/// <summary>
/// Member-level options for a destination member.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
public interface IMemberOptions<TSource>
{
    /// <summary>
    /// Supplies a value for the destination member from a custom source expression.
    /// </summary>
    /// <param name="mapFrom">The value factory.</param>
    void MapFrom(Func<TSource, object?> mapFrom);

    /// <summary>
    /// Excludes the destination member from conventional mapping.
    /// </summary>
    void Ignore();
}

internal sealed class MemberOptions<TSource> : IMemberOptions<TSource>
{
    private bool _ignore;
    private Func<object, object?>? _mapFrom;

    public void MapFrom(Func<TSource, object?> mapFrom)
    {
        ArgumentNullException.ThrowIfNull(mapFrom);
        _mapFrom = source => mapFrom((TSource)source);
    }

    public void Ignore()
    {
        _ignore = true;
    }

    public MemberMap ToMemberMap()
    {
        return new MemberMap(_ignore, _mapFrom);
    }
}
