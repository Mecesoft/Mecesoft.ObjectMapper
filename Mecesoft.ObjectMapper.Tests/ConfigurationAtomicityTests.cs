namespace Mecesoft.ObjectMapper.Tests;

public sealed class ConfigurationAtomicityTests
{
    [Fact]
    public void Failed_ReverseMap_rolls_back_the_forward_map_and_keeps_configuration_usable()
    {
        var configuration = new MapperConfiguration(_ => { });
        configuration.CreateMap<ReverseFlatDestination, ReverseFlatSource>();
        configuration.CreateMap<ProfileSource, ProfileDestination>();

        var expression = configuration.CreateMap<ReverseFlatSource, ReverseFlatDestination>();

        Assert.Throws<MappingConfigurationException>(() => expression.ReverseMap());
        Assert.Throws<MappingConfigurationException>(() => expression.ForMember(
            dest => dest.FullName,
            options => options.MapFrom(_ => "late")));

        var mapper = configuration.CreateMapper();

        var reverse = mapper.Map<ReverseFlatSource>(
            new ReverseFlatDestination { Id = 3, FirstName = "Grace" });
        Assert.Equal(3, reverse.Id);
        Assert.Equal("Grace", reverse.FirstName);

        Assert.Throws<InvalidOperationException>(
            () => mapper.Map<ReverseFlatDestination>(new ReverseFlatSource()));

        Assert.Equal(
            5,
            mapper.Map<ProfileDestination>(new ProfileSource { Value = 5 }).Value);
    }

    [Fact]
    public void Failed_ReverseMap_rolls_back_only_the_pair_and_preserves_snapshot_isolation()
    {
        var configuration = new MapperConfiguration(_ => { });
        var expression = configuration.CreateMap<ReverseFlatSource, ReverseFlatDestination>();
        var existingMapper = configuration.CreateMapper();

        configuration.CreateMap<ReverseFlatDestination, ReverseFlatSource>();

        Assert.Throws<MappingConfigurationException>(() => expression.ReverseMap());

        var existingDestination = existingMapper.Map<ReverseFlatDestination>(
            new ReverseFlatSource { Id = 1, FirstName = "Ada" });
        Assert.Equal("Ada", existingDestination.FirstName);

        var newMapper = configuration.CreateMapper();

        var reverse = newMapper.Map<ReverseFlatSource>(
            new ReverseFlatDestination { Id = 2, FirstName = "Grace" });
        Assert.Equal("Grace", reverse.FirstName);

        Assert.Throws<InvalidOperationException>(
            () => newMapper.Map<ReverseFlatDestination>(new ReverseFlatSource()));
    }

    [Fact]
    public void Duplicate_ReverseMap_call_does_not_remove_successfully_registered_maps()
    {
        var configuration = new MapperConfiguration(_ => { });
        var expression = configuration.CreateMap<ReverseFlatSource, ReverseFlatDestination>();

        expression.ReverseMap();
        Assert.Throws<MappingConfigurationException>(() => expression.ReverseMap());

        var mapper = configuration.CreateMapper();

        Assert.Equal(
            "Ada",
            mapper.Map<ReverseFlatDestination>(
                new ReverseFlatSource { FirstName = "Ada" }).FirstName);
        Assert.Equal(
            "Grace",
            mapper.Map<ReverseFlatSource>(
                new ReverseFlatDestination { FirstName = "Grace" }).FirstName);
    }

    [Fact]
    public void Failed_AddProfile_does_not_add_any_profile_maps()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<ProfileSource, ProfileDestination>());

        Assert.Throws<MappingConfigurationException>(
            () => configuration.AddProfile(new AtomicityProfile()));

        var mapper = configuration.CreateMapper();

        Assert.Throws<InvalidOperationException>(
            () => mapper.Map<AtomicityDestination>(new AtomicitySource()));

        Assert.Equal(
            7,
            mapper.Map<ProfileDestination>(new ProfileSource { Value = 7 }).Value);
    }

    [Fact]
    public void Failed_AddProfile_can_be_followed_by_a_valid_registration()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<ProfileSource, ProfileDestination>());

        Assert.Throws<MappingConfigurationException>(
            () => configuration.AddProfile(new AtomicityProfile()));
        configuration.CreateMap<Address, AddressDto>();

        var mapper = configuration.CreateMapper();
        Assert.Equal(
            9,
            mapper.Map<ProfileDestination>(new ProfileSource { Value = 9 }).Value);
        Assert.Equal(
            "Istanbul",
            mapper.Map<AddressDto>(
                new Address { City = "Istanbul" }).City);
    }

    [Fact]
    public void Duplicate_CreateMap_does_not_change_configuration()
    {
        var configuration = new MapperConfiguration(_ => { });
        configuration.CreateMap<BasicSource, BasicDestination>();

        Assert.Throws<MappingConfigurationException>(
            () => configuration.CreateMap<BasicSource, BasicDestination>());

        var mapper = configuration.CreateMapper();
        Assert.Equal(
            12,
            mapper.Map<BasicDestination>(new BasicSource { Id = 12 }).Id);
    }
}
