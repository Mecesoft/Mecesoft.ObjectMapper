using BenchmarkDotNet.Attributes;

namespace Mecesoft.ObjectMapper.Benchmarks;

public class CircularMappingBenchmarks
{
    private IMapper _warmMapper = null!;
    private NodeSource _source = null!;

    [GlobalSetup]
    public void Setup()
    {
        _warmMapper = new MapperConfiguration(cfg => cfg.CreateMap<NodeSource, NodeDto>()).CreateMapper();

        var first = new NodeSource { Name = "first" };
        var second = new NodeSource { Name = "second", Next = first };
        first.Next = second;
        _source = first;
    }

    [Benchmark(Baseline = true)]
    public NodeDto Manual_Circular()
    {
        var first = new NodeDto { Name = _source.Name };
        var second = new NodeDto { Name = _source.Next!.Name };
        first.Next = second;
        second.Next = first;
        return first;
    }

    [Benchmark]
    public NodeDto Warm_Circular()
    {
        return _warmMapper.Map<NodeDto>(_source);
    }

    [Benchmark]
    public NodeDto Cold_Circular()
    {
        var mapper = new MapperConfiguration(cfg => cfg.CreateMap<NodeSource, NodeDto>()).CreateMapper();
        return mapper.Map<NodeDto>(_source);
    }
}
