using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Willow.Calendar;
using Willow.Common;

using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Models;
using WorkflowCore.Repository;
using WorkflowCore.Services.Apis;

namespace WorkflowCore.Services
{
	public interface IInspectionService
    {
        Task<Zone> CreateZone(Guid siteId, CreateZoneRequest createZoneRequest);
        Task<List<Zone>> GetZones(Guid siteId, bool includeStatistics);
        Task<List<Zone>> GetZones(List<Guid> siteIds);
        Task UpdateZone(Guid siteId, Guid zoneId, UpdateZoneRequest updateZoneRequest);
        Task<List<Inspection>> GetZoneInspections(Guid siteId, Guid zoneId);
        Task<Inspection> CreateInspection(Guid siteId, CreateInspectionRequest createInspectionRequest);
        Task<List<Inspection>> GetSiteInspections(Guid siteId);
        Task<Inspection> UpdateInspection(Guid siteId, Guid inspectionId, UpdateInspectionRequest updateInspectionRequest);
        Task<Inspection> GetInspection(Guid siteId, Guid inspectionId);
        Task<List<CheckRecordReportDto>> GetCheckHistory(Guid siteId,Guid customerId, Guid inspectionId, Guid? checkId, DateTime startDate, DateTime endDate);
        Task ArchiveZone(Guid siteId, Guid zoneId, bool isArchived);
        Task ArchiveInspection(Guid siteId, Guid inspectionId, bool isArchived);
        Task UpdateSortOrder(Guid siteId, Guid zoneId, UpdateInspectionSortOrderRequest request);
		Task<List<CheckRecord>> GetCheckSubmittedHistory(Guid siteId, Guid inspectionId, Guid checkId, int count);
		Task<List<Inspection>> CreateInspections(Guid siteId, CreateInspectionsRequest request);
		Task AddTwinIdToInspectionsAsync(int batchSize, CancellationToken stoppingToken);
        Task<Inspection> GetInspection(Guid inspectionId);

    }

    public class InspectionService : IInspectionService
    {
        private readonly IInspectionRepository _repository;
        private readonly ISiteService          _siteService;
        private readonly IDateTimeService      _dateTimeService;
        private readonly IDigitalTwinServiceApi _digitalTwinServiceApi;
        private readonly ILogger<InspectionService> _logger;
        private readonly IImagePathHelper _imagePathHelper;

        public InspectionService(IInspectionRepository repository, ISiteService siteService, IDateTimeService dateTimeService, IDigitalTwinServiceApi digitalTwinServiceApi, ILogger<InspectionService> logger, IImagePathHelper imagePathHelper)
        {
            _repository      = repository;
            _siteService     = siteService;
            _dateTimeService = dateTimeService;
			_digitalTwinServiceApi=digitalTwinServiceApi;
			_logger = logger;
            _imagePathHelper = imagePathHelper;
        }

        public async Task<Zone> CreateZone(Guid siteId, CreateZoneRequest createZoneRequest)
        {
            var zone = new Zone
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                Name = createZoneRequest.Name
            };
            await _repository.CreateZone(zone);
            return zone;
        }

        public async Task<List<Inspection>> GetZoneInspections(Guid siteId, Guid zoneId)
        {
            var timeZone = (await _siteService.GetSite(siteId)).TimezoneId;
            var now      = _dateTimeService.UtcNow.InTimeZone(timeZone);

            return await _repository.GetZoneInspections(siteId, zoneId, now, timeZone);
        }

        public async Task<List<Zone>> GetZones(Guid siteId, bool includeStatistics)
        {
            var zones = await _repository.GetZones(new List<Guid>(){ siteId});
            if (includeStatistics)
            {
                await _repository.FillZonesStatistics(siteId, zones);
            }
            return zones;
        }
        public async Task<List<Zone>> GetZones(List<Guid> siteIds)
        {
            if (siteIds == null || !siteIds.Any())
            {
                throw new ArgumentException().WithData(new {siteIds });
            }
            return await _repository.GetZones(siteIds);
        }
        public async Task UpdateZone(Guid siteId, Guid zoneId, UpdateZoneRequest updateZoneRequest)
        {
            await _repository.UpdateZone(siteId, zoneId, updateZoneRequest); 
        }

