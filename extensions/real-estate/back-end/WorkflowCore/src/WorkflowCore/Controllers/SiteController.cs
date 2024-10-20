using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Services.Apis;
using WorkflowCore.Models;

using Willow.Data;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class SiteController : ControllerBase
    {
        private readonly IReadRepository<Guid, Site> _siteRepo;
        public SiteController(IReadRepository<Guid, Site> siteRepo)
        {
            _siteRepo = siteRepo;
        }

        [HttpGet("sites/{siteId}")]
        [Authorize]
        public async Task<IActionResult> GetSite([FromRoute] Guid siteId)
        {
            var site = await _siteRepo.Get(siteId);
            return Ok(site);
        }
    }
}
