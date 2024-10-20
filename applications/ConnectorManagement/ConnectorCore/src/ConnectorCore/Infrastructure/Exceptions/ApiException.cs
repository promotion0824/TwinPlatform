namespace Willow.Infrastructure.Exceptions;

using System;
using System.Net;

internal class ApiException : Exception
{
    protected ApiException(HttpStatusCode statusCode, string message, object additionalData, Exception innerException)
    : base(message, innerException)
    {
        StatusCode = statusCode;
        AdditionalData = additionalData;
    }

    public HttpStatusCode StatusCode { get; set; }

    public object AdditionalData { get; set; }
}