        public async Task<Inspection> CreateInspection(Guid siteId, CreateInspectionRequest createInspectionRequest)
        {
            ValidateInspectionRequest(createInspectionRequest);

            var inspectionId = Guid.NewGuid();
            var sortOrder = await _repository.GetNewInspectionSortOrder(siteId, createInspectionRequest.ZoneId);
            var inspectionTwinId =!string.IsNullOrWhiteSpace(createInspectionRequest.TwinId)? createInspectionRequest.TwinId:
	            (await GetTwinIdByAssetIdAsync(new List<Guid> { createInspectionRequest.AssetId }, siteId))
	            .FirstOrDefault(c => c.UniqueId.Equals(createInspectionRequest.AssetId.ToString(),
		            StringComparison.InvariantCultureIgnoreCase))?.Id;

			var inspection = new Inspection
            {
                Id = inspectionId,
                SiteId = siteId,
                Name = createInspectionRequest.Name,
                FloorCode = createInspectionRequest.FloorCode,
                ZoneId = createInspectionRequest.ZoneId,
                AssetId = createInspectionRequest.AssetId,
				TwinId = inspectionTwinId,
                AssignedWorkgroupId = createInspectionRequest.AssignedWorkgroupId,
                Frequency = createInspectionRequest.Frequency,
                FrequencyUnit = createInspectionRequest.FrequencyUnit,
                FrequencyDaysOfWeek = createInspectionRequest.FrequencyDaysOfWeek,
                StartDate = createInspectionRequest.StartDate,
                EndDate = createInspectionRequest.EndDate,
                LastRecordId = null,
                IsArchived = false,
                SortOrder = sortOrder,
                Checks = CheckListBuilder(createInspectionRequest.Checks, inspectionId)
            };
            await _repository.CreateInspection(inspection);
            return inspection;
        }

        private static void ValidateInspectionRequest<T>(T inspectionRequest) where T : InspectionRequest
        {
            if ((inspectionRequest.FrequencyUnit == SchedulingUnit.Hours && !Enumerable.Range(1, 24).Contains(inspectionRequest.Frequency))
            || (inspectionRequest.FrequencyUnit == SchedulingUnit.Days && !Enumerable.Range(1, 7).Contains(inspectionRequest.Frequency))
            || (inspectionRequest.FrequencyUnit == SchedulingUnit.Weeks && !Enumerable.Range(1, 52).Contains(inspectionRequest.Frequency))
            || (inspectionRequest.FrequencyUnit == SchedulingUnit.Months && !Enumerable.Range(1, 12).Contains(inspectionRequest.Frequency))
            || (inspectionRequest.FrequencyUnit == SchedulingUnit.Years && !Enumerable.Range(1, 10).Contains(inspectionRequest.Frequency)))
            {
                throw new ArgumentException().WithData(new { inspectionRequest.Frequency });
            }
            var listTypeCheckValueDict = new Dictionary<string, string>();
            foreach (var createCheck in inspectionRequest.Checks)
            {
                if (!string.IsNullOrWhiteSpace(createCheck.DependencyName) &&
                    (!listTypeCheckValueDict.TryGetValue(createCheck.DependencyName, out string dependencyValue) || 
                    !dependencyValue.Contains(createCheck.DependencyValue, StringComparison.InvariantCulture)))
                {
                    throw new ArgumentException().WithData(new { createCheck.DependencyName });
                }
                if (createCheck.Type == CheckType.List)
                {
                    listTypeCheckValueDict.Add(createCheck.Name, createCheck.TypeValue);
                }
            }
        }

