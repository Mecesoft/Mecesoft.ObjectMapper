using Mecesoft.ObjectMapper;
using Mecesoft.ObjectMapper.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency-injection extensions for Mecesoft.ObjectMapper.
/// </summary>
public static class MecesoftObjectMapperServiceCollectionExtensions
{
    /// <summary>
    /// Registers a singleton <see cref="IMapper"/> using the supplied configuration.
    /// </summary>
    public static IServiceCollection AddMecesoftObjectMapper(
        this IServiceCollection services,
        Action<IMapperConfigurationExpression> configure)
    {
        return AddMecesoftObjectMapper(services, configure, static _ => { });
    }

    /// <summary>
    /// Registers a singleton <see cref="IMapper"/> using the supplied configuration and
    /// mapper options. The options are captured when registration occurs.
    /// </summary>
    public static IServiceCollection AddMecesoftObjectMapper(
        this IServiceCollection services,
        Action<IMapperConfigurationExpression> configure,
        MapperConfigurationOptions configurationOptions)
    {
        ArgumentNullException.ThrowIfNull(configurationOptions);

        return AddMecesoftObjectMapper(
            services,
            configure,
            options =>
            {
                options.Configuration.MaxDepth = configurationOptions.MaxDepth;
                options.Configuration.MaxCollectionSize = configurationOptions.MaxCollectionSize;
            });
    }

    /// <summary>
    /// Registers a singleton <see cref="IMapper"/> using the supplied configuration and
    /// optional registration settings such as profiles, validation and mapper options.
    /// </summary>
    public static IServiceCollection AddMecesoftObjectMapper(
        this IServiceCollection services,
        Action<IMapperConfigurationExpression> configure,
        Action<MecesoftObjectMapperRegistrationOptions> configureRegistration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        ArgumentNullException.ThrowIfNull(configureRegistration);

        if (services.Any(descriptor =>
                descriptor.ServiceType == typeof(MecesoftObjectMapperRegistrationMarker)))
        {
            throw new InvalidOperationException(
                "Mecesoft.ObjectMapper has already been registered on this service collection. " +
                "AddMecesoftObjectMapper does not override an existing registration.");
        }

        if (services.Any(descriptor => descriptor.ServiceType == typeof(IMapper)))
        {
            throw new InvalidOperationException(
                "An IMapper registration already exists on this service collection. " +
                "Mecesoft.ObjectMapper will not override it.");
        }

        var registrationOptions = new MecesoftObjectMapperRegistrationOptions();
        configureRegistration(registrationOptions);

        var configurationOptions = new MapperConfigurationOptions
        {
            MaxDepth = registrationOptions.Configuration.MaxDepth,
            MaxCollectionSize = registrationOptions.Configuration.MaxCollectionSize
        };

        var profiles = registrationOptions.Profiles.ToArray();
        var assertConfigurationIsValid = registrationOptions.AssertConfigurationIsValid;

        if (assertConfigurationIsValid)
        {
            // Validate before mutating the service collection so a failed startup check
            // leaves the collection unchanged.
            var configuration = BuildConfiguration(configure, profiles, configurationOptions);
            configuration.AssertConfigurationIsValid();
            var mapper = configuration.CreateMapper();

            services.AddSingleton(MecesoftObjectMapperRegistrationMarker.Instance);
            services.AddSingleton<IMapper>(mapper);
            return services;
        }

        services.AddSingleton(MecesoftObjectMapperRegistrationMarker.Instance);
        services.AddSingleton<IMapper>(_ =>
            BuildConfiguration(configure, profiles, configurationOptions).CreateMapper());

        return services;
    }

    private static MapperConfiguration BuildConfiguration(
        Action<IMapperConfigurationExpression> configure,
        IReadOnlyList<Profile> profiles,
        MapperConfigurationOptions configurationOptions)
    {
        return new MapperConfiguration(
            cfg =>
            {
                configure(cfg);

                foreach (var profile in profiles)
                {
                    cfg.AddProfile(profile);
                }
            },
            configurationOptions);
    }

    private sealed class MecesoftObjectMapperRegistrationMarker
    {
        public static readonly MecesoftObjectMapperRegistrationMarker Instance = new();

        private MecesoftObjectMapperRegistrationMarker()
        {
        }
    }
}
