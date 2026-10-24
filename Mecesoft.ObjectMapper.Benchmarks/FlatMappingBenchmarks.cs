using BenchmarkDotNet.Attributes;

namespace Mecesoft.ObjectMapper.Benchmarks;

public class FlatMappingBenchmarks
{
    private IMapper _warmMapper = null!;
    private FlatSource _source = null!;

    [GlobalSetup]
    public void Setup()
    {
        _warmMapper = new MapperConfiguration(cfg => cfg.CreateMap<FlatSource, FlatDto>()).CreateMapper();
        _source = CreateSource();
    }

    [Benchmark(Baseline = true)]
    public FlatDto Manual_Flat()
    {
        return new FlatDto
        {
            Id = _source.Id,
            FirstName = _source.FirstName,
            LastName = _source.LastName,
            Email = _source.Email,
            Phone = _source.Phone,
            City = _source.City,
            Country = _source.Country,
            Balance = _source.Balance,
            CreatedAt = _source.CreatedAt,
            IsActive = _source.IsActive,
            ExternalId = _source.ExternalId
        };
    }

    [Benchmark]
    public FlatDto Warm_Flat()
    {
        return _warmMapper.Map<FlatDto>(_source);
    }

    [Benchmark]
    public FlatDto Cold_Flat()
    {
        var mapper = new MapperConfiguration(cfg => cfg.CreateMap<FlatSource, FlatDto>()).CreateMapper();
        return mapper.Map<FlatDto>(_source);
    }

    private static FlatSource CreateSource()
    {
        return new FlatSource
        {
            Id = 42,
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            Phone = "+90 555 000 00 00",
            City = "Istanbul",
            Country = "TR",
            Balance = 1250.75m,
            CreatedAt = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc),
            IsActive = true,
            ExternalId = Guid.Parse("2f4a6c70-8a76-4d0c-9db6-6edb3c8f0e7b")
        };
    }
}
