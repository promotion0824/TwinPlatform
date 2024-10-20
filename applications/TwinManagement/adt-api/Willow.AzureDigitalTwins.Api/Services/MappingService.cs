using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Diagnostic;
using Willow.AzureDigitalTwins.Api.Helpers;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.Model.Mapping;
using Willow.AzureDigitalTwins.Api.Persistence.Models.Mapped;
using System.Linq.Expressions;
using Willow.AzureDigitalTwins.Api.Model.Response.Mapped;
using Willow.Batch;

namespace Willow.AzureDigitalTwins.Api.Services;

public interface IMappingService
{
    public Task<MappedEntry> CreateMappedEntry(CreateMappedEntry entry);

    public Task<MappedEntry> UpdateMappedEntry(UpdateMappedEntry entry);

    public Task<int> UpdateMappedEntryStatus(UpdateMappedEntryStatusRequest request);

    public Task<int> UpdateAllMappedEntryStatus(MappedEntryAllRequest request, Status status);

    public Task<MappedEntryResponse> GetMappedEntriesAsync(MappedEntryRequest request);

    public Task<MappedEntry> GetMappedEntry(string mappedId);

    public Task<List<MappedEntriesGroupCount>> GetGroupedMappedEntriesCountAsync(string fieldName, Status? status);

    public Task DeleteMappedEntry(MappedEntry mappedEntry);

    public Task<int> DeleteBulk(IEnumerable<string> mappedIds);

    public Task<int> DeleteAll(MappedEntryAllRequest request);

    public Task<int> GetMappedEntriesCountAsync(IEnumerable<Status> status, string[]? prefixToMatchId = null, bool? excludePrefixes = false);

    public Task<UpdateMappedTwinRequest> CreateUpdateTwinRequest(string willowTwinId, List<JsonPatchOperation> jsonPatch);

    public Task<UpdateMappedTwinRequest> UpdateTwinUpdateRequest(Guid id, List<JsonPatchOperation> jsonPatch);

    public Task<int> GetUpdateTwinRequestsCountAsync();

    public Task<UpdateMappedTwinRequest> UpsertUpdateTwinRequest(string willowTwinId, List<JsonPatchOperation> jsonPatch);

    public Task<IAsyncEnumerable<UpdateMappedTwinRequestResponse>> GetUpdateTwinRequestsAsync(int offset, int pageSize);

    public Task<int> DeleteBulkUpdateTwinRequests(IEnumerable<Guid> ids);

    public Task<int> DeleteAllUpdateTwinRequests();
}

public class MappingService : IMappingService
{
    private readonly MappingContext _context;
    private readonly HealthCheckSqlServer _healthCheckSqlServer;
    private readonly ILogger<MappingService> _logger;
    private readonly ITelemetryCollector _telemetryCollector;
    public MappingService(MappingContext context, HealthCheckSqlServer healthCheckMappingDb, ITelemetryCollector telemetryCollector, ILogger<MappingService> logger)
    {
        _context = context;
        _healthCheckSqlServer = healthCheckMappingDb;
        _telemetryCollector = telemetryCollector;
        _logger = logger;
    }

    public async Task<MappedEntry> CreateMappedEntry(CreateMappedEntry entry)
    {
        var mappedEntity = new MappedEntry()
        {
            MappedId = entry.MappedId,
            MappedModelId = entry.MappedModelId,
            WillowModelId = entry.WillowModelId,
            ParentMappedId = entry.ParentMappedId,
            ParentWillowId = entry.ParentWillowId,
            WillowParentRel = entry.WillowParentRel,
            Name = entry.Name,
            Description = entry.Description,
            ModelInformation = entry.ModelInformation,
            StatusNotes = entry.StatusNotes,
            Status = entry.Status,
            AuditInformation = entry.AuditInformation,
            TimeCreated = DateTimeOffset.UtcNow,
            TimeLastUpdated = DateTimeOffset.UtcNow,
            ConnectorId = entry.ConnectorId,
            WillowId = entry.WillowId,
            BuildingId = entry.BuildingId,
            Unit = entry.Unit,
            DataType = entry.DataType,
        };

        await _context.AddAsync(mappedEntity);
        await _context.SaveChangesAsync();
        _telemetryCollector.TrackCreateMappedEntity(1);
        return mappedEntity;
    }

