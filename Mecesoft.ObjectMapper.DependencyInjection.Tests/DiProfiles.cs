namespace Mecesoft.ObjectMapper.DependencyInjection.Tests;

public sealed class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>();
    }
}

public sealed class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderDto>();
    }
}

public sealed class ExplicitProfile : Profile
{
    public ExplicitProfile()
    {
        CreateMap<ExplicitSource, ExplicitDestination>();
    }
}
