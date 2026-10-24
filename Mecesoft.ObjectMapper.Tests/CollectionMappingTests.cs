namespace Mecesoft.ObjectMapper.Tests;

public sealed class CollectionMappingTests
{
    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CollectionSource, CollectionDestination>();
            cfg.CreateMap<CollectionItem, CollectionItemDto>();
            cfg.CreateMap<Address, AddressDto>();
            cfg.CreateMap<CatalogSource, CatalogDestination>();
            cfg.CreateMap<CatalogDetails, CatalogDetailsDto>();
        });

        return configuration.CreateMapper();
    }

    [Fact]
    public void Maps_all_supported_collection_shapes()
    {
        var source = new CollectionSource
        {
            Items = new List<CollectionItem>
            {
                new()
                {
                    Id = 1,
                    Name = "one",
                    Address = new Address { City = "Istanbul", Street = "Istiklal" }
                }
            },
            ArrayItems = new[] { new CollectionItem { Id = 2, Name = "two" } },
            EnumerableItems = GetItems(3),
            CollectionItems = new List<CollectionItem> { new() { Id = 4, Name = "four" } },
            ListItems = new List<CollectionItem> { new() { Id = 5, Name = "five" } },
            ReadOnlyCollectionItems = new[] { new CollectionItem { Id = 6, Name = "six" } },
            ReadOnlyListItems = new List<CollectionItem> { new() { Id = 7, Name = "seven" } }
        };

        var destination = CreateMapper().Map<CollectionDestination>(source);

        Assert.Single(destination.Items);
        Assert.Equal(1L, destination.Items[0].Id);
        Assert.Equal("Istanbul", destination.Items[0].Address?.City);

        Assert.IsType<CollectionItemDto[]>(destination.ArrayItems);
        Assert.Single(destination.ArrayItems);
        Assert.Equal(2L, destination.ArrayItems[0].Id);

        Assert.IsType<List<CollectionItemDto>>(destination.EnumerableItems);
        Assert.Single(destination.EnumerableItems);
        Assert.Equal(3L, destination.EnumerableItems[0].Id);

        Assert.IsType<List<CollectionItemDto>>(destination.CollectionItems);
        Assert.Single(destination.CollectionItems);
        Assert.Equal(4L, destination.CollectionItems.Single().Id);

        Assert.IsType<List<CollectionItemDto>>(destination.ListItems);
        Assert.Single(destination.ListItems);
        Assert.Equal(5L, destination.ListItems[0].Id);

        Assert.IsType<List<CollectionItemDto>>(destination.ReadOnlyCollectionItems);
        Assert.Single(destination.ReadOnlyCollectionItems);
        Assert.Equal(6L, destination.ReadOnlyCollectionItems.Single().Id);

        Assert.IsType<List<CollectionItemDto>>(destination.ReadOnlyListItems);
        Assert.Single(destination.ReadOnlyListItems);
        Assert.Equal(7L, destination.ReadOnlyListItems[0].Id);
    }

    [Fact]
    public void Maps_top_level_collections()
    {
        var mapper = CreateMapper();
        var source = new List<CollectionItem>
        {
            new() { Id = 1, Name = "one" },
            new() { Id = 2, Name = "two" }
        };

        var list = mapper.Map<List<CollectionItemDto>>(source);
        Assert.Equal(new long[] { 1, 2 }, list.Select(item => item.Id));

        var array = mapper.Map<CollectionItemDto[]>(source);
        Assert.IsType<CollectionItemDto[]>(array);
        Assert.Equal(new long[] { 1, 2 }, array.Select(item => item.Id));

        var readOnlyList = mapper.Map<IReadOnlyList<CollectionItemDto>>(
            source.Select(item => item));

        Assert.IsType<List<CollectionItemDto>>(readOnlyList);
        Assert.Equal(new long[] { 1, 2 }, readOnlyList.Select(item => item.Id));

        var collection = mapper.Map<ICollection<CollectionItemDto>>(source);
        Assert.IsType<List<CollectionItemDto>>(collection);
        Assert.Equal(2, collection.Count);

        var enumerable = mapper.Map<IEnumerable<CollectionItemDto>>(source);
        Assert.IsType<List<CollectionItemDto>>(enumerable);
        Assert.Equal(new long[] { 1, 2 }, enumerable.Select(item => item.Id));
    }

    [Fact]
    public void Maps_primitive_enum_guid_and_nullable_elements()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<PrimitiveCollectionsSource, PrimitiveCollectionsDestination>()).CreateMapper();

        var guid = Guid.Parse("2f4a6c70-8a76-4d0c-9db6-6edb3c8f0e7b");
        var source = new PrimitiveCollectionsSource
        {
            Numbers = new List<int> { 1, 2 },
            Guids = new List<string> { guid.ToString() },
            Statuses = new List<string> { "Active", "disabled" },
            OptionalNumbers = new List<int?> { 1, null, 3 },
            StatusNumbers = new List<int> { 0, 2 }
        };

        var destination = mapper.Map<PrimitiveCollectionsDestination>(source);

        Assert.Equal(new long[] { 1, 2 }, destination.Numbers);
        Assert.Equal(new[] { guid }, destination.Guids);
        Assert.Equal(new[] { Status.Active, Status.Disabled }, destination.Statuses);
        Assert.Equal(new long?[] { 1, null, 3 }, destination.OptionalNumbers);
        Assert.Equal(new[] { CodeType.None, CodeType.Premium }, destination.StatusNumbers);
    }

    [Fact]
    public void Empty_collections_create_empty_destinations()
    {
        var destination = CreateMapper().Map<CollectionDestination>(new CollectionSource());

        Assert.NotNull(destination.Items);
        Assert.Empty(destination.Items);

        Assert.NotNull(destination.ArrayItems);
        Assert.Empty(destination.ArrayItems);

        Assert.NotNull(destination.EnumerableItems);
        Assert.Empty(destination.EnumerableItems);

        Assert.NotNull(destination.CollectionItems);
        Assert.Empty(destination.CollectionItems);

        Assert.NotNull(destination.ListItems);
        Assert.Empty(destination.ListItems);

        Assert.NotNull(destination.ReadOnlyCollectionItems);
        Assert.Empty(destination.ReadOnlyCollectionItems);

        Assert.NotNull(destination.ReadOnlyListItems);
        Assert.Empty(destination.ReadOnlyListItems);
    }

    [Fact]
    public void Null_source_collections_remain_null()
    {
        var source = new CollectionSource
        {
            Items = null!,
            ArrayItems = null!,
            EnumerableItems = null!,
            CollectionItems = null!,
            ListItems = null!,
            ReadOnlyCollectionItems = null!,
            ReadOnlyListItems = null!
        };

        var destination = CreateMapper().Map<CollectionDestination>(source);

        Assert.Null(destination.Items);
        Assert.Null(destination.ArrayItems);
        Assert.Null(destination.EnumerableItems);
        Assert.Null(destination.CollectionItems);
        Assert.Null(destination.ListItems);
        Assert.Null(destination.ReadOnlyCollectionItems);
        Assert.Null(destination.ReadOnlyListItems);
    }

    [Fact]
    public void Maps_collection_inside_nested_object()
    {
        var source = new CatalogSource
        {
            Details = new CatalogDetails
            {
                Items = new List<CollectionItem>
                {
                    new() { Id = 9, Name = "nested" }
                }
            }
        };

        var destination = CreateMapper().Map<CatalogDestination>(source);

        Assert.Single(destination.Details.Items);
        Assert.Equal(9L, destination.Details.Items[0].Id);
        Assert.Equal("nested", destination.Details.Items[0].Name);
    }

    [Fact]
    public void Null_reference_elements_are_preserved()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<NullableItemSource, NullableItemDestination>();
            cfg.CreateMap<CollectionItem, CollectionItemDto>();
            cfg.CreateMap<Address, AddressDto>();
        }).CreateMapper();

        var source = new NullableItemSource
        {
            Items = new List<CollectionItem?>
            {
                new() { Id = 1, Name = "present" },
                null
            }
        };

        var destination = mapper.Map<NullableItemDestination>(source);

        Assert.Equal(2, destination.Items.Count);
        Assert.NotNull(destination.Items[0]);
        Assert.Equal(1L, destination.Items[0]!.Id);
        Assert.Null(destination.Items[1]);
    }

    [Fact]
    public void Null_reference_elements_are_preserved_in_arrays()
    {
        var source = new CollectionSource
        {
            ArrayItems = new CollectionItem[]
            {
                new() { Id = 1, Name = "present" },
                null!
            }
        };

        var destination = CreateMapper().Map<CollectionDestination>(source);

        Assert.Equal(2, destination.ArrayItems.Length);
        Assert.NotNull(destination.ArrayItems[0]);
        Assert.Null(destination.ArrayItems[1]);
    }

    [Fact]
    public void Null_nullable_elements_are_preserved()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<NullableElementSource, NullableElementDestination>()).CreateMapper();

        var source = new NullableElementSource
        {
            Values = new List<string?> { "1", null, "3" }
        };

        var destination = mapper.Map<NullableElementDestination>(source);

        Assert.Equal(new int?[] { 1, null, 3 }, destination.Values);
    }

    [Fact]
    public void Maps_nested_collections()
    {
        var mapper = CreateMapper();
        var source = new List<List<CollectionItem>>
        {
            new() { new CollectionItem { Id = 1, Name = "one" } }
        };

        var destination = mapper.Map<List<List<CollectionItemDto>>>(source);

        Assert.Single(destination);
        Assert.Single(destination[0]);
        Assert.Equal(1L, destination[0][0].Id);
        Assert.Equal("one", destination[0][0].Name);
    }

    [Fact]
    public void Maps_enum_elements_to_numeric_and_string_values()
    {
        var mapper = new MapperConfiguration(_ => { }).CreateMapper();

        var numbers = mapper.Map<List<int>>(new[] { Status.Active, Status.Disabled });
        var names = mapper.Map<List<string>>(new[] { Status.Active, Status.Disabled });

        Assert.Equal(new[] { 1, 2 }, numbers);
        Assert.Equal(new[] { "Active", "Disabled" }, names);
    }

    [Fact]
    public void Maps_guid_elements_to_string_values()
    {
        var mapper = new MapperConfiguration(_ => { }).CreateMapper();
        var guid = Guid.Parse("2f4a6c70-8a76-4d0c-9db6-6edb3c8f0e7b");

        var destination = mapper.Map<List<string>>(new[] { guid });

        Assert.Equal(new[] { guid.ToString() }, destination);
    }

    [Fact]
    public void Maps_same_element_type_collection_to_a_new_instance()
    {
        var mapper = CreateMapper();
        var source = new List<CollectionItem>
        {
            new() { Id = 1, Name = "one" }
        };

        var destination = mapper.Map<List<CollectionItem>>(source);

        Assert.NotSame(source, destination);
        Assert.Single(destination);
        Assert.Same(source[0], destination[0]);

        var sourceArray = new[] { source[0] };
        var arrayDestination = mapper.Map<CollectionItem[]>(sourceArray);

        Assert.NotSame(sourceArray, arrayDestination);
        Assert.Single(arrayDestination);
        Assert.Same(sourceArray[0], arrayDestination[0]);
    }

    [Fact]
    public void Maps_enumerable_to_list_of_same_element_type()
    {
        var mapper = CreateMapper();
        IEnumerable<CollectionItem> source = GetItems(1);

        var destination = mapper.Map<List<CollectionItem>>(source);

        Assert.IsType<List<CollectionItem>>(destination);
        Assert.Single(destination);
        Assert.Equal(1, destination[0].Id);
    }

    [Fact]
    public void Maps_nullable_value_elements_in_arrays_and_lists()
    {
        var mapper = new MapperConfiguration(_ => { }).CreateMapper();
        var guid = Guid.Parse("2f4a6c70-8a76-4d0c-9db6-6edb3c8f0e7b");

        var nullableInts = mapper.Map<int?[]>(new string?[] { "1", null, "3" });
        var nullableGuids = mapper.Map<Guid?[]>(new string?[] { guid.ToString(), null });
        var nullableEnums = mapper.Map<List<Status?>>(new string?[] { "Active", null });
        var nullableLongs = mapper.Map<List<long?>>(new int?[] { 5, null });

        var guidsFromGuids = mapper.Map<List<Guid?>>(new[] { guid });
        var enumsFromEnums = mapper.Map<List<Status?>>(new[] { Status.Disabled });
        var intsFromInts = mapper.Map<List<int?>>(new[] { 7 });
        var guidsFromNullableGuids = mapper.Map<List<Guid>>(new Guid?[] { guid });

        Assert.Equal(new int?[] { 1, null, 3 }, nullableInts);
        Assert.Equal(new Guid?[] { guid, null }, nullableGuids);
        Assert.Equal(new Status?[] { Status.Active, null }, nullableEnums);
        Assert.Equal(new long?[] { 5, null }, nullableLongs);
        Assert.Equal(new Guid?[] { guid }, guidsFromGuids);
        Assert.Equal(new Status?[] { Status.Disabled }, enumsFromEnums);
        Assert.Equal(new int?[] { 7 }, intsFromInts);
        Assert.Equal(new Guid[] { guid }, guidsFromNullableGuids);
    }

    private static IEnumerable<CollectionItem> GetItems(int id)
    {
        yield return new CollectionItem { Id = id, Name = $"item-{id}" };
    }
}
