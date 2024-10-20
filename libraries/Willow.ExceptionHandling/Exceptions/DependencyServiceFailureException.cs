using System.Net;
using System.Runtime.Serialization;

namespace Willow.ExceptionHandling.Exceptions;

[Serializable]
public class DependencyServiceFailureException : Exception
{
    public string ServiceName { get; set; }
    public HttpStatusCode ServiceStatusCode { get; set; }

    public DependencyServiceFailureException(string serviceName, HttpStatusCode serviceStatusCode)
        : this(serviceName, serviceStatusCode, string.Empty)
    {
    }

    public DependencyServiceFailureException(
        string serviceName,
        HttpStatusCode serviceStatusCode,
        Exception innerException)
        : this(serviceName, serviceStatusCode, string.Empty, innerException)
    {
    }

    public DependencyServiceFailureException(
        string serviceName,
        HttpStatusCode serviceStatusCode,
        string? message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ServiceName = serviceName;
        ServiceStatusCode = serviceStatusCode;
    }

    protected DependencyServiceFailureException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        ServiceName = info.GetString(nameof(ServiceName)) ?? "";
        ServiceStatusCode = (HttpStatusCode)info.GetInt32(nameof(ServiceStatusCode));
    }
}
