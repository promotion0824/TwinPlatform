using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Controllers.Responses;
using WorkflowCore.Models;
using WorkflowCore.Repository;

namespace WorkflowCore.Services;
public interface IAuditTrailService
{
	Task<List<TicketActivity>> GetInsightTicketActivitiesAsync(Guid insightId);
}
public class AuditTrailService : IAuditTrailService
{
	private readonly IAuditTrailRepository _auditTrailRepository;

	public AuditTrailService(IAuditTrailRepository auditTrailRepository)
	{
		_auditTrailRepository = auditTrailRepository;
	}

	public async Task<List<TicketActivity>> GetInsightTicketActivitiesAsync(Guid insightId)
	{
		return  await _auditTrailRepository.GetInsightTicketActivitiesAsync(insightId);
	
	}
}

