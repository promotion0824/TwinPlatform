using Ardalis.GuardClauses;

namespace Willow.IoTService.Deployment.DataAccess.Services;

public record PagedQuery
{
    private const int MaxPageSize = 50;

    protected PagedQuery(int page, int pageSize)
    {
        Guard.Against.NegativeOrZero(page);
        Guard.Against.InvalidInput(pageSize,
                                   nameof(pageSize),
                                   x => x is > 0 and <= MaxPageSize);
        Page = page;
        PageSize = pageSize;
    }

    public int Page { get; init; }
    public int PageSize { get; init; }

    public void Deconstruct(out int Page, out int PageSize)
    {
        Page = this.Page;
        PageSize = this.PageSize;
    }
}
