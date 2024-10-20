using Microsoft.Extensions.Configuration;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Features.Twins;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PlatformPortalXL.Services.Assets;
using Willow.Platform.Models;
using Willow.Common;
using static PlatformPortalXL.Features.Twins.TwinSearchResponse;

namespace PlatformPortalXL.Services.Twins
{
    public interface ITwinService
    {
        Task<List<TwinRelationshipDto>> GetTwinOutgoingRelationships(Guid siteId, string twinId, bool? excludeDocuments, bool? excludeAgents, bool? excludeEvents);
        Task<List<TwinRelationshipDto>> GetTwinIncomingRelationships(Guid siteId, string twinId, bool? excludeCapabilities);
        public Task ExportTwins(Stream streamToWrite, string queryId, TwinExport[] twins, Site[] userSites);
        Task<TwinRelationshipDto[]> GetTwinRelationships(Guid siteId, string twinId, TwinRelationshipsRequest request);
        Task<TwinRelationshipDto[]> GetTwinRelationships(string dtId, TwinRelationshipsRequest request);
        Task<List<PointDto>> GetTwinPointsAsync(Guid siteId, Guid twinId);
        Task ExportCognitiveSearchTwins(Stream streamToWrite, CognitiveSearchRequest request);
    }

    public class TwinService : ITwinService
    {
        private readonly IConnectorService _connectorService;
        private readonly IDigitalTwinAssetService _digitalTwinService;
        private readonly IDigitalTwinApiService _digitalTwinApiService;

        private readonly IConfiguration _configuration;

        public TwinService(
            IDigitalTwinAssetService digitalTwinService,
            IConnectorService connectorService,
            IDigitalTwinApiService digitalTwinApiService,
            IConfiguration configuration)
        {
            _connectorService = connectorService;
            _digitalTwinService = digitalTwinService;
            _digitalTwinApiService = digitalTwinApiService;
            _configuration = configuration;
        }

        public async Task<List<TwinRelationshipDto>> GetTwinOutgoingRelationships(Guid siteId, string twinId, bool? excludeDocuments, bool? excludeAgents, bool? excludeEvents)
        {
            // the ADT API only gets the outgoing relationships of the twin
            var outgoingRelationships = await _digitalTwinApiService.GetTwinRelationships(siteId, twinId);

            var excludingRelationshipNames = GetExcludedOutgoingRelationshipNames(excludeDocuments, excludeAgents, excludeEvents);

            return outgoingRelationships.Where(r => !excludingRelationshipNames.Contains(r.Name)).ToList();
        }

        public async Task<List<TwinRelationshipDto>> GetTwinIncomingRelationships(Guid siteId, string twinId, bool? excludeCapabilities)
        {
            var excludingRelationshipNames = GetExcludedIncomingRelationshipNames(excludeCapabilities);

            return await _digitalTwinApiService.GetTwinIncomingRelationships(siteId, twinId, excludingRelationshipNames.ToArray());
        }

        public async Task ExportTwins(Stream streamToWrite, string queryId, TwinExport[] twins, Site[] userSites)
        {
            var exportedTwins = new List<SearchTwin>();

            if (twins != null && twins.Length > 0)
            {
                var maxTwinsAllowedToExportPerQuery = _configuration.GetValue<int>("MaxTwinsAllowedToExportPerQuery");
                var twinToExportSplits = SplitTwinExports(twins.ToList(), maxTwinsAllowedToExportPerQuery);
                foreach (var twinsToExport in twinToExportSplits)
                {
                    var bulkQueryRequest = new TwinBulkQueryRequest
                    {
                        QueryId = queryId,
                        SiteIds = userSites.Select(x => x.Id).ToArray(),
                        Twins = twinsToExport.ToArray(),
                    };
                    var bulkQueryResult = await _digitalTwinApiService.BulkQuery(bulkQueryRequest);
                    exportedTwins.AddRange(bulkQueryResult.ToList());
                }
            }
            else
            {
                var bulkQueryRequest = new TwinBulkQueryRequest
                {
                    QueryId = queryId,
                    SiteIds = userSites.Select(x => x.Id).ToArray(),
                };
                var bulkQueryResult = await _digitalTwinApiService.BulkQuery(bulkQueryRequest);
                exportedTwins.AddRange(bulkQueryResult.ToList());
            }

            TwinExportHelper.ExportTwins(streamToWrite, exportedTwins.ToArray());
        }

