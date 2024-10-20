using DigitalTwinCore.Dto;
using DigitalTwinCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalTwinCore.Controllers
{
	[Route("[controller]")]
    [Authorize]
    [ApiController]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class TenantsController : ControllerBase
    {        
        private readonly ITenantsService _tenantsService;

        public TenantsController(ITenantsService tenantsService)
        {
            _tenantsService = tenantsService;
        }

		/// <summary>
		/// Retrieves the list of tenant data for the provided site ids
		/// </summary>
		/// <param name="siteIds">A list of sites ids</param>
		/// <returns>A list of tenant data</returns>
		///
		[HttpGet("")]
        [SwaggerOperation(OperationId = "GetTenants", Tags = new[] { "Tenants" })]
        public async Task<ActionResult<IEnumerable<TenantDto>>> GetTenants([FromQuery] List<Guid> siteIds, [FromQuery] bool useAdx)
        {
            var tenants = await _tenantsService.GetTenants(siteIds, useAdx);

			return Ok(tenants);
        }
    }
}