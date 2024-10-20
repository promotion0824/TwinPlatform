using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetCoreTwinCreator.Features.Asset.Search
{
    public class AssetSearchQuery
    {
        public int CategoryId { get; set; }
        public SearchPagination Pagination { get; set; }
        public SearchSorting Sorting { get; set; }
        public SearchFilters  Filters { get; set; }
        public SearchInstigator Instigator { get; set; }
        public bool IncludeAssetDetails { get; set; }

        public class SearchPagination
        {
            public int? SkipResultCount { get; set; }
            public int? LimitResultCount { get; set; }
        }

        public class SearchSorting
        {
            public string SortBy { get; set; }
            public bool SortByAscending { get; set; } = true;
            public bool IsDefaultSorting => string.IsNullOrWhiteSpace(SortBy);
        }

        public class SearchFilters
        {
            public string FilterByFloorCode { get; set; }
            public string FilterByKeyword { get; set; }
            public ValidationStatus FilterByValidationStatus { get; set; }
            public IEnumerable<int> FilterByAssetRegisterIds { get; set; }

            public bool IsSearchByKeyword => string.IsNullOrWhiteSpace(FilterByKeyword) == false;
        }

        public class SearchInstigator
        {
            public bool CanViewAllAssets { get; set; }
            public int CompanyId { private get; set; }
            public string CompanyHierarchy { private get; set; }

            public IEnumerable<int> CompanyIds
            {
                get
                {
                    var companyIds = new List<int> { CompanyId };

                    if (string.IsNullOrWhiteSpace(CompanyHierarchy) == false)
                    {
                        foreach (var cId in CompanyHierarchy.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (int.TryParse(cId, out var companyId))
                            {
                                companyIds.Add(companyId);
                            }
                        }
                    }

                    return companyIds.Distinct();
                }
            }
        }
    }
}