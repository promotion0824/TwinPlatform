using System;
using System.Net;
using System.Runtime.Serialization;

namespace Willow.Infrastructure.Exceptions
{
    [Serializable]
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public object AdditionalData { get; set; }

        public ApiException(
            HttpStatusCode statusCode,
            string message,
            object additionalData,
            Exception innerException
        )
            : base(message, innerException)
        {
            StatusCode = statusCode;
            AdditionalData = additionalData;
        }

        protected ApiException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
