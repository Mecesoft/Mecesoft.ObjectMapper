namespace Mecesoft.ObjectMapper.Tests;

public sealed class ReverseMapTests
{
    [Fact]
    public void ReverseMap_maps_flat_dto_in_both_directions()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<ReverseFlatSource, ReverseFlatDestination>().ReverseMap())
            .CreateMapper();

        var destination = mapper.Map<ReverseFlatDestination>(new ReverseFlatSource
        {
            Id = 1,
            FirstName = "Ada",
            LastName = "Lovelace",
            Secret = "source-secret"
        });

        Assert.Equal(1, destination.Id);
        Assert.Equal("Ada", destination.FirstName);
        Assert.Equal("Lovelace", destination.LastName);
        Assert.Equal("source-secret", destination.Secret);

        var source = mapper.Map<ReverseFlatSource>(new ReverseFlatDestination
        {
            Id = 2,
            FirstName = "Grace",
            LastName = "Hopper",
            Secret = "reverse-secret"
        });

        Assert.Equal(2, source.Id);
        Assert.Equal("Grace", source.FirstName);
        Assert.Equal("Hopper", source.LastName);
        Assert.Equal("reverse-secret", source.Secret);
    }

    [Fact]
    public void Forward_ignore_and_map_from_are_not_reversed_automatically()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<ReverseFlatSource, ReverseFlatDestination>()
                .ForMember(
                    dest => dest.FullName,
                    options => options.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .Ignore(dest => dest.Secret)
                .ReverseMap())
            .CreateMapper();

        var destination = mapper.Map<ReverseFlatDestination>(new ReverseFlatSource
        {
            Id = 1,
            FirstName = "Ada",
            LastName = "Lovelace",
            Secret = "source-secret"
        });

        Assert.Equal("Ada Lovelace", destination.FullName);
        Assert.Equal("destination-secret", destination.Secret);

        var reverse = mapper.Map<ReverseFlatSource>(new ReverseFlatDestination
        {
            Id = 2,
            FirstName = "Grace",
            LastName = "Hopper",
            FullName = "ignored",
            Secret = "reverse-secret"
        });

        Assert.Equal("Grace", reverse.FirstName);
        Assert.Equal("Hopper", reverse.LastName);
        Assert.Equal("reverse-secret", reverse.Secret);
    }

    [Fact]
    public void Returned_reverse_expression_can_be_configured_explicitly()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            var reverse = cfg.CreateMap<ReverseFlatSource, ReverseFlatDestination>()
                .ReverseMap();

            reverse.Ignore(dest => dest.Secret);
        }).CreateMapper();

        var source = mapper.Map<ReverseFlatSource>(new ReverseFlatDestination
        {
            Id = 3,
            Secret = "reverse-secret"
        });

        Assert.Equal(3, source.Id);
        Assert.Equal("source-secret", source.Secret);
    }

    [Fact]
    public void ReverseMap_supports_nested_objects_and_null_handling()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ReverseAddressSource, ReverseAddressDestination>().ReverseMap();
            cfg.CreateMap<ReverseCustomerSource, ReverseCustomerDestination>().ReverseMap();
        }).CreateMapper();

        var destination = mapper.Map<ReverseCustomerDestination>(new ReverseCustomerSource
        {
            Name = "Ada",
            Address = new ReverseAddressSource { City = "Istanbul", Street = "Istiklal" }
        });

        Assert.NotNull(destination.Address);
        Assert.Equal("Istanbul", destination.Address.City);

        var source = mapper.Map<ReverseCustomerSource>(destination);
        Assert.NotNull(source.Address);
        Assert.Equal("Istanbul", source.Address.City);

        var nullDestination = mapper.Map<ReverseCustomerDestination>(
            new ReverseCustomerSource { Name = "Null", Address = null });
        Assert.Null(nullDestination.Address);

        var nullSource = mapper.Map<ReverseCustomerSource>(
            new ReverseCustomerDestination { Name = "Null", Address = null });
        Assert.Null(nullSource.Address);
    }

    [Fact]
    public void ReverseMap_supports_collections_and_shared_references()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ReverseItemSource, ReverseItemDestination>().ReverseMap();
            cfg.CreateMap<ReverseOrderSource, ReverseOrderDestination>().ReverseMap();
        }).CreateMapper();

        var destination = mapper.Map<ReverseOrderDestination>(new ReverseOrderSource
        {
            Id = 7,
            Items =
            {
                new ReverseItemSource { Id = 1 },
                new ReverseItemSource { Id = 2 }
            }
        });

        Assert.Equal(7L, destination.Id);
        Assert.Equal(new[] { 1, 2 }, destination.Items.Select(item => item.Id));

        var source = mapper.Map<ReverseOrderSource>(destination);
        Assert.Equal(7, source.Id);
        Assert.Equal(new[] { 1, 2 }, source.Items.Select(item => item.Id));

        var shared = new ReverseItemDestination { Id = 9 };
        var sharedList = mapper.Map<List<ReverseItemSource>>(
            new List<ReverseItemDestination> { shared, shared });

        Assert.Same(sharedList[0], sharedList[1]);
    }

    [Fact]
    public void ReverseMap_preserves_circular_references()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<ReverseNodeSource, ReverseNodeDestination>().ReverseMap())
            .CreateMapper();

        var first = new ReverseNodeSource { Name = "first" };
        var second = new ReverseNodeSource { Name = "second", Next = first };
        first.Next = second;

        var destination = mapper.Map<ReverseNodeDestination>(first);
        Assert.Same(destination, destination.Next!.Next);

        var reverse = mapper.Map<ReverseNodeSource>(destination);
        Assert.Same(reverse, reverse.Next!.Next);
    }

    [Fact]
    public void ReverseMap_preserves_strict_conversion_errors()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<ReverseNumberSource, ReverseNumberDestination>().ReverseMap())
            .CreateMapper();

        var exception = Assert.Throws<MappingException>(() =>
            mapper.Map<ReverseNumberSource>(new ReverseNumberDestination
            {
                Value = long.MaxValue
            }));

        Assert.Equal("Value", exception.MemberPath);
        Assert.Contains(nameof(ReverseNumberDestination), exception.Message);
        Assert.Contains(nameof(ReverseNumberSource), exception.Message);
    }

    [Fact]
    public void ReverseMap_can_be_registered_in_a_profile()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfile(new ReverseMapProfile())).CreateMapper();

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
    public void Duplicate_reverse_maps_are_rejected()
    {
        var configuration = new MapperConfiguration(_ => { });
        var expression = configuration.CreateMap<ReverseFlatSource, ReverseFlatDestination>();
        expression.ReverseMap();

        Assert.Throws<MappingConfigurationException>(() => expression.ReverseMap());
    }

    [Fact]
    public void Explicit_reverse_map_before_ReverseMap_is_rejected()
    {
        Assert.Throws<MappingConfigurationException>(() =>
            new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ReverseFlatDestination, ReverseFlatSource>();
                cfg.CreateMap<ReverseFlatSource, ReverseFlatDestination>().ReverseMap();
            }));
    }

    [Fact]
    public void Explicit_reverse_map_after_ReverseMap_is_rejected()
    {
        var configuration = new MapperConfiguration(_ => { });
        configuration.CreateMap<ReverseFlatSource, ReverseFlatDestination>().ReverseMap();

        Assert.Throws<MappingConfigurationException>(
            () => configuration.CreateMap<ReverseFlatDestination, ReverseFlatSource>());
    }

    [Fact]
    public void Reverse_map_snapshot_is_isolated_from_existing_mapper()
    {
        var configuration = new MapperConfiguration(_ => { });
        var expression = configuration.CreateMap<ReverseFlatSource, ReverseFlatDestination>();
        var mapper = configuration.CreateMapper();

        expression.ReverseMap();

        Assert.Throws<InvalidOperationException>(
            () => mapper.Map<ReverseFlatSource>(new ReverseFlatDestination()));

        var newMapper = configuration.CreateMapper();
        var source = newMapper.Map<ReverseFlatSource>(
            new ReverseFlatDestination { Id = 5, FirstName = "Ada" });

        Assert.Equal(5, source.Id);
        Assert.Equal("Ada", source.FirstName);
    }

    [Fact]
    public void Reverse_map_participates_in_configuration_validation()
    {
        var validConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ReverseAddressSource, ReverseAddressDestination>().ReverseMap();
            cfg.CreateMap<ReverseCustomerSource, ReverseCustomerDestination>().ReverseMap();
        });

        validConfiguration.AssertConfigurationIsValid();

        var invalidConfiguration = new MapperConfiguration(cfg =>
            cfg.CreateMap<ReverseCustomerSource, ReverseCustomerDestination>()
                .Ignore(dest => dest.Address)
                .ReverseMap());

        var exception = Assert.Throws<MappingConfigurationException>(
            invalidConfiguration.AssertConfigurationIsValid);

        Assert.Contains("Address", exception.Message);
        Assert.Contains("(reverse)", exception.Message);
        Assert.Contains(nameof(ReverseCustomerDestination), exception.Message);
        Assert.Contains(nameof(ReverseCustomerSource), exception.Message);
    }

    [Fact]
    public void Reverse_map_validation_reports_missing_collection_element_maps()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ReverseItemSource, ReverseItemDestination>();
            cfg.CreateMap<ReverseOrderSource, ReverseOrderDestination>().ReverseMap();
        });

        var exception = Assert.Throws<MappingConfigurationException>(
            configuration.AssertConfigurationIsValid);

        Assert.Contains("Items", exception.Message);
        Assert.Contains("[]", exception.Message);
        Assert.Contains("(reverse)", exception.Message);
        Assert.Contains(nameof(ReverseItemDestination), exception.Message);
        Assert.Contains(nameof(ReverseItemSource), exception.Message);
    }

    [Fact]
    public void ReverseMap_can_be_used_concurrently()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<ReverseFlatSource, ReverseFlatDestination>().ReverseMap())
            .CreateMapper();

        Parallel.For(0, 32, index =>
        {
            var forward = mapper.Map<ReverseFlatDestination>(
                new ReverseFlatSource { Id = index, FirstName = $"f-{index}" });
            var reverse = mapper.Map<ReverseFlatSource>(
                new ReverseFlatDestination { Id = index, FirstName = $"r-{index}" });

            Assert.Equal(index, forward.Id);
            Assert.Equal($"f-{index}", forward.FirstName);
            Assert.Equal(index, reverse.Id);
            Assert.Equal($"r-{index}", reverse.FirstName);
        });
    }
}
