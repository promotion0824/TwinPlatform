using System.Runtime.Serialization;

namespace Willow.Exceptions.Exceptions;

[Serializable]
public class DeploymentException : Exception
{
    public Guid RequestId { get; }
    public Guid EnvironmentId { get; }

    public DeploymentException()
    {
    }

    public DeploymentException(
        Guid requestId,
        Guid environmentId,
        string? message) : base(message)
    {
        RequestId = requestId;
        EnvironmentId = environmentId;
    }

    public DeploymentException(string? message) : base(message)
    {
    }

    public DeploymentException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
