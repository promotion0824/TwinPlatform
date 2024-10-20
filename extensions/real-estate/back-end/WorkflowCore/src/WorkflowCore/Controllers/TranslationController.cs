using Microsoft.AspNetCore.Mvc;
using System;
using WorkflowCore.Http;

namespace WorkflowCore.Controllers
{
    public abstract class TranslationController : ControllerBase
    {
        private readonly IHttpRequestHeaders _headers;

        public TranslationController(IHttpRequestHeaders headers)
        {
            _headers = headers ?? throw new ArgumentNullException(nameof(headers));
        }

        protected string Language => _headers.Get(HttpContext, "language", false) ?? "en";
    }
}
