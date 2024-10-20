using LazyCache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WorkflowCore.Entities;
using WorkflowCore.Infrastructure.Configuration;
using WorkflowCore.Services.Apis;
using WorkflowCore.Services.Apis.Requests;
using WorkflowCore.Services.MappedIntegration.Dtos.Responses;
using WorkflowCore.Services.MappedIntegration.Interfaces;

namespace WorkflowCore.Services.MappedIntegration.Services;


public class MappedSyncMetadataService : IMappedSyncMetadataService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MappedSyncMetadataService> _logger;
    private readonly WorkflowContext _workflowContext;
    private readonly AppSettings _appSettings;
    private readonly IMappedApiService _mappedApiService;
    private readonly IDigitalTwinServiceApi _digitalTwinServiceApi;
    private readonly IAppCache _appCache;
    // this should be the name of the external id property in the twin
    private const string SpaceIdPropertyName = "spaceID";

    public MappedSyncMetadataService(IHttpClientFactory httpClientFactory,
                                     ILogger<MappedSyncMetadataService> logger,
                                     WorkflowContext workflowContext,
                                     IConfiguration configuration,
                                     IMappedApiService mappedApiService,
                                     IDigitalTwinServiceApi digitalTwinServiceApi,
                                     IAppCache appCache)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _workflowContext = workflowContext;
        _appSettings = configuration.Get<AppSettings>();
        _mappedApiService = mappedApiService;
        _digitalTwinServiceApi = digitalTwinServiceApi;
        _appCache = appCache;
    }

    public async Task SyncTicketMetadata()
    {

        var ticketMetadata = await _mappedApiService.GetTicketMetaDataAsync(_appSettings.MappedIntegrationConfiguration);
        if (ticketMetadata is null)
        {
            _logger.LogError("Ticket Metadata Sync : Failed to sync ticket metadata from Mti. Response is null");
            return;
        }

        var existingJobTypes = await _workflowContext.JobTypes.AsTracking().ToListAsync();
        var existingCategories = await _workflowContext.TicketCategories.AsTracking().ToListAsync();
        var existingServiceNeeded = await _workflowContext.ServiceNeeded.AsTracking().ToListAsync();
        var existingServiceNeededSpaceTwinList = await _workflowContext.ServiceNeededSpaceTwin.AsTracking().ToListAsync();

        // sync metadata
        await SyncJobTypes(existingJobTypes, ticketMetadata.JobTypes);
        await SyncCategories(existingCategories, ticketMetadata.RequestTypes);
        // we should sync service needed first before syncing service needed space twin
        await SyncServiceNeeded(existingServiceNeeded, ticketMetadata.ServiceNeededList);
        await SyncServiceNeededSpaceTwin(existingServiceNeededSpaceTwinList, ticketMetadata.SpaceServiceNeededList);

        _logger.LogInformation("Ticket Metadata Sync : Synced ticket metadata from Mapped completed");


    }


    #region private methods

    private async Task SyncJobTypes(List<JobTypeEntity> existingJobTypes, List<JobType> mappedJobTypes)
    {
        _logger.LogInformation("Ticket Metadata Sync : Syncing JobTypes from Mapped started");

        if (mappedJobTypes is null || mappedJobTypes.Count == 0)
        {
            _logger.LogInformation("Ticket Metadata Sync : Sync ticket metadata no Job types found from Mapped");
            return;
        }
        // remove duplicates
        mappedJobTypes = mappedJobTypes.DistinctBy(x => x.Id).ToList();
        var newJobTypes = new List<JobTypeEntity>();
        foreach (var item in mappedJobTypes)
        {
            var existingJobType = existingJobTypes.Where(x => x.Id == item.Id).FirstOrDefault();
            // add if not exists
            if (existingJobType is null)
            {
                newJobTypes.Add(new JobTypeEntity
                {
                    Id = item.Id,
                    Name = item.Name,
                    IsActive = true,
                    LastUpdate = DateTime.UtcNow
                });
            }
            // update name if different
            else
            {
                if (existingJobType.Name != item.Name)
                {
                    existingJobType.Name = item.Name;
                    existingJobType.LastUpdate = DateTime.UtcNow;
                }
                // activate if not active
                if (!existingJobType.IsActive)
                {
                    existingJobType.IsActive = true;
                    existingJobType.LastUpdate = DateTime.UtcNow;
                }
            }
        }
        // get exiting job type ids after the update
        var existingJobTypeIds = existingJobTypes.Union(newJobTypes).Select(x => x.Id).ToList();
        var mappedIds = mappedJobTypes.Select(x => x.Id).ToList();
        var jobTypeIdsToDeactivate = existingJobTypeIds.Where(x => !mappedIds.Contains(x)).ToList();
        foreach (var jobTypeId in jobTypeIdsToDeactivate)
        {
            // deacativate if exists and active
            var jobTypeToDeactivate = existingJobTypes.Where(x => x.Id == jobTypeId && x.IsActive).FirstOrDefault();
            if (jobTypeToDeactivate is not null)
            {
                jobTypeToDeactivate.IsActive = false;
                jobTypeToDeactivate.LastUpdate = DateTime.UtcNow;

            }
        }


        _logger.LogInformation("Ticket Metadata Sync : Adding new job types from Mapped: {JobTypes count}", newJobTypes.Count);
        await _workflowContext.JobTypes.AddRangeAsync(newJobTypes);
        await _workflowContext.SaveChangesAsync();
    }

    private async Task SyncCategories(List<TicketCategoryEntity> existingCategories, List<RequestType> mappedRequestTypes)
    {
        _logger.LogInformation("Ticket Metadata Sync : Syncing Categories from Mapped started");
        if (mappedRequestTypes is null || mappedRequestTypes.Count == 0)
        {
            _logger.LogInformation("Ticket Metadata Sync : Sync ticket metadata no Categories found from Mapped");
            return;
        }
        // remove duplicates
        mappedRequestTypes = mappedRequestTypes.DistinctBy(x => x.Id).ToList();
        var newCategories = new List<TicketCategoryEntity>();

        foreach (var item in mappedRequestTypes)
        {
            var existingCategory = existingCategories.Where(x => x.Id == item.Id).FirstOrDefault();
            if (existingCategory is null)
            {
                newCategories.Add(new TicketCategoryEntity
                {
                    Id = item.Id,
                    Name = item.Name,
                    IsActive = true,
                    LastUpdate = DateTime.UtcNow
                });
            }
            else
            {

                if (existingCategory.Name != item.Name)
                {
                    existingCategory.Name = item.Name;
                }
                // activate if not active   
                if (!existingCategory.IsActive)
                {
                    existingCategory.IsActive = true;
                    existingCategory.LastUpdate = DateTime.UtcNow;
                }
            }
        }

        var existingCategoryIds = existingCategories.Union(newCategories).Select(x => x.Id);
        var mappedRequestTypeIds = mappedRequestTypes.Select(x => x.Id);
        var categoryIdsToDeactivate = existingCategoryIds.Where(x => !mappedRequestTypeIds.Contains(x));
        foreach (var categoryId in categoryIdsToDeactivate)
        {
            // deacativate if exists and active
            var categoryToDeactivate = existingCategories.Where(x => x.Id == categoryId && x.IsActive).FirstOrDefault();
            if (categoryToDeactivate is not null)
            {
                categoryToDeactivate.IsActive = false;
                categoryToDeactivate.LastUpdate = DateTime.UtcNow;
            }
        }
        _logger.LogInformation("Ticket Metadata Sync : Adding new Categories from Mapped: {Categories count}", newCategories.Count);
        await _workflowContext.TicketCategories.AddRangeAsync(newCategories);
        await _workflowContext.SaveChangesAsync();
    }


    private async Task SyncServiceNeeded(List<ServiceNeededEntity> existingServiceNeeded, List<ServiceNeeded> mappedServiceNeeded)
    {
        _logger.LogInformation("Ticket Metadata Sync : Syncing ServiceNeeded from Mapped started");

        if (mappedServiceNeeded is null || mappedServiceNeeded.Count == 0)
        {
            _logger.LogInformation("Sync ticket metadata no ServiceNeeded found from Mapped");
            return;
        }
        // remove duplicates
        mappedServiceNeeded = mappedServiceNeeded.DistinctBy(x => x.Id).ToList();
        var newServiceNeeded = new List<ServiceNeededEntity>();

        foreach (var item in mappedServiceNeeded)
        {
            var existingService = existingServiceNeeded.Where(x => x.Id == item.Id).FirstOrDefault();
            if (existingService is null)
            {
                newServiceNeeded.Add(new ServiceNeededEntity
                {
                    Id = item.Id,
                    Name = item.Name,
                    CategoryId = item.RequestTypeId,
                    IsActive = true,
                    LastUpdate = DateTime.UtcNow
                });
            }
            else
            {
                if (existingService.Name != item.Name)
                {
                    existingService.Name = item.Name;
                }
                if (existingService.CategoryId != item.RequestTypeId)
                {
                    existingService.CategoryId = item.RequestTypeId;
                }
                // activate if not active
                if (!existingService.IsActive)
                {
                    existingService.IsActive = true;
                    existingService.LastUpdate = DateTime.UtcNow;
                }

            }
        }

        var existingServiceIds = existingServiceNeeded.Union(newServiceNeeded).Select(x => x.Id);
        var mappedServiceNeededIds = mappedServiceNeeded.Select(x => x.Id);
        var serviceIdsToDeactivate = existingServiceIds.Where(x => !mappedServiceNeededIds.Contains(x));
        foreach (var serviceId in serviceIdsToDeactivate)
        {
            // deacativate if exists and active
            var serviceToDeactivate = existingServiceNeeded.Where(x => x.Id == serviceId && x.IsActive).FirstOrDefault();
            if (serviceToDeactivate is not null)
            {
                serviceToDeactivate.IsActive = false;
                serviceToDeactivate.LastUpdate = DateTime.UtcNow;
            }
        }
        _logger.LogInformation("Ticket Metadata Sync : Adding new ServiceNeeded from Mapped: {ServiceNeeded count}", newServiceNeeded.Count);
        await _workflowContext.ServiceNeeded.AddRangeAsync(newServiceNeeded);
        await _workflowContext.SaveChangesAsync();
    }

    private async Task SyncServiceNeededSpaceTwin(List<ServiceNeededSpaceTwinEntity> existingServiceNeededSpaceTwinList, List<SpaceServiceNeeded> mappedSpaceServiceNeeded)
    {
        _logger.LogInformation("Ticket Metadata Sync : Syncing ServiceNeededSpaceTwin from Mapped started");
        if (mappedSpaceServiceNeeded is null || mappedSpaceServiceNeeded.Count == 0)
        {
            _logger.LogInformation("Ticket Metadata Sync : Sync ticket metadata no ServiceNeededSpaceTwin found from Mapped");
            return;
        }

        // this service needed list should be updated list after the sync
        var existingServiceNeededList = await _workflowContext.ServiceNeeded.ToListAsync();
        // remove hyphens from space id guid   
        var spaceIds = mappedSpaceServiceNeeded.Select(x => x.SpaceId.ToString("N")).ToList();
        var twinIdDict = await GetTwinIdBySpaceIds(spaceIds);
        foreach (var item in mappedSpaceServiceNeeded)
        {

            var spaceId = item.SpaceId.ToString("N");
            var twinId = twinIdDict.GetValueOrDefault(spaceId);

            if (string.IsNullOrWhiteSpace(twinId))
            {
                _logger.LogWarning("Ticket Metadata Sync : Failed to get twin id for space id: {SpaceId}", spaceId);
                continue;
            }
            var newServiceNeededSpaceTwinEntityList = new List<ServiceNeededSpaceTwinEntity>();
            foreach (var serviceNeededId in item.ServiceNeededIds)
            {
                var existingServiceSpaceTwin = existingServiceNeededSpaceTwinList.Where(x => x.SpaceTwinId == twinId && x.ServiceNeededId == serviceNeededId).FirstOrDefault();
                var existingServiceNeeded = existingServiceNeededList.Where(x => x.Id == serviceNeededId).FirstOrDefault();
                if (existingServiceSpaceTwin is null && existingServiceNeeded is not null)
                {

                    newServiceNeededSpaceTwinEntityList.Add(new ServiceNeededSpaceTwinEntity
                    {
                        Id = Guid.NewGuid(),
                        SpaceTwinId = twinId,
                        ServiceNeededId = serviceNeededId,
                        LastUpdate = DateTime.UtcNow
                    });
                }
            }

            // remove existing serviceNeededSpaceTwin that are not in the mapped serviceNeeded for this twin id list
            var existingSpaceTwinServiceNeeded = existingServiceNeededSpaceTwinList.Union(newServiceNeededSpaceTwinEntityList)
                                                                                   .Where(x => x.SpaceTwinId == twinId)
                                                                                   .ToList();

            // get the service needed ids for the current twin id
            var mappedServiceNeededIdsForCurrentTwin = mappedSpaceServiceNeeded.Where(x => x.SpaceId == item.SpaceId)
                                                                               .SelectMany(x => x.ServiceNeededIds)
                                                                               .ToList();
            // get the service needed ids that no longer exists in mapped service needed for this twin id
            var spaceServiceNeededToRemove = existingSpaceTwinServiceNeeded.Where(x => !mappedServiceNeededIdsForCurrentTwin.Contains(x.ServiceNeededId))
                                                                           .ToList();


            _workflowContext.ServiceNeededSpaceTwin.RemoveRange(spaceServiceNeededToRemove);
            await _workflowContext.AddRangeAsync(newServiceNeededSpaceTwinEntityList);
            await _workflowContext.SaveChangesAsync();

        }

    }

    private async Task<Dictionary<string, string>> GetTwinIdBySpaceIds(List<string> spaceIds)
    {
        // remove duplicates
        spaceIds = spaceIds.Distinct().ToList();

        var spaceTwinsDict = new Dictionary<string, string>();
        var request = new GetBuildingTwinsByExternalIdsRequest
        {
            ExternalIdName = SpaceIdPropertyName,
            ExternalIdValues = spaceIds
        };
        // sort the space ids to create a unique cache key
        var sortedSpaceIds = spaceIds.OrderBy(x => x).ToList();
        var cacheKey = $"GetBuildingTwinsByExternalIdsRequest_{string.Join("", sortedSpaceIds)}";
        spaceTwinsDict = await _appCache.GetOrAddAsync(cacheKey, async (cache) =>
         {
             cache.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
             var buildingTwins = await _digitalTwinServiceApi.GetBuildingTwinsByExternalIds(request);
             if (buildingTwins is null || buildingTwins.Count == 0)
             {
                 _logger.LogWarning("Ticket Metadata Sync :  Failed to get building twins by external ids");
                 return spaceTwinsDict;
             }

             foreach (var item in buildingTwins)
             {
                 if (item.ExternalIds.Any())
                 {
                     var externalIdValue = (JObject)item.ExternalIds.First().Value;
                     if (externalIdValue is not null)
                     {
                         var spaceId = externalIdValue.GetValue(SpaceIdPropertyName)?.ToString();
                         spaceTwinsDict.TryAdd(spaceId, item.TwinId);
                     }
                 }
             }

             return spaceTwinsDict;

         });

        return spaceTwinsDict;
    }
    #endregion
}

