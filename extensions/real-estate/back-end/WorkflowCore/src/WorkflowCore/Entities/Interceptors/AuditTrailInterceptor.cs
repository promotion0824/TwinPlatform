using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkflowCore.Services;

namespace WorkflowCore.Entities.Interceptors;

public class AuditTrailInterceptor : SaveChangesInterceptor
{
	private readonly ISessionService _sessionService;

	public AuditTrailInterceptor(ISessionService sessionService)
	{
		_sessionService = sessionService;
	}

	public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
	{
		TrackChanges(eventData);
		return base.SavingChangesAsync(eventData, result, cancellationToken);
	}

	public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
	{
		TrackChanges(eventData);
		return base.SavingChanges(eventData, result);
	}

	private void TrackChanges(DbContextEventData eventData)
	{

		var entries = eventData.Context.ChangeTracker.Entries<IAuditTrail>()
											.Where(x => x.State == EntityState.Added
													 || x.State == EntityState.Modified
													 || x.State == EntityState.Deleted);
		var auditTrails = new List<AuditTrailEntity>();
		foreach (var entry in entries)
		{
			var entityName = entry.Entity.GetType().Name;

			var props = entry.Properties.Where(x => (x.IsModified || entry.State == EntityState.Added)
												&& (entry.Entity.GetTrackedColumns()?.Contains(x.Metadata.Name) ?? false));
			// for each change , all the properties tracked at the same time
			var trackDate = DateTime.UtcNow;

			foreach (var prop in props)
			{
                // only track values that are changed
                var oldValue = entry.State == EntityState.Added ? null : prop.OriginalValue?.ToString();
                var newValue = prop.CurrentValue?.ToString() ?? "";

                if (oldValue != newValue)
                {
                    var auditTrail = new AuditTrailEntity
                    {
                        RecordID = entry.Entity.Id,
                        OperationType = entry.State.ToString(),
                        Timestamp = trackDate,
                        TableName = entityName,
                        ColumnName = prop.Metadata?.Name,
                        SourceType = _sessionService.SourceType,
                        SourceId = _sessionService.SourceId,
                        OldValue = oldValue,
                        NewValue = newValue
                    };
                    auditTrails.Add(auditTrail);
                }
               

			}

		}
		eventData.Context.Set<AuditTrailEntity>().AddRange(auditTrails);
	}
}

