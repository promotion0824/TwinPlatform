namespace Willow.IoTService.WebApiErrorHandling.Contracts;

public class RequestBaseException : Exception
{
    protected RequestBaseException(int statusCode)
    {
        StatusCode = statusCode;
    }

    protected RequestBaseException(string? message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    protected RequestBaseException(string? message,
                                   Exception? innerException,
                                   int statusCode) : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