        public async Task<List<Inspection>> GetSiteInspections(Guid siteId)
        {
            var timeZone = (await _siteService.GetSite(siteId)).TimezoneId;
            var now      = _dateTimeService.UtcNow.InTimeZone(timeZone);

           return await _repository.GetSiteInspections(siteId, now, timeZone);
        }

        public async Task<Inspection> UpdateInspection(Guid siteId, Guid inspectionId, UpdateInspectionRequest updateInspectionRequest)
        {
            ValidateInspectionRequest(updateInspectionRequest);

            await _repository.UpdateInspection(inspectionId , updateInspectionRequest);
            
            return await _repository.GetInspection(siteId, inspectionId);
        }

        public async Task<Inspection> GetInspection(Guid siteId, Guid inspectionId)
        {
            return await _repository.GetInspection(siteId, inspectionId);
        }
        public async Task<Inspection> GetInspection(Guid inspectionId)
        {
            return await _repository.GetInspection(inspectionId);
        }
        public async Task<List<CheckRecordReportDto>> GetCheckHistory(Guid siteId, Guid customerId, Guid inspectionId, Guid? checkId, DateTime startDate, DateTime endDate)
        {
            var inspection = await _repository.GetInspection(inspectionId);
            if (inspection.Checks == null || !inspection.Checks.Any())
            {
               return null;
            }
            var checkRecordsTask= _repository.GetCheckHistory(checkId.HasValue? [checkId.Value] : inspection.Checks.Select(c=>c.Id).ToList(), startDate, endDate);
            var twinNameTask = _digitalTwinServiceApi.GetTwinById(siteId, inspection.TwinId);
            return CheckRecordReportDto.MapFromModels(checkRecordsTask.Result,_imagePathHelper, inspection,(twinNameTask.Result)?.Name,customerId,siteId);
        }

        public async Task ArchiveZone(Guid siteId, Guid zoneId, bool isArchived)
        {
            await _repository.ArchiveZone(siteId, zoneId, isArchived);
        }

        public async Task ArchiveInspection(Guid siteId, Guid inspectionId, bool isArchived)
        {
            await _repository.ArchiveInspection(siteId, inspectionId, isArchived);
        }

        public async Task UpdateSortOrder(Guid siteId, Guid zoneId, UpdateInspectionSortOrderRequest request)
        {
            await _repository.UpdateInspectionSortOrder(siteId, zoneId, request.InspectionIds);
        }

		public async Task<List<CheckRecord>> GetCheckSubmittedHistory(Guid siteId, Guid inspectionId, Guid checkId, int count)
		{
			return await _repository.GetCheckSubmittedHistory(siteId, inspectionId, checkId, count);
		}
		/// <summary>
		/// Create Inspection for each asset
		/// </summary>
		/// <param name="siteId"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public async Task<List<Inspection>> CreateInspections(Guid siteId, CreateInspectionsRequest request)
		{
			ValidateInspectionRequest(request);
			var sortOrder = await _repository.GetNewInspectionSortOrder(siteId, request.ZoneId);
			var inspections = new List<Inspection>();
			var inspectionsTwinId =request.AssetList.Any(c=>string.IsNullOrWhiteSpace(c.TwinId))?  await GetTwinIdByAssetIdAsync(request.AssetList.Select(c => c.AssetId), siteId):null;
			foreach (var asset in request.AssetList)
			{
				var inspectionId = Guid.NewGuid();
				var inspection = new Inspection
				{
					Id = inspectionId,
					SiteId = siteId,
					Name = request.Name,
					FloorCode = asset.FloorCode ?? string.Empty,
					ZoneId = request.ZoneId,
					AssetId = asset.AssetId,
					TwinId =!string.IsNullOrWhiteSpace(asset.TwinId)? asset.TwinId: inspectionsTwinId?.FirstOrDefault(c => c.UniqueId.Equals(asset.AssetId.ToString(),StringComparison.InvariantCultureIgnoreCase))?.Id,
					AssignedWorkgroupId = request.AssignedWorkgroupId,
					Frequency = request.Frequency,
					FrequencyUnit = request.FrequencyUnit,
                    FrequencyDaysOfWeek = request.FrequencyDaysOfWeek,
					StartDate = request.StartDate,
					EndDate = request.EndDate,
					LastRecordId = null,
					IsArchived = false,
					SortOrder = sortOrder,
					Checks = CheckListBuilder(request.Checks, inspectionId)
				};

				inspections.Add(inspection);
				sortOrder++;
			}

			await _repository.CreateInspections(inspections);
			return inspections;
		}

