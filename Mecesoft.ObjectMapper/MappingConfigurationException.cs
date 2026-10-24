namespace Mecesoft.ObjectMapper;

/// <summary>
/// Thrown when a mapping configuration is invalid.
/// </summary>
public sealed class MappingConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new exception with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public MappingConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new exception with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this configuration error.</param>
    public MappingConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
