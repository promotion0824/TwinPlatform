using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Common;
using WorkflowCore.Entities;
using WorkflowCore.Models;

namespace WorkflowCore.Repository;

public interface IAuditTrailRepository
{
	Task<List<TicketActivity>> GetInsightTicketActivitiesAsync(Guid insightId);
    Task<List<AuditTrailEntity>> GetAuditTrailsAsync(string tableName, string columnName, Guid recordId);
}
public class AuditTrailRepository : IAuditTrailRepository
{
	private readonly WorkflowContext _context;

	public AuditTrailRepository(WorkflowContext context)
	{
		_context = context;
	}

	public async Task<List<TicketActivity>> GetInsightTicketActivitiesAsync(Guid insightId)
	{
		var activity = new List<TicketActivity>();
        var insightTickets = await _context.Tickets
                                                .Include(x => x.Attachments)
                                                .Where(c => c.InsightId.HasValue && c.InsightId.Value == insightId)
                                                .Select(x => new
                                                {
                                                    TicketId = x.Id,
                                                    Summary = x.Summary,
                                                    AttachmentIds = x.Attachments.Select(x => x.Id)
                                                })
                                                .ToListAsync();

        if (insightTickets.Any())
		{
			var trackedActivities = new string[] { EntityState.Added.ToString(), EntityState.Modified.ToString() };
			var ticketIds = insightTickets.Select(x => x.TicketId).ToList();
			var attachmentIds = insightTickets.SelectMany(x => x.AttachmentIds).ToList();

			var ticketAuditTrails = _context.AuditTrails.Where(x => ticketIds.Contains(x.RecordID) && x.TableName == nameof(TicketEntity));
			var attachmentAuditTrails = _context.AuditTrails.Where(x => attachmentIds.Contains(x.RecordID) && x.TableName == nameof(AttachmentEntity));

			var auditTrails = await ticketAuditTrails.Union(attachmentAuditTrails)
													 .Where(x => trackedActivities.Contains(x.OperationType)
															&& x.SourceId.HasValue
															&& x.SourceType.HasValue)
													.ToListAsync();

			var ticketActivity = auditTrails.Where(x => x.TableName == nameof(TicketEntity)).GroupBy(x => new
			{
				x.RecordID,
				x.Timestamp,
				x.SourceType,
				x.SourceId,
				x.OperationType
			})
						.Select(x => new TicketActivity
						{
							TicketId = x.Key.RecordID,
							ActivityDate = x.Key.Timestamp,
							ActivityType = GetTicketActivityType(x.Key.OperationType),
							Activities = x.Select(MapActivity).ToList(),
							SourceId = x.Key.SourceId.Value,
							SourceType = (SourceType)x.Key.SourceType,
                            TicketSummary = insightTickets.Where(y => y.TicketId == x.Key.RecordID).Select(y => y.Summary).FirstOrDefault()
						}).ToList();

			activity.AddRange(ticketActivity);


			var attachmentActivity = auditTrails.Where(x => x.TableName == nameof(AttachmentEntity)).GroupBy(x => new
			{
				x.RecordID,
				x.Timestamp,
				x.SourceType,
				x.SourceId,
				x.OperationType
			})
					.Select(x => new TicketActivity
					{
						TicketId = insightTickets.Where(y => y.AttachmentIds.Contains(x.Key.RecordID)).Select(y => y.TicketId).FirstOrDefault(),
						ActivityDate = x.Key.Timestamp,
						ActivityType = TicketActivityType.TicketAttachment,
						Activities = x.Select(MapActivity).ToList(),
						SourceId = x.Key.SourceId.Value,
						SourceType = (SourceType)x.Key.SourceType,
                        TicketSummary = insightTickets.Where(y => y.AttachmentIds.Contains(x.Key.RecordID)).Select(y => y.Summary).FirstOrDefault()
					}).ToList();

			activity.AddRange(attachmentActivity);
		}



		return activity;
	}

    public async Task<List<AuditTrailEntity>> GetAuditTrailsAsync(string tableName, string columnName, Guid recordId)
    {
        var auditTrails = await _context.AuditTrails.Where(x => x.RecordID == recordId
                                                             && x.ColumnName == columnName
                                                             && x.TableName == tableName)
                                                    .ToListAsync();

        return auditTrails;
    }

    private KeyValuePair<string, string> MapActivity(AuditTrailEntity entity)
	{

		if (entity.ColumnName == nameof(TicketEntity.Status) && int.TryParse(entity.NewValue, out var statusInt))
		{
			var status = Enum.GetName(typeof(TicketStatusEnum), statusInt) ?? "";
			return new KeyValuePair<string, string>(entity.ColumnName, status);

		}
		else
		{
			return new KeyValuePair<string, string>(entity.ColumnName, entity.NewValue);
		}
	}

	private TicketActivityType GetTicketActivityType(string operationType)
	{
		return operationType == EntityState.Added.ToString() ?
											TicketActivityType.NewTicket : TicketActivityType.TicketModified;
	}
}