		private List<Check> CheckListBuilder(List<CheckRequest> ChecksReuest, Guid inspectionId)
		{
			var checks = new List<Check>();
			var checkIndex = 0;
			foreach (var checkRequest in ChecksReuest)
			{
				var check = new Check
				{
					Id = Guid.NewGuid(),
					InspectionId = inspectionId,
					SortOrder = ++checkIndex,
					Name = checkRequest.Name,
					Type = checkRequest.Type.Value,
					TypeValue = checkRequest.TypeValue,
					DecimalPlaces = checkRequest.DecimalPlaces.HasValue ? checkRequest.DecimalPlaces.Value : 0,
					MinValue = checkRequest.MinValue,
					MaxValue = checkRequest.MaxValue,
                    Multiplier = checkRequest.Multiplier,
					DependencyId = string.IsNullOrEmpty(checkRequest.DependencyName) ? null : checks.FirstOrDefault(c => c.Name == checkRequest.DependencyName).Id,
					DependencyValue = string.IsNullOrEmpty(checkRequest.DependencyName) ? null : checkRequest.DependencyValue,
					PauseStartDate = checkRequest.PauseStartDate,
					PauseEndDate = checkRequest.PauseEndDate,
					LastRecordId = null,
					LastSubmittedRecordId = null,
					IsArchived = false,
					CanGenerateInsight = checkRequest.CanGenerateInsight
				};
				checks.Add(check);
			}
			return checks;

		}

		public async Task AddTwinIdToInspectionsAsync(int batchSize, CancellationToken stoppingToken)
		{
			var pageNumber = 1;
			var inspections = await _repository.GetPagedInspectionsWithoutTwinIdAsync(pageNumber, batchSize);
			while (inspections != null && inspections.Any())
			{

				var inspectionGroupedBySite = inspections.GroupBy(c => c.SiteId);

				foreach (var site in inspectionGroupedBySite)
				{
					try
					{
						var siteInspections = site.ToList();
						var siteTwinIds = await _digitalTwinServiceApi.GetTwinIdsByUniqueIdsAsync(site.Key,
							siteInspections.Select(c => c.AssetId).Distinct());
						if (siteTwinIds != null && siteTwinIds.Any())
						{
							siteInspections.ForEach(inspection =>
							inspection.TwinId =
									siteTwinIds.FirstOrDefault(c =>  c.UniqueId.Equals(  inspection.AssetId.ToString(),StringComparison.InvariantCultureIgnoreCase))?.Id);

						}
						await _repository.UpdateInspectionsAsync(siteInspections);
					}
					catch (Exception ex)
					{
						_logger.LogWarning("Failed to get twinId  in AddTwinIdToInspectionsAsync {Message}", ex.Message);
					}

					pageNumber += 1;
					inspections = await _repository.GetPagedInspectionsWithoutTwinIdAsync(pageNumber, batchSize);
				}

			}
		}

		private async Task<List<TwinIdDto>> GetTwinIdByAssetIdAsync(IEnumerable<Guid> assetIds, Guid siteId)
		{       // Due to the misconfigured DT setting for some sites, and also invalid uniqueIds on production, we skip the exception. 
			try
			{

				return await _digitalTwinServiceApi.GetTwinIdsByUniqueIdsAsync(siteId,
					assetIds);

			}
			catch (Exception ex)
			{
				_logger.LogError(
					$"'adding TwinId to the Inspection failed with exception, message: {ex.Message} {Environment.NewLine} stack trace: {ex.StackTrace}");

			}

			return null;
		}

}
}
