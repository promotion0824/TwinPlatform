using System.Linq;
using Willow.Batch;

namespace Willow.AzureDigitalTwins.Api.Helpers;

/// <summary>
/// Willow Pagination Batch Request Query Helper
/// </summary>
public class BatchRequestQueryHelper
{
    public static IQueryable<T> ApplyWhere<T>(IQueryable<T> queryable, FilterSpecificationDto[] filterSpecifications)
    {
        if (filterSpecifications == null || filterSpecifications.Length == 0)
        {
            return queryable;
        }
        return queryable.FilterBy(filterSpecifications);

    }

    public static IQueryable<T> ApplySort<T>(IQueryable<T> queryable, SortSpecificationDto[] sortSpecifications)
    {
        if (sortSpecifications == null || sortSpecifications.Length == 0)
            return queryable;
        return queryable.SortBy(sortSpecifications);
    }

    public static IQueryable<T> ApplyPagination<T>(IQueryable<T> queryable, int? page, int? take, out int skipped)
    {
        skipped = 0;

        if (page.HasValue && take.HasValue && page.Value > 0)
        {
            skipped = (page.Value - 1) * take.Value;

            queryable = queryable.Skip(skipped);
        }

        if (take.HasValue && take.Value > 0 && take.Value < 1000000)
        {
            queryable = queryable.Take(take.Value);
        }

        return queryable;
    }
}
