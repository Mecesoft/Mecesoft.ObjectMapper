namespace Mecesoft.ObjectMapper;

/// <summary>
/// Optional configuration behavior for a mapper.
/// </summary>
public sealed class MapperConfigurationOptions
{
    internal const int DefaultMaxDepth = 128;
    internal const int DefaultMaxCollectionSize = 100_000;

    /// <summary>
    /// Gets or sets the maximum number of nested object/collection mapping levels.
    /// The top-level mapping has depth 1. Default is 128.
    /// </summary>
    public int MaxDepth { get; set; } = DefaultMaxDepth;

    /// <summary>
    /// Gets or sets the maximum number of elements allowed in a single mapped collection.
    /// The limit is applied per collection, including nested collections. Default is 100,000.
    /// Set to <see cref="int.MaxValue"/> to effectively disable the limit.
    /// </summary>
    public int MaxCollectionSize { get; set; } = DefaultMaxCollectionSize;
}
