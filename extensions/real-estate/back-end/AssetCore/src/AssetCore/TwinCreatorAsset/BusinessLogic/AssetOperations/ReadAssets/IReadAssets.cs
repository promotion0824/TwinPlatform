using AssetCoreTwinCreator.BusinessLogic.AssetOperations.Shared;
using AssetCoreTwinCreator.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetSearchRequest = AssetCoreTwinCreator.Models.AssetSearchRequest;

namespace AssetCoreTwinCreator.BusinessLogic.AssetOperations.ReadAssets
{
    public interface IReadAssets
    {
        Task<IEnumerable<Asset>> SearchAssetsAsync(AssetSearchRequest searchRequest, Guid userId, bool isSuperuser, bool includeCategory);
    }
}
