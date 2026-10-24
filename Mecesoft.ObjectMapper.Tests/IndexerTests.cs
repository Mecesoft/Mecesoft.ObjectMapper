namespace Mecesoft.ObjectMapper.Tests;

public sealed class IndexerTests
{
    [Fact]
    public void Ignores_source_and_destination_indexers()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<IndexerSource, IndexerDestination>()).CreateMapper();

        var destination = mapper.Map<IndexerDestination>(
            new IndexerSource { Id = 5 });

        Assert.Equal(5, destination.Id);
        Assert.Equal("initial", destination.StoredValue);
    }

    [Fact]
    public void Does_not_use_source_indexer_for_named_destination_property()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<IndexerOnlySource, NamedItemDestination>()).CreateMapper();

        var destination = mapper.Map<NamedItemDestination>(new IndexerOnlySource());

        Assert.Equal("default", destination.Item);
    }
}
