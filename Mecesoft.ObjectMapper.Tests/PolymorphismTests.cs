using System.Collections.Concurrent;

namespace Mecesoft.ObjectMapper.Tests;

public sealed class PolymorphismTests
{
    [Fact]
    public void Base_source_map_maps_derived_runtime_source()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<AnimalSource, AnimalDto>()).CreateMapper();

        var destination = mapper.Map<AnimalDto>(new DogSource
        {
            Name = "Rex",
            BarkVolume = 3
        });

        Assert.IsType<AnimalDto>(destination);
        Assert.Equal("Rex", destination.Name);
    }

    [Fact]
    public void Derived_collection_elements_use_base_element_map()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ZooSource, ZooDto>();
            cfg.CreateMap<AnimalSource, AnimalDto>();
        }).CreateMapper();

        var source = new ZooSource
        {
            Animals =
            {
                new DogSource { Name = "Rex", BarkVolume = 3 },
                new CatSource { Name = "Tom", Lives = 9 }
            }
        };

        var destination = mapper.Map<ZooDto>(source);

        Assert.Equal(new[] { "Rex", "Tom" }, destination.Animals.Select(animal => animal.Name));
        Assert.All(destination.Animals, animal => Assert.IsType<AnimalDto>(animal));
    }

    [Fact]
    public void Exact_derived_map_wins_over_base_map()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AnimalSource, AnimalDto>();
            cfg.CreateMap<DogSource, DogDto>();
        }).CreateMapper();

        var destination = mapper.Map<DogDto>(new DogSource
        {
            Name = "Rex",
            BarkVolume = 4
        });

        Assert.IsType<DogDto>(destination);
        Assert.Equal("Rex", destination.Name);
        Assert.Equal(4, destination.BarkVolume);
    }

    [Fact]
    public void Most_derived_base_map_is_selected()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<LevelASource, LevelDto>()
                .ForMember(dest => dest.Source, options => options.MapFrom(_ => "A"));
            cfg.CreateMap<LevelBSource, LevelDto>()
                .ForMember(dest => dest.Source, options => options.MapFrom(_ => "B"));
        }).CreateMapper();

        var destination = mapper.Map<LevelDto>(new LevelCSource { Name = "C" });

        Assert.Equal("C", destination.Name);
        Assert.Equal("b", destination.BValue);
        Assert.Equal("unset", destination.CValue);
        Assert.Equal("B", destination.Source);
    }

    [Fact]
    public void Base_map_does_not_map_derived_only_members()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<LevelASource, LevelDto>()).CreateMapper();

        var destination = mapper.Map<LevelDto>(new LevelCSource { Name = "C" });

        Assert.Equal("C", destination.Name);
        Assert.Equal("unset", destination.BValue);
        Assert.Equal("unset", destination.CValue);
    }

    [Fact]
    public void Class_map_is_preferred_over_interface_map_at_same_distance()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BaseTaggableSource, TaggedDto>()
                .ForMember(dest => dest.Source, options => options.MapFrom(_ => "base"));
            cfg.CreateMap<ITaggedSource, TaggedDto>()
                .ForMember(dest => dest.Source, options => options.MapFrom(_ => "interface"));
        }).CreateMapper();

        var destination = mapper.Map<TaggedDto>(new TaggedDerivedSource { Name = "tagged" });

        Assert.Equal("tagged", destination.Name);
        Assert.Equal("base", destination.Source);
    }

    [Fact]
    public void Interface_map_can_map_a_concrete_implementation()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<ITaggedSource, TaggedDto>()
                .ForMember(dest => dest.Source, options => options.MapFrom(_ => "interface")))
            .CreateMapper();

        var destination = mapper.Map<TaggedDto>(new TaggedDerivedSource { Name = "tagged" });

        Assert.Equal("tagged", destination.Name);
        Assert.Equal("interface", destination.Source);
    }

    [Fact]
    public void Ambiguous_interface_maps_throw()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ILeftSource, LevelDto>()
                .ForMember(dest => dest.Source, options => options.MapFrom(_ => "left"));
            cfg.CreateMap<IRightSource, LevelDto>()
                .ForMember(dest => dest.Source, options => options.MapFrom(_ => "right"));
        }).CreateMapper();

        var exception = Assert.Throws<MappingConfigurationException>(
            () => mapper.Map<LevelDto>(new MultiSource()));

        Assert.Contains("Ambiguous mapping", exception.Message);
        Assert.Contains(nameof(ILeftSource), exception.Message);
        Assert.Contains(nameof(IRightSource), exception.Message);
    }

    [Fact]
    public void Concurrent_derived_mapping_is_isolated()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<AnimalSource, AnimalDto>()).CreateMapper();

        var names = new ConcurrentBag<string>();

        Parallel.For(0, 32, index =>
        {
            names.Add(mapper.Map<AnimalDto>(new DogSource
            {
                Name = $"dog-{index}",
                BarkVolume = index
            }).Name);
        });

        Assert.Equal(32, names.Count);
        Assert.Equal(32, names.Distinct().Count());
    }
}
