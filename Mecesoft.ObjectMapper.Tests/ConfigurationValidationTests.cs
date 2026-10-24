namespace Mecesoft.ObjectMapper.Tests;

public sealed class ConfigurationValidationTests
{
    [Fact]
    public void Valid_nested_and_collection_configuration_passes()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ValidationParentSource, ValidationParentDestination>();
            cfg.CreateMap<ValidationChildSource, ValidationChildDestination>();
            cfg.CreateMap<ValidationCollectionSource, ValidationCollectionDestination>();
        });

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Missing_nested_map_is_reported()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<ValidationParentSource, ValidationParentDestination>());

        var exception = Assert.Throws<MappingConfigurationException>(
            configuration.AssertConfigurationIsValid);

        Assert.Contains("Child", exception.Message);
        Assert.Contains("no conversion or nested map", exception.Message);
        Assert.Contains(nameof(ValidationChildSource), exception.Message);
        Assert.Contains(nameof(ValidationChildDestination), exception.Message);
    }

    [Fact]
    public void Unmapped_destination_member_is_reported()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<ValidationUnmappedSource, ValidationUnmappedDestination>());

        var exception = Assert.Throws<MappingConfigurationException>(
            configuration.AssertConfigurationIsValid);

        Assert.Contains("Optional", exception.Message);
        Assert.Contains("no matching source member", exception.Message);
    }

    [Fact]
    public void Ignored_destination_member_is_valid()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<ValidationUnmappedSource, ValidationUnmappedDestination>()
                .Ignore(dest => dest.Optional));

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Missing_collection_element_map_is_reported()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<ValidationCollectionSource, ValidationCollectionDestination>());

        var exception = Assert.Throws<MappingConfigurationException>(
            configuration.AssertConfigurationIsValid);

        Assert.Contains("Children", exception.Message);
        Assert.Contains("[]", exception.Message);
        Assert.Contains(nameof(ValidationChildSource), exception.Message);
        Assert.Contains(nameof(ValidationChildDestination), exception.Message);
    }

    [Fact]
    public void Invalid_conversion_is_reported()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<ValidationBadSource, ValidationBadDestination>());

        var exception = Assert.Throws<MappingConfigurationException>(
            configuration.AssertConfigurationIsValid);

        Assert.Contains("Value", exception.Message);
        Assert.Contains(nameof(Guid), exception.Message);
        Assert.Contains(nameof(Int32), exception.Message);
    }

    [Fact]
    public void Base_source_map_satisfies_derived_source_property()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ValidationDerivedSource, ValidationDerivedDestination>();
            cfg.CreateMap<AnimalSource, AnimalDto>();
        });

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Ambiguous_map_resolution_is_reported()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ValidationAmbiguousSource, ValidationAmbiguousDestination>();
            cfg.CreateMap<ILeftSource, TaggedDto>()
                .ForMember(dest => dest.Source, options => options.MapFrom(_ => "left"));
            cfg.CreateMap<IRightSource, TaggedDto>()
                .ForMember(dest => dest.Source, options => options.MapFrom(_ => "right"));
        });

        var exception = Assert.Throws<MappingConfigurationException>(
            configuration.AssertConfigurationIsValid);

        Assert.Contains("Ambiguous mapping", exception.Message);
        Assert.Contains(nameof(ILeftSource), exception.Message);
        Assert.Contains(nameof(IRightSource), exception.Message);
    }
}