        public async Task<TwinRelationshipDto[]> GetTwinRelationships(Guid siteId, string twinId, TwinRelationshipsRequest request)
        {
            const string virtualRelationshipId = "AUTOGENERATED-BUILDING-RELATIONSHIP";
            const string virtualRelationshipName = "locatedIn";
            var siteTwinTask = _digitalTwinApiService.GetTwinByUniqueId(siteId, siteId);
            var outgoingRelationshipsTask = GetTwinOutgoingRelationships(siteId, twinId, request.ExcludeDocuments, request.ExcludeAgents, request.ExcludeEvents);
            var incomingRelationshipsTask = GetTwinIncomingRelationships(siteId, twinId, request.ExcludeCapabilities);
            var locationRelationshipsTask = GetTwinLocationRelationships(twinId, siteId);

            await Task.WhenAll(siteTwinTask, outgoingRelationshipsTask, incomingRelationshipsTask, locationRelationshipsTask);

            var siteTwin = await siteTwinTask;
            var outgoingRelationships = await outgoingRelationshipsTask;
            var incomingRelationships = await incomingRelationshipsTask;
            var locationRelationships = await locationRelationshipsTask;

            if (locationRelationships.Any())
            {
                locationRelationships.RemoveAll(r => outgoingRelationships.Any(or => or.TargetId == r.TargetId));
            }

            var relationships = outgoingRelationships.Concat(incomingRelationships).Concat(locationRelationships).ToList();

            if (twinId == siteTwin.Id || relationships.Any(r => r.TargetId == siteTwin.Id || r.SourceId == siteTwin.Id))
            {
                return relationships.ToArray();
            }

            var twin = await _digitalTwinApiService.GetTwin<TwinDto>(siteId, twinId);
            relationships.Add(new TwinRelationshipDto
            {
                Id = virtualRelationshipId,
                Name = virtualRelationshipName,
                SourceId = twin.Id,
                Source = twin,
                TargetId = siteTwin.Id,
                Target = siteTwin
            });

            return relationships.ToArray();
        }

        public async Task<TwinRelationshipDto[]> GetTwinRelationships(string dtId, TwinRelationshipsRequest request)
        {
            var relationshipsTask = GetAndFilterTwinRelationships(dtId, request);
            var locationRelationshipsTask = GetTwinLocationRelationships(dtId);

            await Task.WhenAll(relationshipsTask, locationRelationshipsTask);

            var relationships = await relationshipsTask;
            var locationRelationships = await locationRelationshipsTask;

            if (locationRelationships.Any())
            {
                locationRelationships.RemoveAll(r => relationships.Any(or => or.TargetId == r.TargetId));
            }

            var allRelationships = relationships.Concat(locationRelationships).ToList();

            return allRelationships.ToArray();
        }

        public async Task<List<PointDto>> GetTwinPointsAsync(Guid siteId, Guid twinId)
        {
            var connectors = _connectorService.GetConnectors(siteId);
            var points = _digitalTwinApiService.GetTwinPointsAsync(siteId, twinId);
            Dictionary<Guid, DeviceDto> devices = new Dictionary<Guid, DeviceDto>();

            await Task.WhenAll(connectors, points);

            foreach (var point in points.Result)
            {
                if (point.Properties?.ContainsKey("connectorID") == true && point.Properties?["connectorID"].Value != null)
                {
                    point.ConnectorName = connectors.Result.Where(x => x.Id.ToString() == point.Properties["connectorID"].Value.ToString()).Select(x => x.Name).FirstOrDefault();
                }

                if (point.DeviceId.HasValue && !devices.Keys.Any(x => x.Equals(point.DeviceId.Value)))
                {
                    point.Device = await _digitalTwinService.GetDeviceAsync(siteId, point.DeviceId.Value);

                    if (point.Device.ConnectorId.HasValue)
                    {
                        point.Device.ConnectorName = connectors.Result.Where(x => x.Id.ToString() == point.Device.ConnectorId.Value.ToString()).Select(x => x.Name).FirstOrDefault();
                    }

                    devices.Add(point.DeviceId.Value, point.Device);
                }
                else if (point.DeviceId.HasValue)
                {
                    point.Device = devices[point.DeviceId.Value];
                }
            }

            return points.Result;
        }

