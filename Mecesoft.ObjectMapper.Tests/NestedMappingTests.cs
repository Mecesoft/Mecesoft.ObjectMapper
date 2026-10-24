namespace Mecesoft.ObjectMapper.Tests;

public sealed class NestedMappingTests
{
    [Fact]
    public void Maps_nested_type_when_a_map_is_configured()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            cfg.CreateMap<Customer, CustomerDto>();
        }).CreateMapper();

        var source = new Order
        {
            Id = 3,
            Customer = new Customer { Name = "Ada" }
        };

        var destination = mapper.Map<OrderDto>(source);

        Assert.Equal(3, destination.Id);
        Assert.NotNull(destination.Customer);
        Assert.Equal("Ada", destination.Customer.Name);
    }

    [Fact]
    public void Throws_when_nested_type_has_no_map()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<Order, OrderDto>()).CreateMapper();

        var exception = Assert.Throws<MappingException>(() =>
            mapper.Map<OrderDto>(new Order
            {
                Customer = new Customer { Name = "Ada" }
            }));

        Assert.Equal("Customer", exception.MemberPath);
        Assert.Equal(typeof(Order), exception.SourceType);
        Assert.Equal(typeof(OrderDto), exception.DestinationType);
        Assert.Contains("Customer", exception.Message);
        Assert.Contains(nameof(CustomerDto), exception.Message);
    }

    [Fact]
    public void Maps_null_nested_source_to_null()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            cfg.CreateMap<Customer, CustomerDto>();
        }).CreateMapper();

        var destination = mapper.Map<OrderDto>(new Order { Customer = null! });

        Assert.Null(destination.Customer);
    }
}
