using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Entities;
using WorkflowCore.Models;

namespace WorkflowCore.Services;

public interface ITicketSubStatusService
{
    Task<List<TicketSubStatus>> GetTicketSubStatusAsync();
}

public class TicketSubStatusService : ITicketSubStatusService
{
    private readonly WorkflowContext _workflowContext;

    public TicketSubStatusService(WorkflowContext workflowContext)
    {
        _workflowContext = workflowContext;
    }

    public async Task<List<TicketSubStatus>> GetTicketSubStatusAsync()
    {
        var subStatus = await _workflowContext.TicketSubStatus.ToListAsync();
        return TicketSubStatusEntity.MapTo(subStatus);
    }
}

