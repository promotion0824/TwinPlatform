namespace Willow.IoTService.Deployment.Dashboard.Application.Queries;

using System.ComponentModel;
using Ardalis.GuardClauses;

public record PagedQuery
{
    internal const int MaxPageSize = 50;

    protected PagedQuery(int page, int pageSize)
    {
        Guard.Against.NegativeOrZero(page);
        Guard.Against.InvalidInput(
            pageSize,
            nameof(pageSize),
            x => x is > 0 and <= MaxPageSize);
        this.Page = page;
        this.PageSize = pageSize;
    }

    /// <summary>
    ///     Gets the page number. Default value 1. Must be greater than 0.
    /// </summary>
    [DefaultValue(1)]
    public int Page { get; }

    /// <summary>
    ///     Gets the page size. Default value 20. Must be inclusively between 1 and 50.
    /// </summary>
    [DefaultValue(20)]
    public int PageSize { get; }
}
