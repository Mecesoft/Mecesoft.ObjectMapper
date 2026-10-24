namespace Mecesoft.ObjectMapper;

internal static class ExceptionHelper
{
    public static bool IsFatal(Exception exception)
    {
        return exception is OutOfMemoryException
            or StackOverflowException
            or AccessViolationException
            or AppDomainUnloadedException
            or BadImageFormatException
            or CannotUnloadAppDomainException
            or InvalidProgramException;
    }

    public static bool ShouldWrap(Exception exception)
    {
        return !IsFatal(exception) &&
               exception is not MappingException &&
               exception is not OperationCanceledException;
    }
}
