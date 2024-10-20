namespace ConnectorCore.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectorCore.Data;
using ConnectorCore.Data.Models;
using ConnectorCore.Entities;
using ConnectorCore.Entities.Validators;
using ConnectorCore.Models;
using ConnectorCore.Repositories;
using LazyCache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

internal class ScanService(
    IConnectorCoreDbContext dbContext,
    IAppCache appCache,
    IOptions<CacheOptions> cacheOptions,
    IJsonSchemaValidator jsonSchemaValidator)
    : IScanService
{
    private const string ScansCacheKey = "connectorScans-{0}";
    private readonly int cacheExpirationMinutes = cacheOptions.Value.ScansCacheTimeoutInMinutes;

    private async Task ValidateScanAsync(ScanEntity scan)
    {
        var connector = await dbContext.Connectors.FirstOrDefaultAsync(x => x.Id == scan.ConnectorId);
        var connectorType = await dbContext.ConnectorTypes.FirstOrDefaultAsync(x => x.Id == connector.ConnectorTypeId);
        if (connectorType.ScanConfigurationSchemaId != null)
        {
            var columns = await dbContext.SchemaColumns
                .Where(x => x.SchemaId == (Guid)connectorType.ScanConfigurationSchemaId)
                .Select(x => new SchemaColumnEntity
                {
                    DataType = x.DataType,
                    Id = x.Id,
                    IsRequired = x.IsRequired,
                    SchemaId = x.SchemaId,
                    UnitOfMeasure = x.UnitOfMeasure,
                }).ToListAsync();

            if (!jsonSchemaValidator.IsValid(columns, scan.Configuration, out var errors))
            {
                throw new ArgumentException("Scan's metadata should comply relevant schema: " + string.Join("\n", errors));
            }
        }
    }

    public async Task<ScanEntity> CreateAsync(ScanEntity scan)
    {
        await ValidateScanAsync(scan);

        await dbContext.Scans.AddAsync(new Scan
        {
            Configuration = scan.Configuration,
            ConnectorId = scan.ConnectorId,
            Message = scan.Message,
            Status = ((int)ScanStatus.New).ToString(),
            CreatedBy = scan.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            StartTime = scan.StartTime,
            EndTime = scan.EndTime,
            DevicesToScan = scan.DevicesToScan,
            ErrorCount = scan.ErrorCount,
            ErrorMessage = scan.ErrorMessage,
        });

        await dbContext.SaveChangesAsync();
        appCache.Remove(string.Format(ScansCacheKey, scan.ConnectorId.ToString()));

        return scan;
    }

    public async Task<List<ScanEntity>> GetByConnectorIdAsync(Guid connectorId)
    {
        var scans = await appCache.GetOrAddAsync(string.Format(ScansCacheKey, connectorId.ToString()),
            async cache =>
            {
                cache.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(cacheExpirationMinutes));
                return await dbContext.Scans.Where(x => x.ConnectorId == connectorId).Select(x => new ScanEntity
                {
                    Configuration = x.Configuration,
                    ConnectorId = x.ConnectorId,
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy,
                    DevicesToScan = x.DevicesToScan,
                    EndTime = x.EndTime,
                    ErrorCount = x.ErrorCount,
                    ErrorMessage = x.ErrorMessage,
                    Id = x.Id,
                    StartTime = x.StartTime,
                    Status = (ScanStatus)Convert.ToInt32(x.Status),
                    Message = x.Message,
                }).ToListAsync();
            });

        return scans;
    }

    public async Task StopAsync(Guid connectorId, Guid scanId)
    {
        var scan = await dbContext.Scans.FirstOrDefaultAsync(x => x.Id == scanId && x.ConnectorId == connectorId);
        if (scan is null)
        {
            return;
        }

        scan.Status = ((int)ScanStatus.Finished).ToString();
        await dbContext.SaveChangesAsync();
        appCache.Remove(string.Format(ScansCacheKey, connectorId.ToString()));
    }

    public async Task<ScanEntity> GetById(Guid scanId)
    {
        var entity = await dbContext.Scans.FirstOrDefaultAsync(x => x.Id == scanId);
        if (entity is null)
        {
            return null;
        }

        return new ScanEntity
        {
            Configuration = entity.Configuration,
            ConnectorId = entity.ConnectorId,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            DevicesToScan = entity.DevicesToScan,
            EndTime = entity.EndTime,
            ErrorCount = entity.ErrorCount,
            ErrorMessage = entity.ErrorMessage,
            Id = entity.Id,
            StartTime = entity.StartTime,
            Status = (ScanStatus)Convert.ToInt32(entity.Status),
            Message = entity.Message,
        };
    }

    public async Task PatchAsync(Guid connectorId,
        Guid scanId,
        ScanStatus? status,
        string errorMessage,
        int? errorCount,
        DateTime? startTime,
        DateTime? endTime)
    {
        var entity = await dbContext.Scans.FirstOrDefaultAsync(x => x.Id == scanId && x.ConnectorId == connectorId);
        if (entity is null)
        {
            return;
        }

        entity.Status = status == null ? entity.Status : ((int)status).ToString();
        entity.ErrorMessage = !string.IsNullOrEmpty(errorMessage) ? errorMessage : entity.ErrorMessage;
        entity.ErrorCount = errorCount ?? entity.ErrorCount;
        entity.StartTime = startTime ?? entity.StartTime;
        entity.EndTime = endTime ?? entity.EndTime;
        await dbContext.SaveChangesAsync();
        appCache.Remove(string.Format(ScansCacheKey, connectorId.ToString()));
    }
}
