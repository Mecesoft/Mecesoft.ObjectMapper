using BenchmarkDotNet.Attributes;

namespace Mecesoft.ObjectMapper.Benchmarks;

public class CustomMappingBenchmarks
{
    private IMapper _warmMapper = null!;
    private CustomSource _source = null!;

    [GlobalSetup]
    public void Setup()
    {
        _warmMapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<CustomSource, CustomDto>()
                .ForMember(
                    dest => dest.FullName,
                    options => options.MapFrom(src => src.FirstName + " " + src.LastName))
                .ForMember(
                    dest => dest.Initials,
                    options => options.MapFrom(src => string.Concat(src.FirstName[0], src.LastName[0])))
                .ForMember(
                    dest => dest.Age,
                    options => options.MapFrom(src => DateTime.UtcNow.Year - src.BirthDate.Year)))
            .CreateMapper();

        _source = new CustomSource
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            BirthDate = new DateTime(1815, 12, 10, 0, 0, 0, DateTimeKind.Utc)
        };
    }

    [Benchmark(Baseline = true)]
    public CustomDto Manual_Custom()
    {
        return new CustomDto
        {
            FullName = _source.FirstName + " " + _source.LastName,
            Initials = string.Concat(_source.FirstName[0], _source.LastName[0]),
            Age = DateTime.UtcNow.Year - _source.BirthDate.Year
        };
    }

    [Benchmark]
    public CustomDto Warm_Custom()
    {
        return _warmMapper.Map<CustomDto>(_source);
    }

    [Benchmark]
    public CustomDto Cold_Custom()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<CustomSource, CustomDto>()
                .ForMember(
                    dest => dest.FullName,
                    options => options.MapFrom(src => src.FirstName + " " + src.LastName))
                .ForMember(
                    dest => dest.Initials,
                    options => options.MapFrom(src => string.Concat(src.FirstName[0], src.LastName[0])))
                .ForMember(
                    dest => dest.Age,
                    options => options.MapFrom(src => DateTime.UtcNow.Year - src.BirthDate.Year)))
            .CreateMapper();

        return mapper.Map<CustomDto>(_source);
    }
}
