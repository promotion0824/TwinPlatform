using System.Runtime.Serialization;

namespace Willow.Exceptions.Exceptions
{
    [Serializable]
    public class FileContentException : Exception
    {
        public FileContentException()
        {
        }

        public FileContentException(string message) : base(message)
        {
        }

        public FileContentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
