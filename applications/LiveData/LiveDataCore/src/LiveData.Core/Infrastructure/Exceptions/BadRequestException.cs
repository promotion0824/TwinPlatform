namespace Willow.Infrastructure.Exceptions;

internal class BadRequestException : Exception
{
    public BadRequestException(string message)
        : this(message, null)
    {
    }

    public BadRequestException(Exception innerException)
        : this(string.Empty, innerException)
    {
    }

    public BadRequestException(string message, Exception innerException)
        : base($"Bad request. {message}", innerException)
    {
    }
}
