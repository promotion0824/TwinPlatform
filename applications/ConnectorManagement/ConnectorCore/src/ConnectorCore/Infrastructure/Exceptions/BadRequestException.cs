namespace Willow.Infrastructure.Exceptions;

using System;
using System.Net;

internal class BadRequestException : ApiException
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
        : base(HttpStatusCode.BadRequest, $"Bad request. {message}", null, innerException)
    {
    }
}
