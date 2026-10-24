namespace Mecesoft.ObjectMapper.Tests;

public sealed class BasicMappingTests
{
    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BasicSource, BasicDestination>();
            cfg.CreateMap<Address, AddressDto>();
        });

        return configuration.CreateMapper();
    }

    [Fact]
    public void Maps_matching_properties_by_name()
    {
        var source = new BasicSource
        {
            Id = 7,
            FirstName = "Ada",
            LastName = "Lovelace",
            Note = "note",
            Secret = "source-secret",
            OptionalNumber = 3,
            NullableNumber = 5,
            NullableNumberNullable = 6,
            NullableText = "text",
            Address = new Address { City = "Istanbul", Street = "Istiklal" }
        };

        var destination = CreateMapper().Map<BasicDestination>(source);

        Assert.Equal(7, destination.Id);
        Assert.Equal("Ada", destination.FirstName);
        Assert.Equal("Lovelace", destination.LastName);
        Assert.Equal("note", destination.Note);
        Assert.Equal("source-secret", destination.Secret);
        Assert.Equal(3, destination.OptionalNumber);
        Assert.Equal(5, destination.NullableNumber);
        Assert.Equal(6, destination.NullableNumberNullable);
        Assert.Equal("text", destination.NullableText);
        Assert.Equal("Istanbul", destination.Address?.City);
        Assert.Equal("Istiklal", destination.Address?.Street);
    }

    [Fact]
    public void Maps_with_explicit_source_and_destination_generics()
    {
        var destination = CreateMapper().Map<BasicSource, BasicDestination>(
            new BasicSource { Id = 11 });

        Assert.Equal(11, destination.Id);
    }

    [Fact]
    public void Maps_with_non_generic_overload()
    {
        var destination = CreateMapper().Map(new BasicSource { Id = 12 }, typeof(BasicDestination));

        var typed = Assert.IsType<BasicDestination>(destination);
        Assert.Equal(12, typed.Id);
    }

    [Fact]
    public void Leaves_destination_only_properties_untouched()
    {
        var destination = CreateMapper().Map<BasicDestination>(new BasicSource());

        Assert.Equal("unchanged", destination.DestinationOnly);
    }

    [Fact]
    public void Does_not_write_read_only_destination_properties()
    {
        var destination = CreateMapper().Map<BasicDestination>(new BasicSource());

        Assert.Equal("read-only", destination.ReadOnly);
    }

    [Fact]
    public void Keeps_default_for_null_source_value_on_non_nullable_destination()
    {
        var destination = CreateMapper().Map<BasicDestination>(
            new BasicSource { NullableNumber = null });

        Assert.Equal(42, destination.NullableNumber);
    }

    [Fact]
    public void Sets_null_for_null_source_value_on_nullable_destination()
    {
        var destination = CreateMapper().Map<BasicDestination>(
            new BasicSource { NullableNumberNullable = null, NullableText = null });

        Assert.Null(destination.NullableNumberNullable);
        Assert.Null(destination.NullableText);
    }

    [Fact]
    public void Returns_source_instance_when_destination_is_assignable()
    {
        var dog = new Dog { Name = "Rex", BarkVolume = 3 };
        var mapper = new MapperConfiguration(_ => { }).CreateMapper();

        var mapped = mapper.Map<Animal>(dog);

        Assert.Same(dog, mapped);
    }

    [Fact]
    public void Throws_when_no_map_is_configured()
    {
        var mapper = new MapperConfiguration(_ => { }).CreateMapper();

        var exception = Assert.Throws<InvalidOperationException>(
            () => mapper.Map<NoMapDestination>(new BasicSource()));

        Assert.Contains("No map configured from 'BasicSource' to 'NoMapDestination'", exception.Message);
    }

    [Fact]
    public void Throws_for_null_source_in_all_overloads()
    {
        var mapper = CreateMapper();

        var objectOverload = Assert.Throws<ArgumentNullException>(
            () => mapper.Map<BasicDestination>(null!));
        var genericOverload = Assert.Throws<ArgumentNullException>(
            () => mapper.Map<BasicSource, BasicDestination>(null!));
        var nonGenericOverload = Assert.Throws<ArgumentNullException>(
            () => mapper.Map(null!, typeof(BasicDestination)));

        Assert.Equal("source", objectOverload.ParamName);
        Assert.Equal("source", genericOverload.ParamName);
        Assert.Equal("source", nonGenericOverload.ParamName);
    }

    [Fact]
    public void Throws_for_null_destination_type()
    {
        var mapper = CreateMapper();

        var exception = Assert.Throws<ArgumentNullException>(
            () => mapper.Map(new BasicSource(), null!));

        Assert.Equal("destinationType", exception.ParamName);
    }

    [Fact]
    public void Throws_when_destination_has_no_parameterless_constructor()
    {
        var mapper = new MapperConfiguration(
            cfg => cfg.CreateMap<BasicSource, NoDefaultConstructorDestination>()).CreateMapper();

        Assert.Throws<MissingMethodException>(
            () => mapper.Map<NoDefaultConstructorDestination>(new BasicSource()));
    }
}
