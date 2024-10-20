using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Http;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Willow.Api.DataValidation;
using Willow.Common;
using Willow.Directory.Models;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Workflow;

namespace PlatformPortalXL.Features
{
    /// <summary>
    /// Any controller that requires language
    /// </summary>
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
