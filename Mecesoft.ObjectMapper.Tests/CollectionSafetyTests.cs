using System.Diagnostics;

namespace Mecesoft.ObjectMapper.Tests;

public sealed class CollectionSafetyTests
{
    [Fact]
    public void Default_max_collection_size_is_100000()
    {
        Assert.Equal(100_000, new MapperConfigurationOptions().MaxCollectionSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Max_collection_size_must_be_positive(int maxCollectionSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MapperConfiguration(
                _ => { },
                new MapperConfigurationOptions { MaxCollectionSize = maxCollectionSize }));
    }

    [Fact]
    public void Collection_under_limit_maps_all_items()
    {
        var mapper = CreateListMapper(maxCollectionSize: 5);
        var source = new EnumerableSource { Values = new List<int> { 1, 2, 3, 4 } };

        var destination = mapper.Map<IntListDestination>(source);

        Assert.Equal(new[] { 1, 2, 3, 4 }, destination.Values);
    }

    [Fact]
    public void Collection_at_limit_maps_all_items()
    {
        var mapper = CreateListMapper(maxCollectionSize: 5);
        var source = new EnumerableSource { Values = new List<int> { 1, 2, 3, 4, 5 } };

        var destination = mapper.Map<IntListDestination>(source);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, destination.Values);
    }

    [Fact]
    public void Collection_over_limit_with_known_count_throws_early()
    {
        var mapper = CreateListMapper(maxCollectionSize: 5);
        var source = new EnumerableSource { Values = new List<int> { 1, 2, 3, 4, 5, 6 } };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<IntListDestination>(source));

