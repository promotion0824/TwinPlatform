using System.Collections.Generic;

namespace AssetCoreTwinCreator.Features.Asset.Search
{
    public class AssetSearchResult
    {
        public IEnumerable<Models.Asset> Assets { get; set; }
        public int TotalCount { get; set; }
    }
}