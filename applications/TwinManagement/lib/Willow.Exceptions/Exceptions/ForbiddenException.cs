using System.Runtime.Serialization;

namespace Willow.Exceptions.Exceptions;

[Serializable]
public class ForbiddenException : Exception
{
    public ForbiddenException(Exception innerException)
        : this(string.Empty, innerException)
    {
    }

    public ForbiddenException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
