using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Willow.Api.Client
{
    public class RestException : Exception
    {
        public RestException(string msg, HttpStatusCode statusCode, string response) : base(msg)
        {
            this.StatusCode = statusCode;
            this.Response = response;
        }

        public RestException(HttpStatusCode statusCode) 
        {
            this.StatusCode = statusCode;
            this.ResponseMessage = new HttpResponseMessage();
        }

        public HttpStatusCode StatusCode { get; }
        public string ApiName { get; set; }
        public string Url { get; set; }
        public object Headers { get; set; }
        public string Response { get; set; }

        /// <summary>
        /// Response message from the server. Can be used to interrogate the response headers, etc.
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; set; }
    }

    public class RestNotFoundException : RestException
    {
        public RestNotFoundException() : base(HttpStatusCode.NotFound)
        {

        }

        public RestNotFoundException(string msg, string response) : base(msg, HttpStatusCode.NotFound, response)
        {

        }
    }
}
