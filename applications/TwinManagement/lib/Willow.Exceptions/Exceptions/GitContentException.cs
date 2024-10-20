using System.Runtime.Serialization;

namespace Willow.Exceptions.Exceptions;

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
}
