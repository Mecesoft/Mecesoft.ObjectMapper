using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Mecesoft.ObjectMapper;

namespace Mecesoft.ObjectMapper.DependencyInjection;

/// <summary>
/// Optional configuration for the dependency-injection registration.
/// </summary>
public sealed class MecesoftObjectMapperRegistrationOptions
{
    private readonly List<Profile> _profiles = new();

    /// <summary>
    /// Gets the core mapper options used for the singleton mapper.
    /// </summary>
    public MapperConfigurationOptions Configuration { get; } = new();

    /// <summary>
    /// Gets or sets whether the configuration is built and validated during registration.
    /// When enabled, invalid configuration throws from <c>AddMecesoftObjectMapper</c>
    /// instead of on the first <see cref="IMapper"/> resolution.
    /// </summary>
    public bool AssertConfigurationIsValid { get; set; }

    internal IReadOnlyList<Profile> Profiles => _profiles;

    /// <summary>
    /// Adds an explicit profile instance.
    /// </summary>
    public MecesoftObjectMapperRegistrationOptions AddProfile(Profile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        _profiles.Add(profile);
        return this;
    }

    /// <summary>
    /// Creates and adds an explicit profile.
    /// </summary>
    public MecesoftObjectMapperRegistrationOptions AddProfile<TProfile>()
        where TProfile : Profile, new()
    {
        _profiles.Add(new TProfile());
        return this;
    }

    /// <summary>
    /// Adds all concrete profile types with a public parameterless constructor from an
    /// assembly. Types are ordered by full name using ordinal comparison.
    /// </summary>
    /// <remarks>
    /// Assembly scanning uses reflection and is not NativeAOT/trimming safe. Prefer explicit
    /// profile registration when publishing with trimming or NativeAOT.
    /// </remarks>
    [RequiresUnreferencedCode(
        "Assembly scanning uses reflection to discover and construct Profile types.")]
    public MecesoftObjectMapperRegistrationOptions AddProfilesFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        Type[] types;

        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            throw new InvalidOperationException(
                $"Failed to scan assembly '{assembly.FullName}' for mapper profiles.",
                exception);
        }

        var profileTypes = types
            .Where(type =>
                typeof(Profile).IsAssignableFrom(type) &&
                !type.IsAbstract &&
                !type.IsGenericTypeDefinition)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        var profiles = new List<Profile>(profileTypes.Length);

        foreach (var profileType in profileTypes)
        {
            if (profileType.GetConstructor(Type.EmptyTypes) is null)
            {
                throw new InvalidOperationException(
                    $"Profile type '{profileType.FullName}' must have a public parameterless constructor.");
            }

            if (Activator.CreateInstance(profileType) is not Profile profile)
            {
                throw new InvalidOperationException(
                    $"Could not create profile type '{profileType.FullName}'.");
            }

            profiles.Add(profile);
        }

        _profiles.AddRange(profiles);
        return this;
    }
}
