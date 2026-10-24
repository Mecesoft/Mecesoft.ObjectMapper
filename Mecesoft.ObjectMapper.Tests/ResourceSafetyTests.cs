namespace Mecesoft.ObjectMapper.Tests;

public sealed class ResourceSafetyTests
{
    [Fact]
    public void Default_max_depth_is_128()
    {
        Assert.Equal(128, new MapperConfigurationOptions().MaxDepth);
    }

    [Fact]
    public void Max_depth_throws_for_deep_acyclic_graph()
    {
        var configuration = new MapperConfiguration(
            cfg => cfg.CreateMap<DeepNodeSource, DeepNodeDto>(),
            new MapperConfigurationOptions { MaxDepth = 4 });
        var mapper = configuration.CreateMapper();
        var source = CreateChain(10);

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<DeepNodeDto>(source));

        Assert.Contains("Maximum mapping depth", exception.Message);
        Assert.Contains("Next", exception.MemberPath);
        Assert.Equal(typeof(DeepNodeSource), exception.SourceType);
        Assert.Equal(typeof(DeepNodeDto), exception.DestinationType);
    }

    [Fact]
    public void Max_depth_can_be_increased_for_legitimate_graphs()
    {
        var configuration = new MapperConfiguration(
            cfg => cfg.CreateMap<DeepNodeSource, DeepNodeDto>(),
            new MapperConfigurationOptions { MaxDepth = 20 });
        var mapper = configuration.CreateMapper();
        var source = CreateChain(10);

        var destination = mapper.Map<DeepNodeDto>(source);

        Assert.Equal("0", destination.Name);
        Assert.NotNull(destination.Next);
    }

    [Fact]
    public void Cycle_detection_is_checked_before_max_depth()
    {
        var configuration = new MapperConfiguration(
            cfg => cfg.CreateMap<DeepNodeSource, DeepNodeDto>(),
            new MapperConfigurationOptions { MaxDepth = 1 });
        var mapper = configuration.CreateMapper();
        var source = new DeepNodeSource { Name = "self" };
        source.Next = source;

        var destination = mapper.Map<DeepNodeDto>(source);

        Assert.Equal("self", destination.Name);
        Assert.Same(destination, destination.Next);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Max_depth_must_be_positive(int maxDepth)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MapperConfiguration(
                _ => { },
                new MapperConfigurationOptions { MaxDepth = maxDepth }));
    }

    [Fact]
    public void Max_depth_is_captured_when_configuration_is_created()
    {
        var options = new MapperConfigurationOptions { MaxDepth = 4 };
        var configuration = new MapperConfiguration(
            cfg => cfg.CreateMap<DeepNodeSource, DeepNodeDto>(),
            options);

        options.MaxDepth = 100;
        var mapper = configuration.CreateMapper();

        Assert.Throws<MappingException>(() => mapper.Map<DeepNodeDto>(CreateChain(10)));
    }

    private static DeepNodeSource CreateChain(int length)
    {
        var root = new DeepNodeSource { Name = "0" };
        var current = root;

        for (var index = 1; index < length; index++)
        {
            var next = new DeepNodeSource { Name = index.ToString() };
            current.Next = next;
            current = next;
        }

        return root;
    }
}
