using AutoMapper;
using AssetCoreTwinCreator.BusinessLogic.AssetOperations.Shared;
using AssetCoreTwinCreator.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetCoreTwinCreator.Services;
using DTO = AssetCoreTwinCreator.Models;

namespace AssetCoreTwinCreator.BusinessLogic.AssetOperations.ReadAssets
{
    public class ReadAssets : BaseAssetOperation, IReadAssets
    {
        private readonly IAssetRegisterIndexCacheService _assetRegisterIndexCacheService;

        public ReadAssets(AssetDbContext dbContext, ILogger<ReadAssets> logger, IMapper mapper, IAssetRegisterIndexCacheService assetRegisterIndexCacheService)
            : base(dbContext, logger, mapper)
        {
            _assetRegisterIndexCacheService = assetRegisterIndexCacheService;
        }

        /// <summary>
        /// Searches assets by various filters
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<DTO.Asset>> SearchAssetsAsync(DTO.AssetSearchRequest searchRequest, Guid userId, bool isSuperuser, bool includeCategory)
        {
            var result = new List<DTO.Asset>();

            if (searchRequest.LimitResultCount == 0)
            {
                return result;
            }

            // companyId hierarchy filter. only users from same company or ancestor companies can view.
            var companyIds = new List<int>();

            var assetQuery = await _dbContext.GetAssetsSearchQuery(searchRequest, companyIds, _assetRegisterIndexCacheService);

            if (includeCategory)
            {
                assetQuery = assetQuery.Include(x => x.Category).ThenInclude(x => x.ParentCategory);
            }
            var assets = await assetQuery.AsNoTracking().ToListAsync();

            result = _mapper.Map<List<DTO.Asset>>(assets);


            return result;
        }

    }
}
