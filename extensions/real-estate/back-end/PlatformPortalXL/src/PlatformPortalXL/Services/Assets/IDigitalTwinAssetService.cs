using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.Platform.Models;
namespace PlatformPortalXL.Services.Assets;

public interface IDigitalTwinAssetService 
{
    Task<IEnumerable<LightCategoryDto>> GetCategories(Guid siteId, Guid? floorId, bool? liveDataAssetsOnly);
    Task<List<AssetCategory>> GetAssetCategoriesTreeAsync(Guid siteId, Guid? floorId, bool? liveDataAssetsOnly, string searchKeyword = null, bool isCategoryOnly = false, List<string> modelNames = null);
    Task<List<Asset>> GetAssetsAsync(Guid siteId, Guid? categoryId, Guid? floorId, bool? liveDataAssetsOnly, bool? subCategories, string searchKeyword, List<string> modelNames = null);
    Task<List<AssetMinimum>> GetAssetsAsync(Guid siteId, IList<Guid> assetOrEquipmentIds);
    Task<List<Asset>> GetAssetsPagedAsync(Guid siteId, Guid? categoryId, Guid? floorId, bool? liveDataAssetsOnly, bool? subCategories, string searchKeyword, int? pageNumber, int? pageSize);
    Task<List<AssetMinimum>> GetAssetsByIds(Guid siteId, IEnumerable<Guid> assetIds);


    Task<FileStreamResult> GetFileAsync(Guid siteId, Guid assetId, Guid fileId);
    Task<List<AssetFile>> GetFilesAsync(Guid siteId, Guid assetId);
    Task<IEnumerable<Asset>> GetWarrantyAsync(Guid siteId, Guid assetId);
    Task<Asset> GetAssetDetailsByForgeViewerModelIdAsync(Guid siteId, string forgeViewerModelId);
    Task<Asset> GetAssetDetailsAsync(Guid siteId, Guid assetId);
    Task<Asset> GetAssetDetailsByEquipmentIdAsync(Guid siteId, Guid equipmentId);
    Task<List<TicketIssueDto>> GetPossibleTicketIssuesAsync(Guid siteId, Guid? floorId, string keyword);

    Task<AssetPoint> GetPointAsync(Guid siteId, Guid pointId);
    Task<Page<Point>> GetPointsPagedAsync(Guid siteId, bool? includeAssets, string continuationToken);

    Task<DeviceDto> GetDeviceAsync(Guid siteId, Guid deviceId);
    Task<List<EquipmentSimpleDto>> GetAssetsNamesAsync(List<AssetNamesForMultiSitesRequest> reques);
}
