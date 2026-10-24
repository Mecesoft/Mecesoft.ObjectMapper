namespace Mecesoft.ObjectMapper.Tests;

public sealed class CollectionErrorTests
{
    [Fact]
    public void Invalid_element_conversion_reports_index_and_types()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<PrimitiveCollectionsSource, PrimitiveCollectionsDestination>()).CreateMapper();

        var source = new PrimitiveCollectionsSource
        {
            Guids = new List<string> { Guid.NewGuid().ToString(), "not-a-guid" }
        };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<PrimitiveCollectionsDestination>(source));

        Assert.Equal("Guids[1]", exception.MemberPath);
        Assert.Equal(typeof(PrimitiveCollectionsSource), exception.SourceType);
        Assert.Equal(typeof(PrimitiveCollectionsDestination), exception.DestinationType);
        Assert.Contains("Guids[1]", exception.Message);
        Assert.Contains(typeof(string).FullName!, exception.Message);
        Assert.Contains(typeof(Guid).FullName!, exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public void Null_element_to_non_nullable_type_reports_index()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<NullableElementSource, NonNullableElementDestination>()).CreateMapper();

        var source = new NullableElementSource
        {
            Values = new List<string?> { "1", null }
        };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<NonNullableElementDestination>(source));

        Assert.Equal("Values[1]", exception.MemberPath);
        Assert.Contains("null collection element", exception.Message);
        Assert.Contains(typeof(int).FullName!, exception.Message);
    }

    [Fact]
    public void Missing_element_map_reports_index()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<MissingElementSource, MissingElementDestination>()).CreateMapper();

        var source = new MissingElementSource
        {
            Items = new List<UnmappedThing> { new() { Id = 1 } }
        };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<MissingElementDestination>(source));

        Assert.Equal("Items[0]", exception.MemberPath);
        Assert.Contains("no conversion or nested map", exception.Message);
        Assert.Contains(nameof(UnmappedThing), exception.Message);
        Assert.Contains(nameof(UnmappedThingDto), exception.Message);
    }

    [Fact]
    public void Nested_element_conversion_reports_full_indexed_path()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<IndexedOrderSource, IndexedOrderDestination>();
            cfg.CreateMap<IndexedLineSource, IndexedLineDestination>();
        }).CreateMapper();

        var source = new IndexedOrderSource
        {
            Orders = new List<IndexedLineSource>
            {
                new() { Price = "1" },
                new() { Price = "2" },
                new() { Price = "not-a-number" }
            }
        };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<IndexedOrderDestination>(source));

        Assert.Equal("Orders[2].Price", exception.MemberPath);
        Assert.Equal(typeof(IndexedLineSource), exception.SourceType);
        Assert.Equal(typeof(IndexedLineDestination), exception.DestinationType);
        Assert.Contains("Orders[2].Price", exception.Message);
        Assert.Contains(typeof(string).FullName!, exception.Message);
        Assert.Contains(typeof(int).FullName!, exception.Message);
    }

    [Fact]
    public void Unsupported_destination_collection_type_throws()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UnsupportedCollectionSource, UnsupportedCollectionDestination>();
            cfg.CreateMap<CollectionItem, CollectionItemDto>();
        }).CreateMapper();

        var source = new UnsupportedCollectionSource
        {
            Items = new List<CollectionItem> { new() { Id = 1 } }
        };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<UnsupportedCollectionDestination>(source));

        Assert.Equal("Items", exception.MemberPath);
        Assert.Contains("not supported", exception.Message);
        Assert.Contains("HashSet", exception.Message);
    }

    [Fact]
    public void Ignored_unsupported_collection_member_does_not_throw()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<UnsupportedCollectionSource, UnsupportedCollectionDestination>()
                .Ignore(dest => dest.Items)).CreateMapper();

        var source = new UnsupportedCollectionSource
        {
            Items = new List<CollectionItem> { new() { Id = 1 } }
        };

        var destination = mapper.Map<UnsupportedCollectionDestination>(source);

        Assert.Empty(destination.Items);
    }

    [Fact]
    public void Dictionary_mapping_throws()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<DictionaryCollectionSource, DictionaryCollectionDestination>()).CreateMapper();

        var source = new DictionaryCollectionSource
        {
            Values = new Dictionary<string, int> { ["one"] = 1 }
        };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<DictionaryCollectionDestination>(source));

        Assert.Equal("Values", exception.MemberPath);
        Assert.Contains("dictionary mapping is not supported", exception.Message);
    }

    [Fact]
    public void Non_generic_collection_source_throws()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<NonGenericCollectionSource, NonGenericCollectionDestination>()).CreateMapper();

        var source = new NonGenericCollectionSource();
        source.Values.Add(1);

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<NonGenericCollectionDestination>(source));

        Assert.Equal("Values", exception.MemberPath);
        Assert.Contains("ArrayList", exception.Message);
        Assert.Contains("not a supported generic collection", exception.Message);
    }

    [Fact]
    public void Unsupported_same_type_collection_throws()
    {
        var mapper = new MapperConfiguration(_ => { }).CreateMapper();

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<HashSet<int>>(new HashSet<int> { 1 }));

        Assert.Contains("not supported", exception.Message);
        Assert.Contains("HashSet", exception.Message);
    }

    [Fact]
    public void Dictionary_same_type_throws()
    {
        var mapper = new MapperConfiguration(_ => { }).CreateMapper();

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<Dictionary<string, int>>(
                new Dictionary<string, int> { ["one"] = 1 }));

        Assert.Contains("dictionary mapping is not supported", exception.Message);
    }
}
