using System;
using System.Collections.Generic;

namespace AssetCoreTwinCreator.Features.Asset.Search
{
    public class AssetSearchParameters
    {
        /// <summary>
        /// Optional skipping of first x records from the matching result set.  Use with LimitResultCount for pagination.
        /// </summary>
        public int? SkipResultCount { get; set; }
        /// <summary>
        /// Optional limiting of records returned from the matching result set. Use with SkipResultCount for pagination.
        /// </summary>
        public int? LimitResultCount { get; set; }
        /// <summary>
        /// Retreive dynamic category asset parameters if set to true. Use only when necessary for performance reasons.
        /// </summary>
        public bool RetrieveAssetParameters { get; set; }

        public string FilterByFloorCode { get; set; }
        public string FilterByKeyword { get; set; }
        public ValidationStatus FilterByValidationStatus { get; set; }
        public List<int> FilterByAssetRegisterIds { get; set; }
        public string SortBy { get; set; }
        public bool SortByAscending { get; set; } = true;
    }

    public enum ValidationStatus
    {
        All = 0,
        Valid = 1,
        Invalid = 2
    }

    public class AssetSearchParametersDto
    {
        /// <summary>
        /// Optional skipping of first x records from the matching result set.  Use with LimitResultCount for pagination.
        /// </summary>
        public int? SkipResultCount { get; set; }
        /// <summary>
        /// Optional limiting of records returned from the matching result set. Use with SkipResultCount for pagination.
        /// </summary>
        public int? LimitResultCount { get; set; }
        /// <summary>
        /// Retreive dynamic category asset parameters if set to true. Use only when necessary for performance reasons.
        /// </summary>
        public bool RetrieveAssetParameters { get; set; }

        public Guid? FilterByFloorId { get; set; }
        public string FilterByKeyword { get; set; }
        public ValidationStatus FilterByValidationStatus { get; set; }
        public List<Guid> FilterByAssetRegisterIds { get; set; }
        public string SortBy { get; set; }
        public bool SortByAscending { get; set; } = true;
    }
}
