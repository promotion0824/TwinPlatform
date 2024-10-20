using System;
using System.Net;
using System.Runtime.Serialization;

namespace Willow.Infrastructure.Exceptions
{
    [Serializable]
    public class ResourceNotFoundException : ApiException
    {
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }

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

        public ResourceNotFoundException(string resourceType, Guid resourceId, string message)
            : this(resourceType, resourceId.ToString(), message, null)
        {
        }

        public ResourceNotFoundException(string resourceType, string resourceId, Exception innerException)
            : this(resourceType, resourceId, string.Empty, innerException)
        {
        }

        public ResourceNotFoundException(string resourceType, string resourceId, string message, Exception innerException)
            : base(
                HttpStatusCode.NotFound,
                $"Resource({resourceType}: {resourceId}) cannot be found. {message}",
                new { ResourceType = resourceType, ResourceId = resourceId },
                innerException)
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }

        protected ResourceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
