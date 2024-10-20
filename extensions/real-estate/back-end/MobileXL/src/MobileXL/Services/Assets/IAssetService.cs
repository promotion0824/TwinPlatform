using MobileXL.Dto;
using MobileXL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MobileXL.Services
{
    public interface IAssetService
    {
        Task<IEnumerable<LightCategoryDto>> GetCategories(Guid siteId, Guid? floorId, bool? liveDataAssetsOnly);
        Task<List<AssetCategory>> GetAssetCategoriesTreeAsync(Guid siteId, Guid? floorId, bool? liveDataAssetsOnly, string searchKeyword = null);
        Task<List<Asset>> GetAssetsAsync(Guid siteId, Guid? categoryId, Guid? floorId, bool? liveDataAssetsOnly, string searchKeyword);
        Task<FileStreamResult> GetFileAsync(Guid siteId, Guid assetId, Guid fileId);
        Task<List<AssetFile>> GetFilesAsync(Guid siteId, Guid assetId);
        Task<Asset> GetAssetDetailsAsync(Guid siteId, Guid assetId);
        Task<List<Asset>> GetAssetsPagedAsync(Guid siteId, Guid? categoryId, Guid? floorId, bool? liveDataAssetsOnly, string searchKeyword, int? pageNumber, int? pageSize);
    }
}
