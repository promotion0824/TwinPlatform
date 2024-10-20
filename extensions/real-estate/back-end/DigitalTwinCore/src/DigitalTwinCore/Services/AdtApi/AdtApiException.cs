using Azure;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;

namespace DigitalTwinCore.Services.AdtApi
{
    [Serializable]
    public class AdtApiException : Exception
    {
        public AdtApiException(
            Exception innerException, 
            Uri instanceUri, 
            string methodName, 
            params KeyValuePair<string, object>[] parameters
            ) : base(innerException?.Message ?? $"An error occurred in {methodName}", innerException )
        {
            InstanceUri = instanceUri;
            Request = methodName;

            if (innerException is RequestFailedException requestFailedException)
            {
                StatusCode = (HttpStatusCode)requestFailedException.Status;
                ErrorCode = requestFailedException.ErrorCode;
            }

            Parameters = parameters == null ? null : new Dictionary<string, object>(parameters);
        }

        public AdtApiException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public HttpStatusCode StatusCode { get; private set; }
        public string ErrorCode { get; private set; }
        public string Request { get; private set; }
        public Uri InstanceUri { get; private set; }
        public Dictionary<string, object> Parameters { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
