using System.Linq.Expressions;

namespace Mecesoft.ObjectMapper;

internal interface ITypeMapRegistrar
{
    void RegisterTypeMap(TypeMap typeMap);
    void UnregisterTypeMap(TypeMap typeMap);
}

/// <summary>
/// Fluent configuration for a single source/destination map.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
public sealed class MappingExpression<TSource, TDestination>
{
    private readonly TypeMap _typeMap;
    private readonly ITypeMapRegistrar _registrar;
    private bool _canRollbackForwardMap;
    private bool _isRegistered;

    internal MappingExpression(
        TypeMap typeMap,
        ITypeMapRegistrar registrar,
        bool canRollbackForwardMap)
    {
        _typeMap = typeMap;
        _registrar = registrar;
        _canRollbackForwardMap = canRollbackForwardMap;
        _isRegistered = true;
    }

    /// <summary>
    /// Configures a destination member with a custom value or excludes it from mapping.
    /// </summary>
    /// <param name="destinationMember">A direct destination member expression.</param>
    /// <param name="memberOptions">The member options callback.</param>
    /// <returns>The same expression for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="destinationMember"/> or <paramref name="memberOptions"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="MappingConfigurationException">
    /// The expression targets a nested destination path, or this expression is no longer
    /// registered.
    /// </exception>
    public MappingExpression<TSource, TDestination> ForMember(
        Expression<Func<TDestination, object?>> destinationMember,
        Action<IMemberOptions<TSource>> memberOptions)
    {
        ArgumentNullException.ThrowIfNull(destinationMember);
        ArgumentNullException.ThrowIfNull(memberOptions);

        EnsureRegistered();

        var memberName = ExpressionHelper.GetMemberName(destinationMember);
        var options = new MemberOptions<TSource>();
        memberOptions(options);
        _typeMap.SetMemberMap(memberName, options.ToMemberMap());
        return this;
    }

    /// <summary>
    /// Excludes a destination member from conventional mapping.
    /// </summary>
    /// <param name="destinationMember">A direct destination member expression.</param>
    /// <returns>The same expression for chaining.</returns>
    /// <exception cref="MappingConfigurationException">
    /// The expression targets a nested destination path, or this expression is no longer
    /// registered.
    /// </exception>
    public MappingExpression<TSource, TDestination> Ignore(
        Expression<Func<TDestination, object?>> destinationMember)
    {
        return ForMember(destinationMember, options => options.Ignore());
    }

    /// <summary>
    /// Adds a conventional reverse map for the destination-to-source pair.
    /// Forward member rules such as <c>Ignore</c> and <c>MapFrom</c> are not inverted.
    /// Configure the returned expression if the reverse map needs custom rules.
    /// </summary>
    /// <remarks>
    /// Registration is atomic for the pair created by this expression. If the reverse map
    /// cannot be registered, the forward map created by this expression is rolled back and
    /// this expression can no longer be used for configuration.
    /// </remarks>
    /// <returns>The expression for the destination-to-source map.</returns>
    /// <exception cref="MappingConfigurationException">
    /// The reverse map already exists, or this expression has already been rolled back.
    /// </exception>
    public MappingExpression<TDestination, TSource> ReverseMap()
    {
        EnsureRegistered();

        var reverseTypeMap = new TypeMap(typeof(TDestination), typeof(TSource), isReverseMap: true);

        try
        {
            _registrar.RegisterTypeMap(reverseTypeMap);
        }
        catch
        {
            // A duplicate or failed registration must not leave a partially-added reverse map.
            _registrar.UnregisterTypeMap(reverseTypeMap);

            if (_canRollbackForwardMap)
            {
                _registrar.UnregisterTypeMap(_typeMap);
                _isRegistered = false;
                _canRollbackForwardMap = false;
            }

            throw;
        }

        _canRollbackForwardMap = false;
        return new MappingExpression<TDestination, TSource>(
            reverseTypeMap,
            _registrar,
            canRollbackForwardMap: false);
    }

    private void EnsureRegistered()
    {
        if (!_isRegistered)
        {
            throw new MappingConfigurationException(
                "This mapping expression is no longer registered because a previous " +
                "ReverseMap operation failed and rolled back the forward map.");
        }
    }
}
