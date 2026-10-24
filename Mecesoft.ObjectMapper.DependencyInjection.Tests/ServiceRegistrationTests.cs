using System.Collections.Concurrent;
using Mecesoft.ObjectMapper;
using Mecesoft.ObjectMapper.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Mecesoft.ObjectMapper.DependencyInjection.Tests;

public sealed class ServiceRegistrationTests
{
    [Fact]
    public void Resolves_mapper_and_maps_entities()
    {
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(cfg => cfg.CreateMap<Customer, CustomerDto>());

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        var destination = mapper.Map<CustomerDto>(new Customer { Name = "Ada" });

        Assert.Equal("Ada", destination.Name);
    }

    [Fact]
    public void Mapper_is_singleton_and_configuration_runs_once()
    {
        var configureCount = 0;
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(cfg =>
        {
            configureCount++;
            cfg.CreateMap<Customer, CustomerDto>();
        });

        using var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<IMapper>();
        var second = provider.GetRequiredService<IMapper>();

        Assert.Same(first, second);
        Assert.Equal(1, configureCount);
    }

    [Fact]
    public void Parallel_resolution_uses_one_singleton()
    {
        var configureCount = 0;
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(cfg =>
        {
            Interlocked.Increment(ref configureCount);
            cfg.CreateMap<Customer, CustomerDto>();
        });

        using var provider = services.BuildServiceProvider();
        var mappers = new ConcurrentBag<IMapper>();

        Parallel.For(0, 64, _ => mappers.Add(provider.GetRequiredService<IMapper>()));

        Assert.Equal(1, configureCount);
        var expected = mappers.First();
        Assert.All(mappers, mapper => Assert.Same(expected, mapper));
    }

    [Fact]
    public void Explicit_profiles_are_registered()
    {
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(
            _ => { },
            options =>
            {
                options.AddProfile<CustomerProfile>();
                options.AddProfile(new OrderProfile());
            });

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        Assert.Equal(
            "Ada",
            mapper.Map<CustomerDto>(new Customer { Name = "Ada" }).Name);
        Assert.Equal(3, mapper.Map<OrderDto>(new Order { Id = 3 }).Id);
    }

    [Fact]
    public void Assembly_scanning_discovers_profiles_deterministically()
    {
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(
            _ => { },
            options => options.AddProfilesFromAssembly(typeof(CustomerProfile).Assembly));

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        Assert.Equal(
            "Ada",
            mapper.Map<CustomerDto>(new Customer { Name = "Ada" }).Name);
        Assert.Equal(4, mapper.Map<OrderDto>(new Order { Id = 4 }).Id);
        Assert.Equal(
            "value",
            mapper.Map<ExplicitDestination>(new ExplicitSource { Value = "value" }).Value);
    }

    [Fact]
    public void Duplicate_profile_maps_are_rejected()
    {
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(
            _ => { },
            options =>
            {
                options.AddProfile(new CustomerProfile());
                options.AddProfile(new CustomerProfile());
            });

        using var provider = services.BuildServiceProvider();

        Assert.Throws<MappingConfigurationException>(
            () => provider.GetRequiredService<IMapper>());
    }

    [Fact]
    public void Duplicate_scanned_profiles_are_rejected()
    {
        var assembly = typeof(CustomerProfile).Assembly;
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(
            _ => { },
            options =>
            {
                options.AddProfilesFromAssembly(assembly);
                options.AddProfilesFromAssembly(assembly);
            });

        using var provider = services.BuildServiceProvider();

        Assert.Throws<MappingConfigurationException>(
            () => provider.GetRequiredService<IMapper>());
    }

    [Fact]
    public void Validation_is_optional_and_can_be_enabled()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<MappingConfigurationException>(() =>
            services.AddMecesoftObjectMapper(
                cfg => cfg.CreateMap<InvalidSource, InvalidDestination>(),
                options => options.AssertConfigurationIsValid = true));

