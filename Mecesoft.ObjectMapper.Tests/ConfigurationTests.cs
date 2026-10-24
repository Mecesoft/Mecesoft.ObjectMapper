namespace Mecesoft.ObjectMapper.Tests;

public sealed class ConfigurationTests
{
    [Fact]
    public void CreateMapper_returns_working_mapper()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<ProfileSource, ProfileDestination>()).CreateMapper();

        var destination = mapper.Map<ProfileDestination>(new ProfileSource { Value = 5 });

        Assert.Equal(5, destination.Value);
    }

    [Fact]
    public void AddProfile_registers_profile_maps()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfile(new TestProfile())).CreateMapper();

        var destination = mapper.Map<ProfileDestination>(new ProfileSource { Value = 8 });

        Assert.Equal(8, destination.Value);
    }

    [Fact]
    public void CreateMap_returns_a_mapping_expression()
    {
        var configuration = new MapperConfiguration(_ => { });

        var expression = configuration.CreateMap<BasicSource, BasicDestination>();

        Assert.NotNull(expression);
        Assert.Same(expression, expression.Ignore(dest => dest.Secret));
    }

    [Fact]
    public void Duplicate_CreateMap_in_configuration_throws()
    {
        var exception = Assert.Throws<MappingConfigurationException>(() =>
            new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<BasicSource, BasicDestination>();
                cfg.CreateMap<BasicSource, BasicDestination>();
            }));

        Assert.Contains(nameof(BasicSource), exception.Message);
        Assert.Contains(nameof(BasicDestination), exception.Message);
        Assert.Contains("already configured", exception.Message);
    }

    [Fact]
    public void Duplicate_CreateMap_across_profiles_throws()
    {
        var exception = Assert.Throws<MappingConfigurationException>(() =>
            new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new TestProfile());
                cfg.AddProfile(new TestProfile());
            }));

        Assert.Contains(nameof(ProfileSource), exception.Message);
        Assert.Contains(nameof(ProfileDestination), exception.Message);
    }

    [Fact]
    public void Duplicate_CreateMap_within_profile_throws()
    {
        var exception = Assert.Throws<MappingConfigurationException>(
            () => new DuplicateMapProfile());

        Assert.Contains(nameof(ProfileSource), exception.Message);
        Assert.Contains(nameof(ProfileDestination), exception.Message);
    }
}
