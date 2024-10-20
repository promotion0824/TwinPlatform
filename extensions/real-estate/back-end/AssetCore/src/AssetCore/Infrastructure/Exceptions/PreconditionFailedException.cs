using System;
using System.Net;
using System.Runtime.Serialization;
using Willow.Infrastructure.Exceptions;

namespace AssetCore.Infrastructure.Exceptions
{
    [Serializable]
    public class PreconditionFailedException : ApiException
    {
        public PreconditionFailedException(string message, Exception innerException) 
            : base(HttpStatusCode.PreconditionFailed, $"Precondition failed. {message}", null, innerException)
        {
        }

        protected PreconditionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}