using DigitalTwinCore.Dto;
using DigitalTwinCore.DTO;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Willow.Tests.Infrastructure.MockServices
{
    public class MockCachelessAssetService : IAssetService
    {
        public ILogger Logger => throw new NotImplementedException();
        public TwinHistoryDto TwinHistoryDto { get; set; }
        public TwinFieldsDto TwinFieldsDto { get; set; }
		public List<TwinSimpleDto> AssetNamesForMultiSitesResponseListDto { get; set; }
        public List<PointTwinDto> PointTwinDtos { get; set; }
        public List<TwinGeometryViewerIdDto> TwinGeometryViewerIdDtos { get; set; }

        public Task<Asset> GetAssetByForgeViewerId(Guid siteId, string forgeViewerId)
        {
            throw new NotImplementedException();
        }

        public Task<Asset> GetAssetById(Guid siteId, string id)
        {
            throw new NotImplementedException();
        }

        public Task<Asset> GetAssetByUniqueId(Guid siteId, Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AssetNameDto>> GetAssetNames(Guid siteId, IEnumerable<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public Task<List<Point>> GetAssetPointsAsync(Guid siteId, Guid assetId)
        {
            throw new NotImplementedException();
        }

        public Task<AssetRelationshipsDto> GetAssetRelationshipsAsync(Guid siteId, Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Page<Asset>> GetAssets(Guid siteId, Guid? categoryId, Guid? floorId, string searchKeyword, bool liveDataOnly = false, bool includeExtraProperties = false, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<Asset>> GetAssetsAsync(Guid siteId, Guid? categoryId, Guid? floorId, string searchKeyword, bool liveDataOnly = false, bool includeExtraProperties = false, int startItemIndex = 0, int pageSize = int.MaxValue)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LightCategoryDto>> GetCategories(Guid siteId, Guid? floorId, bool isLiveDataOnly)
        {
            throw new NotImplementedException();
        }

        public Task<List<Category>> GetCategoriesAndAssetsAsync(Guid siteId, bool isCategoryOnly, List<string> modelNames, Guid? floorId = null)
        {
            throw new NotImplementedException();
        }

        public Task<Page<Category>> GetCategoriesAndAssetsAsync(Guid siteId, bool isCategoryOnly, List<string> modelNames, Guid? floorId = null, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Device> GetDeviceByExternalPointIdAsync(Guid siteId, string externalPointId)
        {
            throw new NotImplementedException();
        }

        public Task<Device> GetDeviceByUniqueIdAsync(Guid siteId, Guid id, bool? includePoints)
        {
            throw new NotImplementedException();
        }

        public Task<Page<Device>> GetDevicesAsync(Guid siteId, bool? includePoints, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<Device>> GetDevicesAsync(Guid siteId, bool? includePoints)
        {
            throw new NotImplementedException();
        }

        public Task<Page<Device>> GetDevicesByConnectorAsync(Guid siteId, Guid connectorId, bool? includePoints, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<Device>> GetDevicesByConnectorAsync(Guid siteId, Guid connectorId, bool? includePoints)
        {
            throw new NotImplementedException();
        }

        public Task<List<Document>> GetDocumentsForAssetAsync(Guid siteId, Guid assetId)
        {
            throw new NotImplementedException();
        }

        public Task<List<(TwinWithRelationships PointTwin, Point Point, Asset Asset)>> GetPointAssetPairsByPointIdsAsync(Guid siteId, List<Guid> pointUniqIds, List<string> pointExternalIds, List<Guid> pointTrendIds, bool includePointsWithNoAssets = false)
        {
            throw new NotImplementedException();
        }

        public Task<Page<(TwinWithRelationships PointTwin, Point Point, Asset Asset)>> GetPointAssetPairsByPointIdsAsync(Guid siteId, List<Guid> pointUniqIds, List<string> pointExternalIds, List<Guid> pointTrendIds, bool includePointsWithNoAssets = false, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Point> GetPointByExternalIdAsync(Guid siteId, string externalId)
        {
            throw new NotImplementedException();
        }

        public Task<Point> GetPointByTrendIdAsync(Guid siteId, Guid trendId)
        {
            throw new NotImplementedException();
        }

        public Task<Point> GetPointByTwinIdAsync(Guid siteId, string twinId)
        {
            throw new NotImplementedException();
        }

        public Task<Point> GetPointByUniqueIdAsync(Guid siteId, Guid pointId)
        {
            throw new NotImplementedException();
        }

        public Task<Page<Point>> GetPointsAsync(Guid siteId, bool includeAssets, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<Point>> GetPointsAsync(Guid siteId, bool includeAssets, int startItemIndex = 0, int pageSize = int.MaxValue)
        {
            throw new NotImplementedException();
        }

        public Task<Page<Point>> GetPointsByConnectorAsync(Guid siteId, Guid connectorId, bool includeAssets = true, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<Point>> GetPointsByConnectorAsync(Guid siteId, Guid connectorId, bool includeAssets = true)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetPointsByConnectorCountAsync(Guid siteId, Guid connectorId)
        {
            throw new NotImplementedException();
        }

        public Task<Page<Point>> GetPointsByTagAsync(Guid siteId, string tag, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<Point>> GetPointsByTagAsync(Guid siteId, string tag)
        {
            throw new NotImplementedException();
        }

        public Task<List<Point>> GetPointsByTrendIdsAsync(Guid siteId, List<Guid> trendIds)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetPointsCountAsync(Guid siteId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LiveDataIngestPointDto>> GetSimplePointAssetPairsByPointIdsAsync(Guid siteId, List<Guid> pointUniqIds, List<string> pointExternalIds, List<Guid> pointTrendIds, bool includePointsWithNoAssets = false)
        {
            throw new NotImplementedException();
        }

        public async Task<TwinHistoryDto> GetTwinHistory(Guid siteId, string twinId)
        {
            return await Task.FromResult(TwinHistoryDto);
        }

        public async Task<TwinFieldsDto> GetTwinFields(Guid siteId, string twinId)
        {
            return await Task.FromResult(TwinFieldsDto);
        }

        public Task<Device> UpdateDeviceMetadataAsync(Guid siteId, Guid deviceId, DeviceMetadataDto device)
        {
            throw new NotImplementedException();
        }

		public async Task<List<TwinSimpleDto>> GetSimpleTwinsDataAsync(IEnumerable<TwinsForMultiSitesRequest> request, CancellationToken cancellationToken)
		{
			return await Task.FromResult(AssetNamesForMultiSitesResponseListDto);
		}

        public async Task<List<PointTwinDto>> GetPointsByTwinIdsAsync(Guid siteId, List<string> pointTwinIds)
        {
            return await Task.FromResult(PointTwinDtos);
        }
        public async Task<List<TwinGeometryViewerIdDto>> GetTwinsWithGeometryIdAsync(
            GetTwinsWithGeometryIdRequest request)
        {
            return await Task.FromResult(TwinGeometryViewerIdDtos);
        }
    }
}
