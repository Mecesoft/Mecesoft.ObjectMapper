using BenchmarkDotNet.Attributes;

namespace Mecesoft.ObjectMapper.Benchmarks;

public class CollectionMappingBenchmarks
{
    private IMapper _warmMapper = null!;
    private CollectionSource _source = null!;

    [Params(10, 100, 1000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _warmMapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<LineSource, LineDto>();
            cfg.CreateMap<CollectionSource, CollectionDto>();
        }).CreateMapper();

        var items = new List<LineSource>(Count);
        for (var index = 0; index < Count; index++)
        {
            items.Add(new LineSource
            {
                Id = index,
                Sku = $"SKU-{index}",
                Price = 10.5m + index,
                Quantity = index % 7
            });
        }

        _source = new CollectionSource { Items = items };
    }

    [Benchmark(Baseline = true)]
    public CollectionDto Manual_Collection()
    {
        var items = new List<LineDto>(Count);
        for (var index = 0; index < Count; index++)
        {
            var source = _source.Items[index];
            items.Add(new LineDto
            {
                Id = source.Id,
                Sku = source.Sku,
                Price = source.Price,
                Quantity = source.Quantity
            });
        }

        return new CollectionDto { Items = items };
    }

    [Benchmark]
    public CollectionDto Warm_Collection()
    {
        return _warmMapper.Map<CollectionDto>(_source);
    }

    [Benchmark]
    public CollectionDto Cold_Collection()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<LineSource, LineDto>();
            cfg.CreateMap<CollectionSource, CollectionDto>();
        }).CreateMapper();

        return mapper.Map<CollectionDto>(_source);
    }
}
