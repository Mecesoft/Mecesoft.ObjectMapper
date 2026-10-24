namespace Mecesoft.ObjectMapper;

/// <summary>
/// Thrown when a configured mapping cannot be applied to a member value.
/// </summary>
public sealed class MappingException : Exception
{
    /// <summary>
    /// Initializes a new mapping exception.
    /// </summary>
    /// <param name="sourceType">The source type that was being mapped.</param>
    /// <param name="destinationType">The destination type that was being mapped.</param>
    /// <param name="memberPath">The destination member path where mapping failed.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The underlying exception, if any.</param>
    public MappingException(
        Type sourceType,
        Type destinationType,
        string memberPath,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        MemberPath = memberPath;
    }

    /// <summary>
    /// Gets the type that was being mapped from.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// Gets the type that was being mapped to.
    /// </summary>
    public Type DestinationType { get; }

    /// <summary>
    /// Gets the full destination member path, for example <c>Address.City</c>.
    /// </summary>
    public string MemberPath { get; }
}
