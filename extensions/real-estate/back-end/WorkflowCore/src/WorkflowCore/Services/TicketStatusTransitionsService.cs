using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Infrastructure.Configuration;

namespace WorkflowCore.Services;

public interface ITicketStatusTransitionsService
{
    Task<bool> IsValidStatusTransitionAsync(int fromStatus, int toStatus);
    Task<List<int>> GetNextValidStatusAsync(int currentStatus);
    Task<TicketStatusTransitionsDto> GetTicketStatusTransitionsAsync();
}
public class TicketStatusTransitionsService : ITicketStatusTransitionsService
{
    private readonly WorkflowContext _workflowContext;
    private readonly AppSettings _appSettings;

    public TicketStatusTransitionsService(WorkflowContext workflowContext, IConfiguration configuration)
    {
        _workflowContext = workflowContext;
        _appSettings = configuration.Get<AppSettings>();
    }

    /// <summary>
    /// Checks if the status transition from the given 'fromStatus' to the 'toStatus' is valid.
    /// This validation only applied if Mapped enabled
    /// </summary>
    /// <param name="fromStatus">The current status of the ticket.</param>
    /// <param name="toStatus">The status to transition to.</param>
    /// <returns>True if the status transition is valid, otherwise false.</returns>
    public async Task<bool> IsValidStatusTransitionAsync(int fromStatus, int toStatus)
    {
        if ((!_appSettings.MappedIntegrationConfiguration?.IsEnabled ?? true)
            || (_appSettings.MappedIntegrationConfiguration?.IsReadOnly ?? true))
            return true;

        var validStatusTransition = await GetNextValidStatusAsync(fromStatus);
        return validStatusTransition.Contains(toStatus);
    }

    /// <summary>
    /// Retrieves the list of valid next status options for the given 'currentStatus'.
    /// </summary>
    /// <param name="currentStatus">The current status of the ticket.</param>
    /// <returns>A list of valid next status options. Returns <c>null</c> if no Mapped is not enabled</returns>
    /// <remarks>
    /// Note that the method will return <c>null</c> rather than an empty list if Mapped is not enabled
    /// </remarks>
    public async Task<List<int>> GetNextValidStatusAsync(int currentStatus)
    {
        if ((!_appSettings.MappedIntegrationConfiguration?.IsEnabled ?? true)
            || (_appSettings.MappedIntegrationConfiguration?.IsReadOnly ?? true))
        {
            return null;
        }
        var validStatusTransition = await _workflowContext.TicketStatusTransitions
                                              .Where(x => x.FromStatus == currentStatus)
                                              .Select(x => x.ToStatus)
                                              .ToListAsync();

        return validStatusTransition;
    }
    /// <summary>
    /// Retrieves the list of status transitions.
    /// </summary>
    /// <returns></returns>
    public async Task<TicketStatusTransitionsDto> GetTicketStatusTransitionsAsync()
    {
        var ticketStatusTransitions = await _workflowContext.TicketStatusTransitions
            .Select(x => new TicketStatusTransition(x.FromStatus, x.ToStatus))
            .ToListAsync();

        return new TicketStatusTransitionsDto
        {
            TicketStatusTransitionList = ticketStatusTransitions
        };
    }
}

