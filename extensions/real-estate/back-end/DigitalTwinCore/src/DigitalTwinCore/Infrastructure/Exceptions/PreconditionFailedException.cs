using System;
using System.Net;
using System.Runtime.Serialization;

namespace Willow.Infrastructure.Exceptions
{
    [Serializable]
    public class PreconditionFailedException : ApiException
    {
        public PreconditionFailedException(string message)
            : this(message, null)
        {
        }

        public PreconditionFailedException(Exception innerException)
            : this(string.Empty, innerException)
        {
        }

        public PreconditionFailedException(string message, Exception innerException)
            : base(HttpStatusCode.PreconditionFailed, $"Precondition Failed. {message}", null, innerException)
        {
        }

        protected PreconditionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
