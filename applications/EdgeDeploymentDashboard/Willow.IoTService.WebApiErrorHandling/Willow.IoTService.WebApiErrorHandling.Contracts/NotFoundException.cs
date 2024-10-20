using System.Net;

namespace Willow.IoTService.WebApiErrorHandling.Contracts;

public class NotFoundException : RequestBaseException
{
    public NotFoundException(string? message) : base(message, (int)HttpStatusCode.NotFound)
    {
    }

    public NotFoundException(string? message, Exception? innerException) : base(message,
                                                                                innerException,
                                                                                (int)HttpStatusCode.NotFound)
    {
    }
}
