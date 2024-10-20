namespace Willow.Infrastructure.Exceptions;

using System;
using System.Net;

internal class DependencyServiceFailureException : ApiException
{
    public DependencyServiceFailureException(string serviceName, HttpStatusCode serviceStatusCode)
        : this(serviceName, serviceStatusCode, string.Empty, null)
    {
    }

    public DependencyServiceFailureException(string serviceName, HttpStatusCode serviceStatusCode, string message)
        : this(serviceName, serviceStatusCode, message, null)
    {
    }

    public DependencyServiceFailureException(string serviceName, HttpStatusCode serviceStatusCode, Exception innerException)
        : this(serviceName, serviceStatusCode, string.Empty, innerException)
    {
    }

    public DependencyServiceFailureException(string serviceName, HttpStatusCode serviceStatusCode, string message, Exception innerException)
        : base(serviceStatusCode, $"Service {serviceName} returns failure ({serviceStatusCode}). {message}", new { ServiceName = serviceName }, innerException)
    {
        ServiceName = serviceName;
        ServiceStatusCode = serviceStatusCode;
    }

    public string ServiceName { get; set; }

    public HttpStatusCode ServiceStatusCode { get; set; }
}