        Assert.Equal("Values", exception.MemberPath);
        Assert.Equal(typeof(EnumerableSource), exception.SourceType);
        Assert.Equal(typeof(IntListDestination), exception.DestinationType);
        Assert.Contains("MaxCollectionSize", exception.Message);
        Assert.Contains("5", exception.Message);
        Assert.Contains("6", exception.Message);
        Assert.Contains("Values", exception.Message);
    }

    [Fact]
    public void Array_over_limit_with_known_count_throws_early()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<IntArraySource, IntArrayDestination>(),
            new MapperConfigurationOptions { MaxCollectionSize = 5 }).CreateMapper();

        var source = new IntArraySource { Values = new[] { 1, 2, 3, 4, 5, 6 } };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<IntArrayDestination>(source));

        Assert.Equal("Values", exception.MemberPath);
        Assert.Contains("MaxCollectionSize", exception.Message);
        Assert.Contains("6", exception.Message);
    }

    [Fact]
    public void Iterator_over_limit_throws_with_index()
    {
        var mapper = CreateListMapper(maxCollectionSize: 5);
        var source = new EnumerableSource { Values = Sequence(6) };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<IntListDestination>(source));

        Assert.Equal("Values[5]", exception.MemberPath);
        Assert.Contains("MaxCollectionSize", exception.Message);
        Assert.Contains("5", exception.Message);
        Assert.Contains("Values[5]", exception.Message);
    }

    [Fact]
    public void Array_destination_from_iterator_over_limit_throws_with_index()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<EnumerableSource, IntArrayDestination>(),
            new MapperConfigurationOptions { MaxCollectionSize = 5 }).CreateMapper();

        var source = new EnumerableSource { Values = Sequence(6) };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<IntArrayDestination>(source));

        Assert.Equal("Values[5]", exception.MemberPath);
        Assert.Contains("MaxCollectionSize", exception.Message);
    }

    [Fact]
    public void Lazy_iterator_at_limit_maps_all_items()
    {
        var mapper = CreateListMapper(maxCollectionSize: 5);
        var source = new EnumerableSource { Values = Sequence(5) };

        var destination = mapper.Map<IntListDestination>(source);

        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, destination.Values);
    }

    [Fact]
    public void Limit_can_be_disabled_with_int_max_value()
    {
        var mapper = CreateListMapper(int.MaxValue);
        var source = new EnumerableSource
        {
            Values = new List<int>(Enumerable.Range(0, 10))
        };

        var destination = mapper.Map<IntListDestination>(source);

        Assert.Equal(10, destination.Values.Count);
    }

    [Fact]
    public void Nested_collections_each_under_limit_pass()
    {
        var mapper = CreateNestedMapper(maxCollectionSize: 3);
        var source = new CollectionParentSource
        {
            Parents =
            {
                new CollectionChildSource { Children = new List<int> { 1, 2 } },
                new CollectionChildSource { Children = new List<int> { 3, 4 } }
            }
        };

        var destination = mapper.Map<CollectionParentDestination>(source);

        Assert.Equal(2, destination.Parents.Count);
        Assert.Equal(new[] { 1, 2 }, destination.Parents[0].Children);
        Assert.Equal(new[] { 3, 4 }, destination.Parents[1].Children);
    }

    [Fact]
    public void Nested_inner_collection_over_limit_uses_nested_path()
    {
        var mapper = CreateNestedMapper(maxCollectionSize: 3);
        var source = new CollectionParentSource
        {
            Parents =
            {
                new CollectionChildSource { Children = new List<int> { 1, 2, 3, 4 } }
            }
        };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<CollectionParentDestination>(source));

        Assert.Equal("Parents[0].Children", exception.MemberPath);
        Assert.Equal(typeof(CollectionChildSource), exception.SourceType);
        Assert.Equal(typeof(CollectionChildDestination), exception.DestinationType);
        Assert.Contains("MaxCollectionSize", exception.Message);
        Assert.Contains("Parents[0].Children", exception.Message);
    }

    [Fact]
    public void Collection_limit_is_captured_in_configuration_snapshot()
    {
        var options = new MapperConfigurationOptions { MaxCollectionSize = 5 };
        var configuration = new MapperConfiguration(
            cfg => cfg.CreateMap<EnumerableSource, IntListDestination>(),
            options);

        options.MaxCollectionSize = 100;
        var mapper = configuration.CreateMapper();

        var source = new EnumerableSource
        {
            Values = new List<int>(Enumerable.Range(0, 6))
        };

        Assert.Throws<MappingException>(() => mapper.Map<IntListDestination>(source));
    }

    [Fact]
    public void Concurrent_mapping_respects_collection_limit()
    {
        var mapper = CreateListMapper(maxCollectionSize: 5);
        var resultSizes = new System.Collections.Concurrent.ConcurrentBag<int>();

        Parallel.For(0, 32, index =>
        {
            var source = new EnumerableSource
            {
                Values = new List<int> { index, index + 1, index + 2, index + 3, index + 4 }
            };

            resultSizes.Add(mapper.Map<IntListDestination>(source).Values.Count);
        });

        Assert.Equal(32, resultSizes.Count);
        Assert.All(resultSizes, size => Assert.Equal(5, size));
    }

    [Fact]
    public void Dispose_exception_is_wrapped_when_no_prior_error()
    {
        var mapper = CreateListMapper(maxCollectionSize: 100);
        var source = new EnumerableSource
        {
            Values = new DisposeThrowingEnumerable(throwDuringEnumeration: false)
        };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<IntListDestination>(source));

        Assert.Equal("Values", exception.MemberPath);
        Assert.Equal(typeof(EnumerableSource), exception.SourceType);
        Assert.Equal(typeof(IntListDestination), exception.DestinationType);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("dispose", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Prior_mapping_error_is_not_hidden_by_dispose_error()
    {
        var mapper = CreateListMapper(maxCollectionSize: 100);
        var source = new EnumerableSource
        {
            Values = new DisposeThrowingEnumerable(throwDuringEnumeration: true)
        };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<IntListDestination>(source));

        Assert.Equal("Values[1]", exception.MemberPath);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Equal("move next boom", exception.InnerException!.Message);
        Assert.True(exception.Data.Contains("CollectionDisposeException"));

        var disposeException = Assert.IsType<InvalidOperationException>(
            exception.Data["CollectionDisposeException"]);
        Assert.Equal("dispose boom", disposeException.Message);
    }

    [Fact]
    public void Fatal_dispose_exception_is_not_wrapped()
    {
        var mapper = CreateListMapper(maxCollectionSize: 100);
        var source = new EnumerableSource { Values = new FatalDisposeEnumerable() };

        Assert.Throws<InvalidProgramException>(
            () => mapper.Map<IntListDestination>(source));
    }

    [Fact]
    public async Task Infinite_enumerable_is_stopped_in_an_isolated_process()
    {
        using var process = StartProbe("--infinite-collection-probe");
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();

        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail("Infinite collection probe did not terminate within two minutes.");
        }

        var output = await standardOutput;
        var error = await standardError;

        Assert.Equal(0, process.ExitCode);
        Assert.Contains("completed successfully", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Stack overflow", output + error, StringComparison.OrdinalIgnoreCase);
    }

    private static IMapper CreateListMapper(int maxCollectionSize)
    {
        return new MapperConfiguration(
            cfg => cfg.CreateMap<EnumerableSource, IntListDestination>(),
            new MapperConfigurationOptions { MaxCollectionSize = maxCollectionSize })
            .CreateMapper();
    }

    private static IMapper CreateNestedMapper(int maxCollectionSize)
    {
        return new MapperConfiguration(
            cfg =>
            {
                cfg.CreateMap<CollectionParentSource, CollectionParentDestination>();
                cfg.CreateMap<CollectionChildSource, CollectionChildDestination>();
            },
            new MapperConfigurationOptions { MaxCollectionSize = maxCollectionSize })
            .CreateMapper();
    }

    private static IEnumerable<int> Sequence(int count)
    {
        for (var index = 0; index < count; index++)
        {
            yield return index;
        }
    }

    private static Process StartProbe(string argument)
    {
        var host = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
        if (string.IsNullOrWhiteSpace(host))
        {
            host = "dotnet";
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = host,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(typeof(Program).Assembly.Location);
        startInfo.ArgumentList.Add(argument);

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start the collection safety probe.");
    }
}
