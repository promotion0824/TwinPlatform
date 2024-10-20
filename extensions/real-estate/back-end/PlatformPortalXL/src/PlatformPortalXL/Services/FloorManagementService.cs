using Microsoft.Extensions.Logging;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.SiteCore;
using PlatformPortalXL.Services.LiveDataApi;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using PlatformPortalXL.ServicesApi.InsightApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Batch;
using Willow.Common;
using Willow.ExceptionHandling.Exceptions;

namespace PlatformPortalXL.Services
{
	public interface IFloorManagementService
    {
        Task<LayerGroupList> GetLayerGroupsAsync(Guid siteId, Guid floorId);
        Task<LayerGroup> CreateLayerGroupAsync(Guid siteId, Guid floorId, CreateLayerGroupRequest createRequest);
        Task<LayerGroup> UpdateLayerGroupAsync(Guid siteId, Guid floorId, Guid layerGroupId, UpdateLayerGroupRequest updateRequest);
        Task DeleteLayerGroupAsync(Guid siteId, Guid floorId, Guid layerGroupId);
        Task<List<Point>> GetPointsByLayerAsync(Guid siteId, Guid floorId, Guid layerGroupId, Guid layerId);
        Task<List<PointLive>> GetPointsLiveDataByLayerAsync(Guid siteId, Guid floorId, Guid layerGroupId, Guid layerId);
    }

    public class FloorManagementService : IFloorManagementService
    {
        private readonly ILayerGroupsApiService _layerGroupsApi;
        private readonly IConnectorPointsService _connetorPointsService;
        private readonly IInsightApiService _insightApi;
        private readonly ILiveDataApiService _liveDataApi;
        private readonly IImageUrlHelper _urlHelper;
        private readonly IDigitalTwinService _digitalTwinService;
        private readonly ILogger<FloorManagementService> _logger;

        public FloorManagementService(
            ILayerGroupsApiService layerGroupsApi,
            IConnectorPointsService connetorPointsService,
            IInsightApiService insightApi,
            ILiveDataApiService liveDataApi,
            IImageUrlHelper urlHelper,
            IDigitalTwinService digitalTwinService,
            ILogger<FloorManagementService> logger)
        {
            _layerGroupsApi = layerGroupsApi;
            _connetorPointsService = connetorPointsService;
            _insightApi = insightApi;
            _liveDataApi = liveDataApi;
            _urlHelper = urlHelper;
            _digitalTwinService = digitalTwinService;
            _logger = logger;
        }

        public async Task<LayerGroupList> GetLayerGroupsAsync(Guid siteId, Guid floorId)
        {
            using (_logger?.BeginScope("GetLayerGroupsAsync site:{site} floor:{floor}", siteId, floorId))
            {
                var layerGroups = await _layerGroupsApi.GetLayerGroupsAsync(siteId, floorId);
               
                return LayerGroupListCore.MapToModel(layerGroups, _urlHelper, _logger);
            }
        }

        public async Task<LayerGroup> CreateLayerGroupAsync(Guid siteId, Guid floorId, CreateLayerGroupRequest createRequest)
        {
            var newLayerGroup = await _layerGroupsApi.CreateLayerGroupAsync(siteId, floorId, createRequest);
            return LayerGroupCore.MapToModel(newLayerGroup);
        }

        public async Task<LayerGroup> UpdateLayerGroupAsync(Guid siteId, Guid floorId, Guid layerGroupId, UpdateLayerGroupRequest updateRequest)
        {
            var updatedLayerGroup = await _layerGroupsApi.UpdateLayerGroupAsync(siteId, floorId, layerGroupId, updateRequest);
            return LayerGroupCore.MapToModel(updatedLayerGroup);
        }

        public async Task DeleteLayerGroupAsync(Guid siteId, Guid floorId, Guid layerGroupId)
        {
            await _layerGroupsApi.DeleteLayerGroupAsync(siteId, floorId, layerGroupId);
        }

        public async Task<List<Point>> GetPointsByLayerAsync(Guid siteId, Guid floorId, Guid layerGroupId, Guid layerId)
        {
            var layerGroup = await _layerGroupsApi.GetLayerGroupAsync(siteId, floorId, layerGroupId);
            var layer = layerGroup.Layers.FirstOrDefault(l => l.Id == layerId);

            if (layer == null)
            {
                throw new NotFoundException().WithData(new { layerId });
            }

            var points = await _connetorPointsService.GetPointsByTagNameAsync(siteId, layer.TagName, true);
            return PointCore.MapToModels(points);
        }

        public async Task<List<PointLive>> GetPointsLiveDataByLayerAsync(Guid siteId, Guid floorId, Guid layerGroupId, Guid layerId)
        {
            var layerGroup = await _layerGroupsApi.GetLayerGroupAsync(siteId, floorId, layerGroupId);
            var layer = layerGroup.Layers.FirstOrDefault(l => l.Id == layerId);

            if (layer == null)
            {
                throw new NotFoundException().WithData(new { layerId });
            }

            //this returns all points of site by specified tag
            var points = await _connetorPointsService.GetPointsByTagNameAsync(siteId, layer.TagName, true);

            //filter only those points that are related to equipments that are mentioned in layer group
            points = points.Where(point => point.Equipment.Select(e => e.Id).Intersect(layerGroup.Equipments.Select(e => e.Id)).Any()).ToList();

            var result = new List<PointLive>();
            foreach (var pointsGroup in points.GroupBy(p => p.ClientId))
            {
                var twinIds = pointsGroup.Select(p => p.TwinId).ToList();
                var partialResult = await _liveDataApi.GetLastTrendlogsAsync(pointsGroup.Key, siteId, twinIds);

                var pointsLive = (from point in pointsGroup
                    join rawValue in partialResult on point.TwinId equals rawValue.Id into joinedValues
                    from joinedValue in joinedValues.DefaultIfEmpty()
                    select new PointLive {Point = PointCore.MapToModel(point), RawData = joinedValue});

                result.AddRange(pointsLive);
            }

            return result;
        }

    }
}
