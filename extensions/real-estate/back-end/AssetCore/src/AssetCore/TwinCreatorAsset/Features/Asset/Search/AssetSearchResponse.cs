using System.Collections.Generic;
using AssetCoreTwinCreator.Dto;
using AssetCoreTwinCreator.Models;

namespace AssetCoreTwinCreator.Features.Asset.Search
{
    public class AssetSearchResponse
    {
        public IEnumerable<AssetDto> Assets { get; set; }
        public IEnumerable<CategoryColumnDto> CategoryColumns { get; set; }

        /// <summary>
        /// Count of all assets matching a the given query. This number is always equal to or greater than the number of actual Assets returned due to pagination.
        /// </summary>
        public int QueryAssetCount { get; set; }
        public string SortBy { get; set; }
        public bool SortByAscending { get; set; } = true;
    }
}
