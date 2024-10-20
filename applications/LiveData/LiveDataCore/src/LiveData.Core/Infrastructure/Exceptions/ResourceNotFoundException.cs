namespace Willow.Infrastructure.Exceptions;

using System;

internal class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string resourceType, string resourceId)
        : this(resourceType, resourceId, string.Empty, null)
    {
    }

    public ResourceNotFoundException(string resourceType, Guid resourceId)
        : this(resourceType, resourceId.ToString(), string.Empty, null)
    {
    }

    public ResourceNotFoundException(string resourceType, string resourceId, string message)
        : this(resourceType, resourceId, message, null)
    {
    }

    public ResourceNotFoundException(string resourceType, string resourceId, Exception innerException)
        : this(resourceType, resourceId, string.Empty, innerException)
    {
    }

    public ResourceNotFoundException(string resourceType, string resourceId, string message, Exception innerException)
        : base($"Resource({resourceType}: {resourceId}) cannot be found. {message}", innerException)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    public string ResourceType { get; set; }

    public string ResourceId { get; set; }
}
