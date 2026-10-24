namespace Mecesoft.ObjectMapper.Tests;

public sealed class ConversionTests
{
    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<ConversionSource, ConversionDestination>()
                .ForMember(dest => dest.Status, options => options.MapFrom(src => src.StatusText))
                .ForMember(dest => dest.Code, options => options.MapFrom(src => src.StatusNumber))
                .ForMember(dest => dest.Guid, options => options.MapFrom(src => src.GuidText))
                .ForMember(dest => dest.InvalidGuid, options => options.MapFrom(src => src.InvalidGuidText))
                .ForMember(dest => dest.NarrowedNumber, options => options.MapFrom(src => src.LargeNumber))
                .ForMember(dest => dest.InvalidNumber, options => options.MapFrom(src => src.InvalidNumberText)));

        return configuration.CreateMapper();
    }

    [Fact]
    public void Converts_numeric_types()
    {
        var destination = CreateMapper().Map<ConversionDestination>(
            new ConversionSource { Number = 42 });

        Assert.Equal(42L, destination.Number);
    }

    [Fact]
    public void Converts_numeric_string_to_number()
    {
        var destination = CreateMapper().Map<ConversionDestination>(
            new ConversionSource { NumberText = "42" });

        Assert.Equal(42, destination.NumberText);
    }

    [Fact]
    public void Throws_for_invalid_numeric_string()
    {
        var exception = Assert.Throws<MappingException>(() =>
            CreateMapper().Map<ConversionDestination>(
                new ConversionSource { InvalidNumberText = "not-a-number" }));

        Assert.Equal("InvalidNumber", exception.MemberPath);
    }

    [Fact]
    public void Throws_when_numeric_conversion_overflows()
    {
        var exception = Assert.Throws<MappingException>(() =>
            CreateMapper().Map<ConversionDestination>(
                new ConversionSource { LargeNumber = long.MaxValue }));

        Assert.Equal("NarrowedNumber", exception.MemberPath);
    }

    [Fact]
    public void Converts_string_to_guid()
    {
        var guid = Guid.Parse("2f4a6c70-8a76-4d0c-9db6-6edb3c8f0e7b");

        var destination = CreateMapper().Map<ConversionDestination>(
            new ConversionSource { GuidText = guid.ToString() });

        Assert.Equal(guid, destination.Guid);
    }

    [Fact]
    public void Throws_for_invalid_guid()
    {
        var exception = Assert.Throws<MappingException>(() =>
            CreateMapper().Map<ConversionDestination>(
                new ConversionSource { InvalidGuidText = "not-a-guid" }));

        Assert.Equal("InvalidGuid", exception.MemberPath);
    }

    [Fact]
    public void Converts_string_to_enum_case_insensitively()
    {
        var destination = CreateMapper().Map<ConversionDestination>(
            new ConversionSource { StatusText = "active" });

        Assert.Equal(Status.Active, destination.Status);
    }

    [Fact]
    public void Converts_number_to_enum()
    {
        var destination = CreateMapper().Map<ConversionDestination>(
            new ConversionSource { StatusNumber = 2 });

        Assert.Equal(CodeType.Premium, destination.Code);
    }

    [Fact]
    public void Throws_for_unknown_enum_name()
    {
        var exception = Assert.Throws<MappingException>(() =>
            CreateMapper().Map<ConversionDestination>(
                new ConversionSource { StatusText = "NotAStatus" }));

        Assert.Equal("Status", exception.MemberPath);
        Assert.IsType<ArgumentException>(exception.InnerException);
    }

    [Fact]
    public void Throws_for_empty_enum_text()
    {
        var exception = Assert.Throws<MappingException>(() =>
            CreateMapper().Map<ConversionDestination>(
                new ConversionSource { StatusText = string.Empty }));

        Assert.Equal("Status", exception.MemberPath);
    }

    [Fact]
    public void Converts_nullable_value_type()
    {
        var destination = CreateMapper().Map<ConversionDestination>(
            new ConversionSource { OptionalNumber = 5 });

        Assert.Equal(5, destination.OptionalNumber);
    }

    [Fact]
    public void Failed_conversion_reports_source_type_destination_type_and_member_path()
    {
        var exception = Assert.Throws<MappingException>(() =>
            CreateMapper().Map<ConversionDestination>(
                new ConversionSource { InvalidGuidText = "not-a-guid" }));

        Assert.Equal(typeof(ConversionSource), exception.SourceType);
        Assert.Equal(typeof(ConversionDestination), exception.DestinationType);
        Assert.Equal("InvalidGuid", exception.MemberPath);
        Assert.Contains(nameof(ConversionSource), exception.Message);
        Assert.Contains(nameof(ConversionDestination), exception.Message);
        Assert.Contains("InvalidGuid", exception.Message);
        Assert.Contains(typeof(string).FullName!, exception.Message);
        Assert.Contains(typeof(Guid).FullName!, exception.Message);
        Assert.NotNull(exception.InnerException);
    }
}
