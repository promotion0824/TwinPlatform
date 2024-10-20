using System.Net;

namespace Willow.IoTService.WebApiErrorHandling.Contracts;

public class ConflictException : RequestBaseException
{
    public ConflictException() : base((int)HttpStatusCode.Conflict)
    {
    }

    public ConflictException(string? message) : base(message, (int)HttpStatusCode.Conflict)
    {
    }

    public ConflictException(string? message, Exception? innerException) : base(message,
                                                                                innerException,
                                                                                (int)HttpStatusCode.Conflict)
    {
    }
}
