namespace Mecesoft.ObjectMapper.Tests;

public sealed class MemberConfigurationTests
{
    [Fact]
    public void ForMember_maps_from_a_custom_expression()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<BasicSource, BasicDestination>()
                .ForMember(
                    dest => dest.FullName,
                    options => options.MapFrom(src => $"{src.FirstName} {src.LastName}")))
            .CreateMapper();

        var destination = mapper.Map<BasicDestination>(
            new BasicSource { FirstName = "Ada", LastName = "Lovelace" });

        Assert.Equal("Ada Lovelace", destination.FullName);
    }

    [Fact]
    public void Ignore_leaves_the_destination_default_value()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<BasicSource, BasicDestination>()
                .Ignore(dest => dest.Secret))
            .CreateMapper();

        var destination = mapper.Map<BasicDestination>(
            new BasicSource { Secret = "source-secret" });

        Assert.Equal("destination-secret", destination.Secret);
    }

    [Fact]
    public void MapFrom_overrides_conventional_mapping()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<BasicSource, BasicDestination>()
                .ForMember(dest => dest.Id, options => options.MapFrom(src => src.Id * 10)))
            .CreateMapper();

        var destination = mapper.Map<BasicDestination>(new BasicSource { Id = 7 });

        Assert.Equal(70, destination.Id);
    }

    [Fact]
    public void Last_configuration_for_a_member_wins()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<BasicSource, BasicDestination>()
                .ForMember(dest => dest.FullName, options => options.MapFrom(_ => "first"))
                .ForMember(dest => dest.FullName, options => options.MapFrom(_ => "second")))
            .CreateMapper();

        var destination = mapper.Map<BasicDestination>(new BasicSource());

        Assert.Equal("second", destination.FullName);
    }

    [Fact]
    public void MapFrom_returning_null_assigns_null_to_nullable_destination()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<BasicSource, BasicDestination>()
                .ForMember(dest => dest.OptionalNumber, options => options.MapFrom(_ => null)))
            .CreateMapper();

        var destination = mapper.Map<BasicDestination>(
            new BasicSource { OptionalNumber = 9 });

        Assert.Null(destination.OptionalNumber);
    }

    [Fact]
    public void Invalid_destination_expression_throws()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new MapperConfiguration(cfg =>
                cfg.CreateMap<BasicSource, BasicDestination>()
                    .ForMember(
                        dest => dest.Id + 1,
                        options => options.MapFrom(src => src.Id))));

        Assert.Contains("Expression must target a member", exception.Message);
    }

    [Fact]
    public void Nested_destination_member_paths_are_rejected()
    {
        var exception = Assert.Throws<MappingConfigurationException>(() =>
            new MapperConfiguration(cfg =>
                cfg.CreateMap<ContactSource, ContactDestination>()
                    .ForMember(
                        dest => dest.Details.City,
                        options => options.MapFrom(src => src.Details.City))));

        Assert.Contains("Nested destination member paths are not supported", exception.Message);
        Assert.Contains("Details.City", exception.Message);
    }

    [Fact]
    public void Nested_destination_paths_are_rejected_for_ignore()
    {
        var exception = Assert.Throws<MappingConfigurationException>(() =>
            new MapperConfiguration(cfg =>
                cfg.CreateMap<ContactSource, ContactDestination>()
                    .Ignore(dest => dest.Details.City)));

        Assert.Contains("Details.City", exception.Message);
    }

    [Fact]
    public void ForMember_can_map_a_top_level_nested_object()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ContactSource, ContactDestination>()
                .ForMember(dest => dest.Details, options => options.MapFrom(src => src.Details));
            cfg.CreateMap<ContactDetails, ContactDetailsDto>();
        }).CreateMapper();

        var destination = mapper.Map<ContactDestination>(new ContactSource
        {
            Details = new ContactDetails { City = "Istanbul", Zip = "34000" }
        });

        Assert.Equal("Istanbul", destination.Details.City);
        Assert.Equal(34000, destination.Details.Zip);
    }

    [Fact]
    public void Ignored_member_does_not_raise_conversion_error()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<IgnoredFailureSource, IgnoredFailureDestination>()
                .Ignore(dest => dest.Value)).CreateMapper();

        var destination = mapper.Map<IgnoredFailureDestination>(
            new IgnoredFailureSource { Value = "not-an-int" });

        Assert.Equal(7, destination.Value);
    }

    [Fact]
    public void MapFrom_exception_is_wrapped_with_member_path()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<BasicSource, BasicDestination>()
                .ForMember(
                    dest => dest.FullName,
                    options => options.MapFrom(_ => throw new InvalidOperationException("boom"))))
            .CreateMapper();

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<BasicDestination>(new BasicSource()));

        Assert.Equal("FullName", exception.MemberPath);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("FullName", exception.Message);
    }
}
