using System.Runtime.Serialization;

namespace Willow.ExceptionHandling.Exceptions;

[Serializable]
public class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(object entity) : base($"Entity {entity} not found")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected NotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
