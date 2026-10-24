using System.Collections.Concurrent;

namespace Mecesoft.ObjectMapper.Tests;

public sealed class CollectionReferenceTests
{
    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CollectionSource, CollectionDestination>();
            cfg.CreateMap<CollectionItem, CollectionItemDto>();
            cfg.CreateMap<Address, AddressDto>();
            cfg.CreateMap<Category, CategoryDto>();
            cfg.CreateMap<Product, ProductDto>();
            cfg.CreateMap<TreeNode, TreeNodeDto>();
            cfg.CreateMap<SharedCollectionHolderSource, SharedCollectionHolderDestination>();
            cfg.CreateMap<MixedSharedCollectionSource, MixedSharedCollectionDestination>();
            cfg.CreateMap<SharedAcrossPropertiesSource, SharedAcrossPropertiesDestination>();
        });

        return configuration.CreateMapper();
    }

    [Fact]
    public void Shared_element_reference_is_preserved()
    {
        var item = new CollectionItem { Id = 1, Name = "shared" };
        var source = new CollectionSource
        {
            Items = new List<CollectionItem> { item, item }
        };

        var destination = CreateMapper().Map<CollectionDestination>(source);

        Assert.Equal(2, destination.Items.Count);
        Assert.Same(destination.Items[0], destination.Items[1]);
    }

    [Fact]
    public void Collection_object_collection_cycle_is_preserved()
    {
        var category = new Category { Name = "root" };
        var product = new Product { Name = "product", Category = category };
        category.Products.Add(product);

        var destination = CreateMapper().Map<CategoryDto>(category);

        Assert.Single(destination.Products);
        Assert.Equal("product", destination.Products[0].Name);
        Assert.Same(destination, destination.Products[0].Category);
    }

    [Fact]
    public void Collection_element_referencing_its_owner_is_preserved()
    {
        var root = new TreeNode { Name = "root" };
        root.Children.Add(root);

        var destination = CreateMapper().Map<TreeNodeDto>(root);

        Assert.Equal("root", destination.Name);
        Assert.Single(destination.Children);
        Assert.Same(destination, destination.Children[0]);
    }

    [Fact]
    public void Shared_collection_reference_is_preserved()
    {
        var items = new List<CollectionItem> { new() { Id = 1 } };
        var source = new SharedCollectionHolderSource
        {
            First = items,
            Second = items
        };

        var destination = CreateMapper().Map<SharedCollectionHolderDestination>(source);

        Assert.Same(destination.First, destination.Second);
    }

    [Fact]
    public void Shared_collection_reference_is_preserved_across_compatible_destination_shapes()
    {
        var items = new List<CollectionItem> { new() { Id = 1 } };
        var source = new MixedSharedCollectionSource
        {
            AsList = items,
            AsReadOnly = items
        };

        var destination = CreateMapper().Map<MixedSharedCollectionDestination>(source);

        Assert.Same(destination.AsList, destination.AsReadOnly);
    }

    [Fact]
    public void Shared_reference_is_preserved_between_a_property_and_a_collection_element()
    {
        var item = new CollectionItem { Id = 1, Name = "shared" };
        var source = new SharedAcrossPropertiesSource
        {
            Single = item,
            Items = new List<CollectionItem> { item }
        };

        var destination = CreateMapper().Map<SharedAcrossPropertiesDestination>(source);

        Assert.NotNull(destination.Single);
        Assert.Same(destination.Single, destination.Items[0]);
    }

    [Fact]
    public void Reference_tracking_is_reset_between_top_level_collection_maps()
    {
        var item = new CollectionItem { Id = 1 };
        var source = new CollectionSource
        {
            Items = new List<CollectionItem> { item }
        };
        var mapper = CreateMapper();

        var first = mapper.Map<CollectionDestination>(source);
        var second = mapper.Map<CollectionDestination>(source);

        Assert.NotSame(first.Items, second.Items);
        Assert.NotSame(first.Items[0], second.Items[0]);
    }

    [Fact]
    public void Concurrent_collection_source_is_mapped()
    {
        var source = new ConcurrentBag<CollectionItem>
        {
            new() { Id = 1 },
            new() { Id = 2 },
            new() { Id = 3 }
        };

        var destination = CreateMapper().Map<List<CollectionItemDto>>(source);

        Assert.Equal(3, destination.Count);
        Assert.Equal(
            new long[] { 1, 2, 3 },
            destination.Select(item => item.Id).OrderBy(id => id));
    }

    [Fact]
    public void Concurrent_queue_source_is_mapped()
    {
        var source = new ConcurrentQueue<CollectionItem>();
        source.Enqueue(new CollectionItem { Id = 4 });
        source.Enqueue(new CollectionItem { Id = 5 });

        var destination = CreateMapper().Map<CollectionItemDto[]>(source);

        Assert.Equal(new long[] { 4, 5 }, destination.Select(item => item.Id));
    }

    [Fact]
    public void Parallel_mapping_calls_are_isolated()
    {
        var mapper = CreateMapper();
        var results = new ConcurrentBag<CollectionDestination>();

        Parallel.For(0, 32, index =>
        {
            var source = new CollectionSource
            {
                Items = new List<CollectionItem>
                {
                    new() { Id = index, Name = $"item-{index}" }
                }
            };

            results.Add(mapper.Map<CollectionDestination>(source));
        });

        Assert.Equal(32, results.Count);
        Assert.All(results, destination => Assert.Single(destination.Items));
        Assert.Equal(32, results.Select(destination => destination.Items[0].Id).Distinct().Count());
    }
}
