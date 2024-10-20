using System;
using System.Net;
using System.Runtime.Serialization;

namespace Willow.Infrastructure.Exceptions
{
    [Serializable]
    public class BadRequestException : ApiException
    {
        public BadRequestException(string message)
            : this(message, null)
        {
        }

        public BadRequestException(Exception innerException)
            : this(string.Empty, innerException)
        {
        }

        public BadRequestException(string message, Exception innerException)
            : base(HttpStatusCode.BadRequest, $"Bad request. {message}", null, innerException)
        {
        }

        protected BadRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
