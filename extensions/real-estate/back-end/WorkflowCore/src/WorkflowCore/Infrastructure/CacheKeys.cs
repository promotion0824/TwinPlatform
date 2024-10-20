using WorkflowCore.Services;

namespace WorkflowCore.Infrastructure;
/// <summary>
/// Cache keys used by application
/// </summary>
public class CacheKeys
{
    /// <summary>
    /// Cache key for getting ticket status list from TicketStatusService.GetCachedTicketStatusList
    /// </summary>
    public const string TicketStatusList = nameof(TicketStatusService.GetCachedTicketStatusList);
}

