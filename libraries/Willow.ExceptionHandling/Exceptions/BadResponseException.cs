using System.Runtime.Serialization;

namespace Willow.ExceptionHandling.Exceptions
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

        protected BadResponseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
