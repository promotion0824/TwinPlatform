using LazyCache;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Entities;
using WorkflowCore.Infrastructure;
using WorkflowCore.Models;

namespace WorkflowCore.Services;

public interface ITicketStatusService
{
    Task<List<int>> GetClosedStatus();
    Task<bool> IsTicketClosed(string status);
    Task<List<TicketStatus>> GetCachedTicketStatusList();
    Task<List<int>> GetResolvedStatus();
    Task<List<int>> GetOpenedStatus();
}
public class TicketStatusService : ITicketStatusService
{
    private readonly WorkflowContext _workflowContext;
    private readonly IAppCache _appCache;
    /// <summary>
    /// Ticket status rarely changes, so the
    /// ticket status will be cached for 6 hours
    /// </summary>
    private const int CACHE_EXPIRATION_HOURS = 6;
    public TicketStatusService(WorkflowContext workflowContext, IAppCache appCache)
    {
        _workflowContext = workflowContext;
        _appCache = appCache;
    }


    /// <summary>
    /// Retrieves the list of ticket statuses from the cache if available,
    /// otherwise fetches it from the database and caches it.
    /// </summary>
    /// <returns>The list of ticket statuses.</returns>
    public async Task<List<TicketStatus>> GetCachedTicketStatusList()
    {
        var cachedTicketStatus = await _appCache.GetOrAddAsync(CacheKeys.TicketStatusList, async (cache) =>
        {
            cache.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CACHE_EXPIRATION_HOURS);
            var ticketStatus = await _workflowContext.TicketStatuses.ToListAsync();
            var ticketStatusModel = TicketStatusEntity.MapToModels(ticketStatus);
            return ticketStatusModel;
        });

        return cachedTicketStatus;
    }
    /// <summary>
    /// Get list of Closed Status by status code based on the closed tab
    /// Customer can configure the status and mapped to the closed tab
    /// </summary>
    /// <returns>List of status code that represents a closed status </returns>
    public async Task<List<int>> GetClosedStatus()
    {
        var allTicketStatus = await GetCachedTicketStatusList();
        var closedStatus =  allTicketStatus.Where(x => x.Tab == TicketTabs.CLOSED)
                                           .Select(x => x.StatusCode)
                                           .ToList();
        return closedStatus;
    }

    /// <summary>
    /// Check if a ticket is closed based on its status.
    /// </summary>
    /// <param name="status">The status of the ticket.</param>
    /// <returns>True if the ticket is closed, false otherwise.</returns>
    public async Task<bool> IsTicketClosed(string status)
    {
        var isValidStatus = Enum.TryParse<TicketStatusEnum>(status, true, out var ticketStatus);
        if (!isValidStatus)
        {
            return false;
        }
        var closedStatus = await GetClosedStatus();
        return closedStatus.Contains((int)ticketStatus);
    }
    /// <summary>
    /// Get list of Resolved Status by status code based on the Resolved tab
    /// Customer can configure the status and mapped to the Resolved tab
    /// </summary>
    /// <returns>List of status code that represents a Resolved status </returns>
    public async Task<List<int>> GetResolvedStatus()
    {
        var allTicketStatus = await GetCachedTicketStatusList();
        var resolvedStatus = allTicketStatus.Where(x => x.Tab == TicketTabs.RESOLVED)
                                           .Select(x => x.StatusCode)
                                           .ToList();
        return resolvedStatus;
    }

    /// <summary>
    /// Get list of Opened Status by status code based on the Opened tab
    /// Customer can configure the status and mapped to the Opened tab
    /// </summary>
    /// <returns>List of status code that represents a Opened status </returns>
    public async Task<List<int>> GetOpenedStatus()
    {
        var allTicketStatus = await GetCachedTicketStatusList();
        var openedStatus = allTicketStatus.Where(x => x.Tab == TicketTabs.OPEN)
                                           .Select(x => x.StatusCode)
                                           .ToList();
        return openedStatus;
    }

}