        Assert.Contains("Optional", exception.Message);
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IMapper));
    }

    [Fact]
    public void Valid_startup_validation_builds_configuration_once()
    {
        var configureCount = 0;
        var services = new ServiceCollection();

        services.AddMecesoftObjectMapper(
            cfg =>
            {
                Interlocked.Increment(ref configureCount);
                cfg.CreateMap<Customer, CustomerDto>();
            },
            options => options.AssertConfigurationIsValid = true);

        Assert.Equal(1, configureCount);

        using var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<IMapper>();
        var second = provider.GetRequiredService<IMapper>();

        Assert.Same(first, second);
        Assert.Equal(1, configureCount);
        Assert.Equal(
            "Ada",
            first.Map<CustomerDto>(new Customer { Name = "Ada" }).Name);
    }

    [Fact]
    public void Startup_validation_supports_ReverseMap()
    {
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(
            cfg => cfg.CreateMap<Customer, CustomerDto>().ReverseMap(),
            options => options.AssertConfigurationIsValid = true);

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        Assert.Equal(
            "Ada",
            mapper.Map<CustomerDto>(new Customer { Name = "Ada" }).Name);
        Assert.Equal(
            "Grace",
            mapper.Map<Customer>(new CustomerDto { Name = "Grace" }).Name);
    }

    [Fact]
    public void Startup_validation_reports_missing_reverse_nested_map()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<MappingConfigurationException>(() =>
            services.AddMecesoftObjectMapper(
                cfg =>
                {
                    cfg.CreateMap<StartupChildSource, StartupChildDestination>();
                    cfg.CreateMap<StartupParentSource, StartupParentDestination>().ReverseMap();
                },
                options => options.AssertConfigurationIsValid = true));

        Assert.Contains("Child", exception.Message);
        Assert.Contains("(reverse)", exception.Message);
        Assert.Contains(nameof(StartupChildDestination), exception.Message);
        Assert.Contains(nameof(StartupChildSource), exception.Message);
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IMapper));
    }

    [Fact]
    public void Startup_validation_builds_explicit_profiles()
    {
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(
            _ => { },
            options =>
            {
                options.AddProfile<CustomerProfile>();
                options.AddProfile<OrderProfile>();
                options.AssertConfigurationIsValid = true;
            });

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        Assert.Equal(
            "Ada",
            mapper.Map<CustomerDto>(new Customer { Name = "Ada" }).Name);
        Assert.Equal(3, mapper.Map<OrderDto>(new Order { Id = 3 }).Id);
    }

    [Fact]
    public void Failed_startup_validation_can_be_retried_with_valid_configuration()
    {
        var services = new ServiceCollection();

        Assert.Throws<MappingConfigurationException>(() =>
            services.AddMecesoftObjectMapper(
                cfg => cfg.CreateMap<InvalidSource, InvalidDestination>(),
                options => options.AssertConfigurationIsValid = true));

        services.AddMecesoftObjectMapper(
            cfg => cfg.CreateMap<Customer, CustomerDto>(),
            options => options.AssertConfigurationIsValid = true);

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        Assert.Equal(
            "Ada",
            mapper.Map<CustomerDto>(new Customer { Name = "Ada" }).Name);
    }

    [Fact]
    public void Validation_is_off_by_default()
    {
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(
            cfg => cfg.CreateMap<InvalidSource, InvalidDestination>());

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        var destination = mapper.Map<InvalidDestination>(
            new InvalidSource { Name = "Ada" });

        Assert.Equal("Ada", destination.Name);
        Assert.Equal("default", destination.Optional);
    }

    [Fact]
    public void Mapper_options_are_forwarded_and_snapshotted()
    {
        var options = new MapperConfigurationOptions { MaxCollectionSize = 1 };
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(
            cfg => cfg.CreateMap<ListSource, ListDestination>(),
            options);

        options.MaxCollectionSize = 100;

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        var source = new ListSource { Values = new List<int> { 1, 2 } };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<ListDestination>(source));

        Assert.Contains("MaxCollectionSize", exception.Message);
    }

    [Fact]
    public void Max_depth_option_is_forwarded()
    {
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(
            cfg => cfg.CreateMap<DeepNodeSource, DeepNodeDto>(),
            new MapperConfigurationOptions { MaxDepth = 2 });

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        var source = new DeepNodeSource { Name = "0" };
        source.Next = new DeepNodeSource { Name = "1" };
        source.Next.Next = new DeepNodeSource { Name = "2" };
        source.Next.Next.Next = new DeepNodeSource { Name = "3" };

        Assert.Throws<MappingException>(() => mapper.Map<DeepNodeDto>(source));
    }

    [Fact]
    public void Duplicate_registration_throws_and_keeps_the_first_registration()
    {
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(cfg => cfg.CreateMap<Customer, CustomerDto>());

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddMecesoftObjectMapper(cfg => cfg.CreateMap<Order, OrderDto>()));

        Assert.Contains("already been registered", exception.Message);

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        Assert.Equal(
            "Ada",
            mapper.Map<CustomerDto>(new Customer { Name = "Ada" }).Name);
        Assert.Throws<InvalidOperationException>(
            () => mapper.Map<OrderDto>(new Order()));
    }

    [Fact]
    public void Existing_mapper_registration_is_not_overridden()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMapper>(new FakeMapper());

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddMecesoftObjectMapper(cfg => cfg.CreateMap<Customer, CustomerDto>()));

        Assert.Contains("IMapper", exception.Message);

        using var provider = services.BuildServiceProvider();
        Assert.IsType<FakeMapper>(provider.GetRequiredService<IMapper>());
    }

    [Fact]
    public void ReverseMap_is_available_through_dependency_injection()
    {
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(cfg =>
            cfg.CreateMap<Customer, CustomerDto>().ReverseMap());

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        Assert.Equal(
            "Ada",
            mapper.Map<CustomerDto>(new Customer { Name = "Ada" }).Name);
        Assert.Equal(
            "Grace",
            mapper.Map<Customer>(new CustomerDto { Name = "Grace" }).Name);
    }

    [Fact]
    public void Mapper_can_be_injected_into_scoped_and_transient_services()
    {
        var services = new ServiceCollection();
        services.AddMecesoftObjectMapper(cfg => cfg.CreateMap<Customer, CustomerDto>());
        services.AddScoped<ScopedService>();
        services.AddTransient<TransientService>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        using var scope = provider.CreateScope();
        var scoped = scope.ServiceProvider.GetRequiredService<ScopedService>();
        var transient = scope.ServiceProvider.GetRequiredService<TransientService>();

        Assert.Equal("Ada", scoped.Map(new Customer { Name = "Ada" }).Name);
        Assert.Equal("Grace", transient.Map(new Customer { Name = "Grace" }).Name);
        Assert.Same(scoped.Mapper, transient.Mapper);
        Assert.Same(provider.GetRequiredService<IMapper>(), scoped.Mapper);
    }
}
