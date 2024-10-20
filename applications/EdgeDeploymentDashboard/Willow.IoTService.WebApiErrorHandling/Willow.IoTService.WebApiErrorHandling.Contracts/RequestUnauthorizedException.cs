using System.Net;

namespace Willow.IoTService.WebApiErrorHandling.Contracts;

public class RequestUnauthorizedException : RequestBaseException
{
    public RequestUnauthorizedException() : base((int)HttpStatusCode.Unauthorized)
    {
    }

    public RequestUnauthorizedException(string? message) : base(message, (int)HttpStatusCode.Unauthorized)
    {
    }

    public RequestUnauthorizedException(string? message, Exception? innerException) : base(message,
                                                                                           innerException,
                                                                                           (int)HttpStatusCode.Unauthorized)
    {
    }
}
