using System.Runtime.Serialization;

namespace Willow.ExceptionHandling.Exceptions;

[Serializable]
public class GitContentException : Exception
{
    public GitContentException()
    {
    }

    public GitContentException(string message) : base(message)
    {
    }

    public GitContentException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected GitContentException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
