using BenchmarkDotNet.Attributes;

namespace Mecesoft.ObjectMapper.Benchmarks;

public class NestedMappingBenchmarks
{
    private IMapper _warmMapper = null!;
    private NestedSource _source = null!;

    [GlobalSetup]
    public void Setup()
    {
        _warmMapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<FlatSource, FlatDto>();
            cfg.CreateMap<AddressSource, AddressDto>();
            cfg.CreateMap<NestedSource, NestedDto>();
        }).CreateMapper();

        _source = new NestedSource
        {
            Customer = new FlatSource
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
            },
            Address = new AddressSource
            {
                City = "Istanbul",
                Street = "Istiklal",
                Zip = "34000",
                Country = "TR"
            }
        };
    }

    [Benchmark(Baseline = true)]
    public NestedDto Manual_Nested()
    {
        return new NestedDto
        {
            Customer = MapFlat(_source.Customer),
            Address = new AddressDto
            {
                City = _source.Address.City,
                Street = _source.Address.Street,
                Zip = _source.Address.Zip,
                Country = _source.Address.Country
            }
        };
    }

    [Benchmark]
    public NestedDto Warm_Nested()
    {
        return _warmMapper.Map<NestedDto>(_source);
    }

    [Benchmark]
    public NestedDto Cold_Nested()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<FlatSource, FlatDto>();
            cfg.CreateMap<AddressSource, AddressDto>();
            cfg.CreateMap<NestedSource, NestedDto>();
        }).CreateMapper();

        return mapper.Map<NestedDto>(_source);
    }

    private static FlatDto MapFlat(FlatSource source)
    {
        return new FlatDto
        {
            Id = source.Id,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email,
            Phone = source.Phone,
            City = source.City,
            Country = source.Country,
            Balance = source.Balance,
            CreatedAt = source.CreatedAt,
            IsActive = source.IsActive,
            ExternalId = source.ExternalId
        };
    }
}
