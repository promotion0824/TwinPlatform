using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace WorkflowCore.Http
{
    public interface IHttpRequestHeaders
    {
        string Get(HttpContext context, string name, bool required = true);
    }

    public class HttpRequestHeaders : IHttpRequestHeaders
    {
        public string Get(HttpContext context, string name, bool required = true)
        {
            var header = context.Request.Headers[name];

            if (header.Count == 0 && required)
                throw new Exception($"Missing {name} header");

            if (header.Count == 0)
                return null;

            return header.First();
        }
    }
}
