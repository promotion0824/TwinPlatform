using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Twins;

namespace PlatformPortalXL.Services.CognitiveSearch.Extensions;

public static class SearchDocumentDtoToSearchTwin
{
    public static TwinSearchResponse.SearchTwin AsSearchTwin(this SearchDocumentDto doc, ILogger logger = null)
    {
        if (!Guid.TryParse(doc.SiteId, out var siteId) && logger is not null)
        {
            logger.LogWarning("Could not parse SiteId '{SiteId}' as Guid. TwinSearchResponse.SearchTwin.SiteId for the twin '{TwinId}' will not be set as a result.", doc.SiteId, doc.Id);
        }

        return new TwinSearchResponse.SearchTwin
        {
            Id = doc.Id,
            Name = doc.Names.FirstOrDefault(),
            ModelId = doc.PrimaryModelId,
            SiteId = siteId,
            ExternalId = doc.ExternalId,
            Locations = doc.Location is not null ? [..doc.Location] : [],
            ModelIds = doc.ModelIds is not null ? [..doc.ModelIds] : [],
        };
    }
}
