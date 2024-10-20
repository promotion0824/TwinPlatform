using System;
using DirectoryCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryCore.Controllers
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
