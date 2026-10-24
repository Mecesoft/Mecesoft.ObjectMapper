using System.Collections.Concurrent;

namespace Mecesoft.ObjectMapper.Tests;

public sealed class MappingPlanCacheTests
{
    [Fact]
    public void Existing_mapper_uses_configuration_snapshot()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<ProfileSource, ProfileDestination>());
        var mapper = configuration.CreateMapper();

        configuration.CreateMap<BasicSource, BasicDestination>();

        Assert.Throws<InvalidOperationException>(
            () => mapper.Map<BasicDestination>(new BasicSource()));

        var newMapper = configuration.CreateMapper();
        var destination = newMapper.Map<BasicDestination>(new BasicSource { Id = 5 });

        Assert.Equal(5, destination.Id);
    }

    [Fact]
    public void Late_ForMember_does_not_change_existing_mapper_plan()
    {
        var configuration = new MapperConfiguration(_ => { });
        var expression = configuration.CreateMap<BasicSource, BasicDestination>();
        var mapper = configuration.CreateMapper();

        expression.ForMember(
            dest => dest.FullName,
            options => options.MapFrom(_ => "late"));

        var existingDestination = mapper.Map<BasicDestination>(new BasicSource());
        Assert.Equal(string.Empty, existingDestination.FullName);

        var newMapper = configuration.CreateMapper();
        var updatedDestination = newMapper.Map<BasicDestination>(new BasicSource());
        Assert.Equal("late", updatedDestination.FullName);
    }

    [Fact]
    public void Concurrent_first_mapping_is_thread_safe()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BasicSource, BasicDestination>();
            cfg.CreateMap<Address, AddressDto>();
        }).CreateMapper();

        var ids = new ConcurrentBag<int>();

        Parallel.For(0, 64, index =>
        {
            var source = new BasicSource
            {
                Id = index,
                Address = new Address { City = $"City-{index}" }
            };

            ids.Add(mapper.Map<BasicDestination>(source).Id);
        });

        Assert.Equal(64, ids.Count);
        Assert.Equal(64, ids.Distinct().Count());
    }

    [Fact]
    public void Different_mappers_do_not_share_mapping_plans()
    {
        var firstMapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<BasicSource, BasicDestination>()
                .ForMember(dest => dest.FullName, options => options.MapFrom(_ => "first")))
            .CreateMapper();

        var secondMapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<BasicSource, BasicDestination>()
                .ForMember(dest => dest.FullName, options => options.MapFrom(_ => "second")))
            .CreateMapper();

        Assert.Equal(
            "first",
            firstMapper.Map<BasicDestination>(new BasicSource()).FullName);
        Assert.Equal(
            "second",
            secondMapper.Map<BasicDestination>(new BasicSource()).FullName);
    }
}
