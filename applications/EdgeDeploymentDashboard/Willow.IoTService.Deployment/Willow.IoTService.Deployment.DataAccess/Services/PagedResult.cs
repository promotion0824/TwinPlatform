namespace Willow.IoTService.Deployment.DataAccess.Services;

public class PagedResult<T>
{
    public int TotalCount { get; init; }
    public IEnumerable<T> Items { get; init; } = ArraySegment<T>.Empty;
}
