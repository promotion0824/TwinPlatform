using System.Collections.Generic;

namespace AssetCoreTwinCreator.Models
{
    public class AssetSearchRequest
    {
        /// <summary>
        /// Building ID
        /// </summary>
        public int BuildingId { get; set; }
        /// <summary>
        /// Optional skipping of first x records from the matching result set.  Use with LimitResultCount for pagination.
        /// </summary>
        public int? SkipResultCount { get; set; }
        /// <summary>
        /// Optional limiting of records returned from the matching result set. Use with SkipResultCount for pagination.
        /// </summary>
        public int? LimitResultCount { get; set; }

        /// <summary>
        /// Asset Search Tags
        /// </summary>
        public IEnumerable<AssetSearchTag> SearchTags { get; set; } = new List<AssetSearchTag>();
    }

    public class AssetSearchTag
    {
        public string Keyword { get; set; }
        public int? Id { get; set; }
        public AssetSearchType Type { get; set; }
    }

    public enum AssetSearchType
    {
        ByFloor,
        ByDiscipline,
        ByResponsibility,
        ByFreeText,
    }
}
