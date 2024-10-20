using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Models;
using PlatformPortalXL.Pilot;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Swashbuckle.AspNetCore.Annotations;

namespace PlatformPortalXL.Features.Pilot
{
    public class Node
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }
    }

    public class Edge
    {
        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("color")]
        public EdgeColor Color { get; set; }
    }

    public class EdgeColor
    {
        [JsonPropertyName("color")]
        public string Color { get; set; }
    }

    public class Graph
    {
        [JsonPropertyName("nodes")]
        public List<Node> Nodes { get; set; }

        [JsonPropertyName("edges")]
        public List<Edge> Edges { get; set; }
    }

    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class AdtGraphController : Controller
    {
        private readonly IAccessControlService _accessControl;
        private readonly IDigitalTwinApiService _digitalTwinApi;

        public AdtGraphController(IAccessControlService accessControl, IDigitalTwinApiService digitalTwinApi)
        {
            _accessControl = accessControl;
            _digitalTwinApi = digitalTwinApi;
        }

        [HttpGet("pilot/sites/{siteId}/adtgraph")]
        [Authorize]
        [SwaggerOperation("Gets ADT model graph", Tags = new [] { "Pilot" })]
        public async Task<IActionResult> GetSiteAdtModels([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var models = await _digitalTwinApi.GetAdtModelsAsync(siteId);
            var edges = new List<Edge>();
            foreach (var model in models)
            {
                if (model.ModelDefinition.Contents != null)
                {
                    var relationships = model.ModelDefinition.Contents.Where(x => x.HasType(ModelDefinitionContentTypeDto.Relationship));
                    edges.AddRange(relationships.Select(x => new Edge
                    {
                        From = model.Id,
                        To = x.Target,
                        Label = x.Name
                    }));
                }
                if (model.ModelDefinition.ExtendModelIds != null)
                {
                    edges.AddRange(model.ModelDefinition.ExtendModelIds.Select(x => new Edge
                    {
                        From = model.Id,
                        To = x,
                        Label = "extends",
                        Color = new EdgeColor { Color = "green" }
                    }));
                }
            }
            var graph = new Graph
            {
                Nodes = models.Where(x => x.ModelDefinition.Type == ModelDefinitionTypeDto.Interface)
                              .Select(x => new Node { Id = x.Id, Label = x.DisplayName?.En, Title = x.Id })
                              .ToList(),
                Edges = edges
            };
            return Ok(graph);
        }

        [HttpGet("pilot/sites/{siteId}/adtgraph/nodes/{nodeId}")]
        [Authorize]
        [SwaggerOperation("Gets ADT model node detail", Tags = new [] { "Pilot" })]
        public async Task<IActionResult> GetNode([FromRoute] Guid siteId, [FromRoute] string nodeId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var settings = new JsonSerializerOptions();
            settings.IgnoreNullValues = true;
            settings.IgnoreReadOnlyProperties = true;
            var model = await _digitalTwinApi.GetAdtModelAsync(siteId, nodeId);
            var node = new Node
            {
                Id = model.Id,
                Label = model.DisplayName.En,
                Title = model.Id,
                Data = JsonSerializer.Serialize(model.ModelDefinition, settings)
            };
            return Ok(node);
        }
    }
}
