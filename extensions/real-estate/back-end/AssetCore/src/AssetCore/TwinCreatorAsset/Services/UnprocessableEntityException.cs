using System;
using System.Net;
using System.Runtime.Serialization;

namespace Willow.Infrastructure.Exceptions
{
    [Serializable]
    public class UnprocessableEntityException : ApiException
    {
        public UnprocessableEntityException(string message, object data)
            : base(HttpStatusCode.UnprocessableEntity, $"Unprocessable entity. {message}", data, null)
        {
        }

        public UnprocessableEntityException(string message, object data, Exception innerException)
            : base(HttpStatusCode.UnprocessableEntity, $"Unprocessable entity. {message}", data, innerException)
        {
        }

        protected UnprocessableEntityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