        /// <summary>
        /// Split twins to export for allowable ADX query length
        /// </summary>
        /// <param name="twins">List of twins to export</param>
        /// <param name="size">Max number of twins that ADX can handle in where clause parameter</param>
        /// <returns></returns>
        private static List<List<TwinExport>> SplitTwinExports(List<TwinExport> twins, int size)
        {
            List<List<TwinExport>> result = new List<List<TwinExport>>();
            for (int i = 0; i < twins.Count; i += size)
            {
                result.Add(twins.GetRange(i, Math.Min(size, twins.Count - i)));
            }
            return result;
        }

        private async Task<List<TwinRelationshipDto>> GetTwinLocationRelationships(string twinId, Guid? siteId = default)
        {
            List<TwinRelationshipDto> locationRelationships = default;
            if (siteId.HasValue)
            {
                locationRelationships = await _digitalTwinApiService.GetTwinRelationshipsByQuery(siteId.Value, twinId, AdtConstants.RelationshipNames.Location, null, AdtConstants.MaxLocationHops);
            }
            else
            {
                locationRelationships = await _digitalTwinApiService.GetTwinRelationshipsByQuery(twinId, AdtConstants.RelationshipNames.Location, null, AdtConstants.MaxLocationHops);
            }

            // Return the relationships that only have the different target location twin e.g. some twins can be part of different levels which located in the same site
            return locationRelationships.DistinctBy(r => r.TargetId).ToList();
        }

        private async Task<List<TwinRelationshipDto>> GetAndFilterTwinRelationships(string dtId, TwinRelationshipsRequest request)
        {
            var relationships = await _digitalTwinApiService.GetTwinRelationships(dtId);

            var excludingOutRelationshipNames = GetExcludedOutgoingRelationshipNames(request.ExcludeDocuments, request.ExcludeAgents, request.ExcludeEvents);
            var excludingInRelationshipNames = GetExcludedIncomingRelationshipNames(request.ExcludeCapabilities);

            return relationships.Where(r => !(excludingOutRelationshipNames.Contains(r.Name) && r.SourceId == dtId))
                                .Where(r => !(excludingInRelationshipNames.Contains(r.Name) && r.TargetId == dtId)).ToList();
        }

        private List<string> GetExcludedOutgoingRelationshipNames(bool? excludeDocuments, bool? excludeAgents, bool? excludeEvents)
        {
            var excludedRelationshipNames = new List<string>();
            if (excludeDocuments ?? true)
                excludedRelationshipNames.Add(AdtConstants.RelationshipNames.HasDocument);
            if (excludeAgents ?? true)
                excludedRelationshipNames.AddRange(new[] { AdtConstants.RelationshipNames.CommissionedBy,
                                                             AdtConstants.RelationshipNames.InstalledBy,
                                                             AdtConstants.RelationshipNames.ServicedBy,
                                                             AdtConstants.RelationshipNames.ServiceResponsibility });
            if (excludeEvents ?? true)
                excludedRelationshipNames.Add(AdtConstants.RelationshipNames.ProducedBy);

            return excludedRelationshipNames;
        }

        private List<string> GetExcludedIncomingRelationshipNames(bool? excludeCapabilities)
        {
            var excludedRelationshipNames = new List<string>();
            if (excludeCapabilities ?? true)
                excludedRelationshipNames.Add(AdtConstants.RelationshipNames.IsCapabilityOf);

            return excludedRelationshipNames;
        }

        public async Task ExportCognitiveSearchTwins(Stream streamToWrite, CognitiveSearchRequest request)
        {
            var twins = await _digitalTwinApiService.GetCognitiveSearchTwins(request);

            await TwinExportHelper.ExportTwins(streamToWrite, twins);
        }
    }
}
