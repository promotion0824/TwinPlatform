using System;
using System.Collections.Generic;
using AssetCoreTwinCreator.Models;

namespace AssetCoreTwinCreator.Dto
{
    public class AssetSearchRequestDto
    {
        /// <summary>
        /// Building ID
        /// </summary>
        public Guid SiteId { get; set; }
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
        public List<AssetSearchTagDto> SearchTags { get; set; } = new List<AssetSearchTagDto>();
    }

    public class AssetSearchTagDto
    {
        public string Keyword { get; set; }
        public Guid? Id { get; set; }
        public AssetSearchType Type { get; set; }
    }
}