    public async Task<MappedEntry> UpdateMappedEntry(UpdateMappedEntry entry)
    {
        var entity = await _context.MappedEntries.Where(m => m.MappedId == entry.MappedId).FirstOrDefaultAsync();

        if (entity == null)
            return null;

        entity.MappedId = entry.MappedId;
        entity.MappedModelId = entry.MappedModelId;
        entity.WillowModelId = entry.WillowModelId;
        entity.ParentMappedId = entry.ParentMappedId;
        entity.ParentWillowId = entry.ParentWillowId;
        entity.WillowParentRel = entry.WillowParentRel;
        entity.Name = entry.Name;
        entity.Description = entry.Description;
        entity.ModelInformation = entry.ModelInformation;
        entity.StatusNotes = entry.StatusNotes;
        entity.Status = entry.Status;
        entity.AuditInformation = entry.AuditInformation;
        entity.TimeLastUpdated = DateTimeOffset.UtcNow;
        entity.ConnectorId = entry.ConnectorId;
        entity.WillowId = entry.WillowId;
        entity.IsExistingTwin = entry.IsExistingTwin;
        entity.BuildingId = entry.BuildingId;
        entity.Unit = entry.Unit;
        entity.DataType = entry.DataType;

        _context.Update(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    public async Task<int> UpdateMappedEntryStatus(UpdateMappedEntryStatusRequest request)
    {
        return await _context.MappedEntries
            .Where(m => request.MappedIds.Contains(m.MappedId))
            .ExecuteUpdateAsync(m => m.SetProperty(e => e.Status, e => request.Status));
    }

    /// <summary>
    /// Update the status of all mapped entries based on MappedEntryAllRequest
    /// </summary>
    public async Task<int> UpdateAllMappedEntryStatus(MappedEntryAllRequest request, Status status)
    {
        var query = _context.MappedEntries.AsQueryable();

        query = AppendPrefixesFilter(query, request.prefixToMatchId, request.excludePrefixes);

        if (request.statuses != null && request.statuses.Count != 0)
        {
            query = query.Where(m => request.statuses.Contains(m.Status));
        }

        if (request.buildingIds != null && request.buildingIds.Length != 0)
        {
            query = query.Where(m => request.buildingIds.Contains(m.BuildingId));
        }

        if (!string.IsNullOrEmpty(request.connectorId))
        {
            query = query.Where(m => m.ConnectorId == request.connectorId);
        }

        return await query.ExecuteUpdateAsync(m => m.SetProperty(e => e.Status, e => status));
    }

    public async Task<MappedEntry> GetMappedEntry(string mappedId)
    {
        var entry = await _context.MappedEntries.Where(m => m.MappedId == mappedId).FirstOrDefaultAsync();

        return entry;
    }

    public async Task<MappedEntryResponse> GetMappedEntriesAsync(MappedEntryRequest request)
    {
        try
        {
            IQueryable<MappedEntry> query = _context.MappedEntries.AsQueryable();

            // Apply Prefix Filters
            query = AppendPrefixesFilter(query, request.prefixToMatchId, request.excludePrefixes);

            // Apply Filter Specs
            query = query.FilterBy(request.FilterSpecifications).AsNoTracking();

            var entries =
                query
                .OrderByDescending(m => m.TimeLastUpdated)
                .Skip(request.offset)
                .Take(request.pageSize);

            var items = entries.ToList();

            var total = await query
                .CountAsync();


            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSqlServer, HealthCheckSqlServer.Healthy, _logger);

            return new MappedEntryResponse(total, items);
        }
        catch
        {
            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSqlServer, HealthCheckSqlServer.FailingCalls, _logger);

            throw;
        }
    }

    /// <summary>
    ///  Sample query for prefixes filter:
    ///  SELECT * FROM dbo.MappedEntries WHERE MappedId LIKE 'SITE%' OR MappedId LIKE 'FLR%' OR MappedId LIKE 'BLDG%' OR MappedId LIKE 'ZONE%' OR MappedId LIKE 'SPC%'
    /// </summary>
    private IQueryable<MappedEntry> AppendPrefixesFilter(IQueryable<MappedEntry> query, string[]? prefixToMatchId, bool? excludePrefixes = false)
    {
        if (prefixToMatchId == null || !prefixToMatchId.Any())
            return query;

        // TODO: We could use Mapped's ontology to determine the type of Mapped twin.

        // Easy way of determining which Mapped twins are "Things", "Points", and "Spaces"
        // The prefixes on the mappedId determine the type of Mapped twin.
        // Things: 'THG'
        // Points: 'PNT'
        // Spaces: 'SPC', 'FLR', 'BLDG', 'SITE'

        // Create the WHERE clause dynamically based on the list of prefixes
        var whereClauses = prefixToMatchId.Select(prefix => $"MappedId {((bool)excludePrefixes ? "NOT" : "")} LIKE '{prefix}%'");
        var whereClause = string.Join($" {((bool)excludePrefixes ? "AND" : "OR")} ", whereClauses);

        // Build the complete SQL statement
        string sqlQuery = $"SELECT * FROM dbo.MappedEntries WHERE {whereClause}";
        var sqlStatement = FormattableStringFactory.Create(sqlQuery);

        query = _context.MappedEntries.FromSql(sqlStatement);

        return query;
    }

