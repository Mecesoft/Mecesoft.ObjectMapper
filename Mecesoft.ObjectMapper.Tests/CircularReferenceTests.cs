using System.Diagnostics;

namespace Mecesoft.ObjectMapper.Tests;

public sealed class CircularReferenceTests
{
    private static IMapper CreateMapper()
    {
        return new MapperConfiguration(cfg =>
            cfg.CreateMap<CircularNode, CircularNodeDto>()).CreateMapper();
    }

    [Fact]
    public void Self_reference_is_preserved()
    {
        var node = new CircularNode { Name = "self" };
        node.Next = node;

        var destination = CreateMapper().Map<CircularNodeDto>(node);

        Assert.Equal("self", destination.Name);
        Assert.Same(destination, destination.Next);
    }

    [Fact]
    public void Two_node_cycle_is_preserved()
    {
        var first = new CircularNode { Name = "first" };
        var second = new CircularNode { Name = "second", Next = first };
        first.Next = second;

        var destination = CreateMapper().Map<CircularNodeDto>(first);

        Assert.Equal("first", destination.Name);
        Assert.NotNull(destination.Next);
        Assert.Equal("second", destination.Next.Name);
        Assert.Same(destination, destination.Next.Next);
    }

    [Fact]
    public void Shared_reference_maps_to_one_destination_instance()
    {
        var shared = new SharedNode { Name = "shared" };
        var source = new HolderSource { Left = shared, Right = shared };

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<HolderSource, HolderDestination>();
            cfg.CreateMap<SharedNode, SharedNodeDto>();
        }).CreateMapper();

        var destination = mapper.Map<HolderDestination>(source);

        Assert.NotNull(destination.Left);
        Assert.Same(destination.Left, destination.Right);
    }

    [Fact]
    public void Reference_tracking_is_reset_between_top_level_maps()
    {
        var node = new CircularNode { Name = "self" };
        node.Next = node;
        var mapper = CreateMapper();

        var first = mapper.Map<CircularNodeDto>(node);
        var second = mapper.Map<CircularNodeDto>(node);

        Assert.NotSame(first, second);
        Assert.Same(first, first.Next);
        Assert.Same(second, second.Next);
        Assert.NotSame(first.Next, second.Next);
    }

    [Fact]
    public async Task Circular_graph_maps_without_stack_overflow_in_isolated_process()
    {
        using var process = StartProbe();
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
            Assert.Fail("Circular reference probe did not terminate within two minutes.");
        }

        var output = await standardOutput;
        var error = await standardError;

        Assert.Equal(0, process.ExitCode);
        Assert.Contains("completed successfully", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Stack overflow", output + error, StringComparison.OrdinalIgnoreCase);
    }

    private static Process StartProbe()
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
        startInfo.ArgumentList.Add("--circular-reference-probe");

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start the circular reference probe.");
    }
}
