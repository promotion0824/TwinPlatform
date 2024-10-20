using System;
using System.Net;
using System.Runtime.Serialization;

namespace Willow.Infrastructure.Exceptions
{
    [Serializable]
    public class ServerException : ApiException
    {
        public ServerException(string message)
            : this(message, null) { }

        public ServerException(Exception innerException)
            : this(string.Empty, innerException) { }

        public ServerException(string message, Exception innerException)
            : base(
                HttpStatusCode.InternalServerError,
                $"Internal Server Error. {message}",
                null,
                innerException
            ) { }

        protected ServerException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