    public async Task<List<MappedEntriesGroupCount>> GetGroupedMappedEntriesCountAsync(string fieldName, Status? status)
    {
        IQueryable<MappedEntry> query = _context.MappedEntries;

        // Apply status filter if provided
        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        // Create the dynamic expression for grouping
        var parameter = Expression.Parameter(typeof(MappedEntry));
        var property = Expression.Property(parameter, fieldName);
        var lambda = Expression.Lambda<Func<MappedEntry, string>>(Expression.Convert(property, typeof(string)), parameter);
        // Create the condition for filtering: property != null && property != ""
        var nullCheck = Expression.NotEqual(property, Expression.Constant(null));
        var emptyCheck = Expression.NotEqual(property, Expression.Constant(string.Empty));
        var condition = Expression.AndAlso(nullCheck, emptyCheck);

        var lambdaCondition = Expression.Lambda<Func<MappedEntry, bool>>(condition, parameter);

        // Group by the dynamic field and select the count
        var groupedData = await query
            .Where(lambdaCondition)
            .GroupBy(lambda)
            .Select(g => new MappedEntriesGroupCount
            {
                Key = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(dto => dto.Count) // Sort by count in descending order
            .ToListAsync();

        return groupedData;
    }

    public async Task DeleteMappedEntry(MappedEntry mappedEntry)
    {
        _ = _context.MappedEntries.Remove(mappedEntry);
        await _context.SaveChangesAsync();
    }

    public async Task<int> DeleteBulk(IEnumerable<string> mappedIds)
    {
        return await _context.MappedEntries.Where(m => mappedIds.Contains(m.MappedId)).ExecuteDeleteAsync();
    }

    /// <summary>
    ///  Delete all mapped entries based on MappedEntryAllRequest
    /// </summary>
    public async Task<int> DeleteAll(MappedEntryAllRequest request)
{
    var query = _context.MappedEntries.AsQueryable();

    query = AppendPrefixesFilter(query, request.prefixToMatchId, request.excludePrefixes);

    if (request.statuses != null && request.statuses.Count != 0)
    {
        query = query.Where(m => request.statuses.Contains(m.Status));
    }

    if (request.buildingIds != null && request.buildingIds.Length != 0)
    {
        query = query.Where(m => request.buildingIds.Contains(m.BuildingId));
    }

    if (!string.IsNullOrEmpty(request.connectorId))
    {
        query = query.Where(m => m.ConnectorId == request.connectorId);
    }

    return await query.ExecuteDeleteAsync();
}

    /// <summary>
    ///  Get count of mapped entries
    /// </summary>
    /// <param name="statuses">query based on mapped entries' status</param>
    /// <param name="prefixToMatchId">Prefixes to match with the first few characters of Mapped Id</param>
    /// <param name="excludePrefixes">Exclude records where prefixes match with the first few characters of Mapped Id</param>
    /// <returns>Return counts based on query</returns>
    public async Task<int> GetMappedEntriesCountAsync(IEnumerable<Status> statuses, string[]? prefixToMatchId = null, bool? excludePrefixes = false)
    {
        IQueryable<MappedEntry> query = _context.MappedEntries;

        query = prefixToMatchId?.Any() == true ? AppendPrefixesFilter(query, prefixToMatchId, excludePrefixes) : query;
        query = statuses?.Any() == true ? query.Where(e => statuses.Contains(e.Status)) : query;


        return query.Count();
    }

    /// <summary>
    /// Create twin update request
    /// </summary>
    /// <param name="willowTwinId">Willow twin id</param>
    /// <param name="jsonPatch">json patch</param>
    /// <returns> created twin update request</returns>
    public async Task<UpdateMappedTwinRequest> CreateUpdateTwinRequest(string willowTwinId, List<JsonPatchOperation> jsonPatch)
    {
        var updateTwinRequest = new UpdateMappedTwinRequest()
        {
            WillowTwinId = willowTwinId,
            ChangedProperties = JsonSerializer.Serialize(jsonPatch),
            TimeCreated = DateTimeOffset.UtcNow,
            TimeLastUpdated = DateTimeOffset.UtcNow,
        };

        await _context.AddAsync(updateTwinRequest);
        await _context.SaveChangesAsync();
        _telemetryCollector.TrackCreateUpdateTwinRequest(1);
        return updateTwinRequest;
    }


    /// <summary>
    ///  Get count of twin update requests.
    /// </summary>
      public async Task<int> GetUpdateTwinRequestsCountAsync()
    {
        IQueryable<UpdateMappedTwinRequest> query = _context.UpdateMappedTwinRequest;

        return query.Count();
    }

    /// <summary>
    /// Update twin update request
    /// </summary>
    /// <param name="id">id of twin update request</param>
    /// <param name="jsonPatch">json patch</param>
    /// <returns> updated twin update request</returns>
    public async Task<UpdateMappedTwinRequest> UpdateTwinUpdateRequest(Guid id, List<JsonPatchOperation> jsonPatch)
    {
        var entity = await _context.UpdateMappedTwinRequest.FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return null;

        entity.ChangedProperties = JsonSerializer.Serialize(jsonPatch);
        entity.TimeLastUpdated = DateTimeOffset.UtcNow;

        _context.Update(entity);
        await _context.SaveChangesAsync();
        _telemetryCollector.TrackUpdateTwinUpdateRequestCount(1);

        return entity;
    }

    /// <summary>
    /// Upsert update twin request. If record with willowTwinId exists, update it. Otherwise, create a new record.
    /// </summary>
    /// <param name="willowTwinId">Willow twin id</param>
    /// <param name="jsonPatch">json patch</param>
    /// <returns> twin update request</returns>
    public async Task<UpdateMappedTwinRequest> UpsertUpdateTwinRequest(string willowTwinId, List<JsonPatchOperation> jsonPatch)
    {
        var entity = await _context.UpdateMappedTwinRequest.FirstOrDefaultAsync(x => x.WillowTwinId == willowTwinId);

        if (entity == null)
        {
            // Record with willowTwinId does not exist, create a new record.
            var updateTwinRequest = new UpdateMappedTwinRequest()
            {
                WillowTwinId = willowTwinId,
                ChangedProperties = JsonSerializer.Serialize(jsonPatch),
                TimeCreated = DateTimeOffset.UtcNow,
                TimeLastUpdated = DateTimeOffset.UtcNow,
            };
            await _context.AddAsync(updateTwinRequest);
            await _context.SaveChangesAsync();
            _telemetryCollector.TrackCreateUpdateTwinRequest(1);
            return updateTwinRequest;

        }
        else
        {
            // Record with willowTwinId doesn't exist, create new record.
            entity.ChangedProperties = JsonSerializer.Serialize(jsonPatch);
            entity.TimeLastUpdated = DateTimeOffset.UtcNow;
            _context.Update(entity);
            await _context.SaveChangesAsync();
            _telemetryCollector.TrackUpdateTwinUpdateRequestCount(1);
            return entity;
        }
    }

    /// <summary>
    /// Get update twin requests
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public async Task<IAsyncEnumerable<UpdateMappedTwinRequestResponse>> GetUpdateTwinRequestsAsync(int offset, int pageSize)
    {
        try
        {
            IQueryable<UpdateMappedTwinRequest> query = _context.UpdateMappedTwinRequest;

            var entries = query
            .OrderByDescending(m => m.TimeLastUpdated)
            .Skip(offset)
            .Take(pageSize)
            .AsAsyncEnumerable();

            var response = entries.Select(x => new UpdateMappedTwinRequestResponse
            {
                Id = x.Id,
                WillowTwinId = x.WillowTwinId,
                ChangedProperties = JsonSerializer.Deserialize<List<JsonPatchOperation>>(x.ChangedProperties),
                TimeCreated = x.TimeCreated,
                TimeLastUpdated = x.TimeLastUpdated
            });

            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSqlServer, HealthCheckSqlServer.Healthy, _logger);

            return response;
        }
        catch
        {
            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSqlServer, HealthCheckSqlServer.FailingCalls, _logger);

            throw;
        }
    }

    /// <summary>
    ///  Delete update twin request based on id
    /// </summary>
    /// <param name="ids">List of updated twin request id</param>
    public async Task<int> DeleteBulkUpdateTwinRequests(IEnumerable<Guid> ids)
    {
        return await _context.UpdateMappedTwinRequest.Where(x => ids.Contains(x.Id)).ExecuteDeleteAsync();
    }

    /// <summary>
    ///  Delete all update twins requests
    /// </summary>
    public async Task<int> DeleteAllUpdateTwinRequests()
    {
        return await _context.UpdateMappedTwinRequest.ExecuteDeleteAsync();
    }
}
