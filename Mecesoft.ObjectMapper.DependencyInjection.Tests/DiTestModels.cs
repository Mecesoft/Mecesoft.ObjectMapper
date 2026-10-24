namespace Mecesoft.ObjectMapper.DependencyInjection.Tests;

public sealed class Customer
{
    public string Name { get; set; } = string.Empty;
}

public sealed class CustomerDto
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class Order
{
    public int Id { get; set; }
}

internal sealed class OrderDto
{
    public int Id { get; set; }
}

internal sealed class ExplicitSource
{
    public string Value { get; set; } = string.Empty;
}

internal sealed class ExplicitDestination
{
    public string Value { get; set; } = string.Empty;
}

internal sealed class InvalidSource
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class InvalidDestination
{
    public string Name { get; set; } = string.Empty;
    public string Optional { get; set; } = "default";
}

internal sealed class DeepNodeSource
{
    public string Name { get; set; } = string.Empty;
    public DeepNodeSource? Next { get; set; }
}

internal sealed class DeepNodeDto
{
    public string Name { get; set; } = string.Empty;
    public DeepNodeDto? Next { get; set; }
}

internal sealed class ListSource
{
    public List<int> Values { get; set; } = new();
}

internal sealed class ListDestination
{
    public List<int> Values { get; set; } = new();
}

internal sealed class FakeMapper : IMapper
{
    public TDestination Map<TDestination>(object source)
    {
        throw new NotSupportedException();
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        throw new NotSupportedException();
    }

    public object Map(object source, Type destinationType)
    {
        throw new NotSupportedException();
    }
}

public sealed class ScopedService
{
    public ScopedService(IMapper mapper)
    {
        Mapper = mapper;
    }

    public IMapper Mapper { get; }

    public CustomerDto Map(Customer customer)
    {
        return Mapper.Map<CustomerDto>(customer);
    }
}

public sealed class TransientService
{
    public TransientService(IMapper mapper)
    {
        Mapper = mapper;
    }

    public IMapper Mapper { get; }

    public CustomerDto Map(Customer customer)
    {
        return Mapper.Map<CustomerDto>(customer);
    }
}

internal sealed class StartupParentSource
{
    public StartupChildSource? Child { get; set; }
}

internal sealed class StartupParentDestination
{
    public StartupChildDestination? Child { get; set; }
}

internal sealed class StartupChildSource
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class StartupChildDestination
{
    public string Name { get; set; } = string.Empty;
}
