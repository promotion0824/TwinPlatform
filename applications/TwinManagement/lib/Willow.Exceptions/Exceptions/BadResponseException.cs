using System.Runtime.Serialization;

namespace Willow.Exceptions.Exceptions
{
    [Serializable]
    public class BadResponseException : Exception
    {
        public BadResponseException()
        {
        }

        public BadResponseException(string message) : base(message)
        {
        }

        public BadResponseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
