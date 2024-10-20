using Microsoft.Extensions.Caching.Memory;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Willow.Api.Client;
using Willow.Common;
using Willow.Platform.Users;
using Willow.Workflow;

namespace PlatformPortalXL.Services
{
    public interface IInspectionService
    {
        Task<List<InspectionDto>> EnrichInspections(Guid siteId, List<Inspection> inspections);
        Task<InspectionDto> EnrichInspection(Guid siteId, Inspection inspection);
        Task<InspectionUsageDto> GetInspectionUsageBySiteId(Guid siteId, InspectionUsagePeriod period);
        Task ArchiveInspection(Guid siteId, Guid inspectionId, bool isArchived);
        Task ArchiveZone(Guid siteId, Guid zoneId, bool isArchived);
        Task UpdateSortOrder(Guid siteId, Guid zoneId, UpdateInspectionSortOrderRequest request);
    }
    public class InspectionService : IInspectionService
    {
        private readonly IDigitalTwinAssetService _digitalTwinService;
        private readonly IWorkflowApiService _workflowApi;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserService _userService;

        public InspectionService(
            IUserService userService,
            IWorkflowApiService workflowApi,
            IDigitalTwinAssetService digitalTwinService,
            IDateTimeService dateTimeService)
        {
            _userService = userService;
            _workflowApi = workflowApi;
            _digitalTwinService = digitalTwinService;
            _dateTimeService = dateTimeService;
        }

        public async Task<List<InspectionDto>> EnrichInspections(Guid siteId, List<Inspection> inspections)
        {
            var inspectionDtos = InspectionDto.MapFromModels(inspections);
            var siteZones = await _workflowApi.GetInspectionZones(siteId, false);
            var inspectionAssetIds = inspectionDtos.Select(i => i.AssetId).Distinct();
            var assetNames = await GetAssetNames(siteId, inspectionAssetIds);

            foreach (var dto in inspectionDtos)
            {
                dto.ZoneName = siteZones.First(z => z.Id == dto.ZoneId).Name;

                try
                {
                    dto.AssignedWorkgroupName = (await _userService.GetUser(siteId, dto.AssignedWorkgroupId, UserType.Workgroup)).Name;
                }
                catch(Exception ex)
                {
                    var rex = ex;
                }

                dto.AssetName = assetNames.FirstOrDefault(a => a.Id == dto.AssetId)?.Name;
                foreach (var checkDto in dto.Checks)
                {
                    checkDto.IsPaused = IsCheckPaused(checkDto.PauseStartDate, checkDto.PauseEndDate);
                    if (checkDto.Statistics.LastCheckSubmittedUserId.HasValue)
                    {
                        try
                        {
                            var user = await _userService.GetUser(siteId, checkDto.Statistics.LastCheckSubmittedUserId.Value);

                            checkDto.Statistics.LastCheckSubmittedUserName = user.Name;
                        }
                        catch (Exception ex2)
                        {
                            var ex3 = ex2;
                        }
                    }
                }

            }
            return inspectionDtos;
        }

        private async Task<List<AssetMinimum>> GetAssetNames(Guid siteId, IEnumerable<Guid> assetIds)
        {
            if (!assetIds.Any())
                return new List<AssetMinimum>();
         
            return await _digitalTwinService.GetAssetsByIds(siteId, assetIds);
        }

        private static List<Asset> FlattenAssetCategoryAssets(IList<AssetCategory> categories)
        {
            var result = new List<Asset>();
            foreach (var category in categories)
            {
                if (category.Categories != null)
                {
                    result.AddRange(FlattenAssetCategoryAssets(category.Categories));
                }
                result.AddRange(category.Assets);
            }
            return result;
        }

        public async Task<InspectionDto> EnrichInspection(Guid siteId, Inspection inspection)
        {
            var inspectionDto = InspectionDto.MapFromModel(inspection);
            var siteWorkgroups = await _workflowApi.GetWorkgroups(siteId);
          
            try
            {
                var asset = await _digitalTwinService.GetAssetDetailsAsync(siteId, inspection.AssetId);
                inspectionDto.AssetName = asset.Name;
            }
            catch (RestException rex) when (rex.StatusCode == HttpStatusCode.NotFound)
            {
                // Leave the assetname empty rather than exception when asset does not exist
            }

            inspectionDto.AssignedWorkgroupName = siteWorkgroups.FirstOrDefault(w => w.Id == inspectionDto.AssignedWorkgroupId)?.Name;
            inspectionDto.Checks.ForEach(c => c.IsPaused = IsCheckPaused(c.PauseStartDate, c.PauseEndDate));

            return inspectionDto;
        }

        public async Task<InspectionUsageDto> GetInspectionUsageBySiteId(Guid siteId, InspectionUsagePeriod period)
        {
            var inspectionUsage = await _workflowApi.GetInspectionUsageBySiteId(siteId, period);
            var inspectionUsageDto = InspectionUsageDto.MapFromModel(inspectionUsage);

           var users = inspectionUsage.UserIds.Select( async userId=>
            {
                try
                {
                    return (await _userService.GetUser(siteId, userId)).Name;
                }
                catch
                {
                    return "Unknown";
                }
            });

            inspectionUsageDto.UserName = (await Task.WhenAll(users)).ToList();

            return inspectionUsageDto;
        }

        public async Task ArchiveInspection(Guid siteId, Guid inspectionId, bool isArchived)
        {
            await _workflowApi.ArchiveInspection(siteId, inspectionId, isArchived);
        }

        public async Task ArchiveZone(Guid siteId, Guid zoneId, bool isArchived)
        {
            await _workflowApi.ArchiveZone(siteId, zoneId, isArchived);
        }

        public async Task UpdateSortOrder(Guid siteId, Guid zoneId, UpdateInspectionSortOrderRequest request)
        {
            await _workflowApi.UpdateInspectionSortOrder(siteId, zoneId, request);
        }

        private bool IsCheckPaused(DateTime? startDate, DateTime? endDate)
        {
            var utcNow = _dateTimeService.UtcNow;
            switch (startDate.HasValue, endDate.HasValue)
            {
                case (false, false):
                    return false;
                case (true, true):
                    return startDate?.CompareTo(endDate) < 0 && utcNow.CompareTo(endDate) <= 0;
                case (true, false):
                    return true;
                case (false, true):
                    return false;
            }
        }
    }
}
