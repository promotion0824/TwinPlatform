using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using Swashbuckle.AspNetCore.Annotations;

namespace PlatformPortalXL.Features.Management
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ConnectorTypesController : Controller
    {
        private readonly IAccessControlService _accessControl;
        private readonly IConnectorApiService _connectorApiService;

        public ConnectorTypesController(IAccessControlService accessControl, IConnectorApiService connectorApiService)
        {
            _accessControl = accessControl;
            _connectorApiService = connectorApiService;
        }

        [HttpGet("sites/{siteId}/ConnectorTypes")]
        [Authorize]
        [SwaggerOperation("Gets a list of Connector Types", Tags = new [] { "ConnectorTypes" })]
        public async Task<ActionResult<List<ConnectorTypeDto>>> Get(Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);
            var types = await _connectorApiService.GetConnectorTypesAsync();
            var response = new List<ConnectorTypeDto>();
            foreach (var connectorType in types)
            {
                var schemaColumns = await _connectorApiService.GetSchemaColumnsAsync(connectorType.ConnectorConfigurationSchemaId);
                response.Add(new ConnectorTypeDto
                {
                    Id = connectorType.Id, 
                    Name = connectorType.Name,
                    Columns = schemaColumns.Select(x => new ConnectorTypeColumnDto
                    {
                        Name = x.Name,
                        Type = x.DataType, 
                        IsRequired = x.IsRequired
                    }).ToList()
                });
            }
            return response;
        }

    }
}
