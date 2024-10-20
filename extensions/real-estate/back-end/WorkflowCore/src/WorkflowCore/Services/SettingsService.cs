using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Entities;
using WorkflowCore.Repository;

namespace WorkflowCore.Services
{
    public interface ISettingsService
    {
        Task<SiteExtensionEntity> GetSiteExtensions(Guid siteId);
        Task<SiteExtensionEntity> UpsertSiteSettingsRequest(Guid siteId, UpsertSiteSettingsRequest request);
        Task<List<SiteExtensionEntity>> GetSiteExtensionsListBySiteIds(List<Guid> siteIds);
        Task UpdateSiteLastDailyReportDate(Guid siteId, DateTime lastDailyReportDate);
    }

    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _repository;

        public SettingsService(ISettingsRepository repository)
        {
            _repository = repository;
        }

        public async Task<SiteExtensionEntity> GetSiteExtensions(Guid siteId)
        {
            return await _repository.GetSiteExtensions(siteId);
        }

        public async Task<List<SiteExtensionEntity>> GetSiteExtensionsListBySiteIds(List<Guid> siteIds)
        {
            return await _repository.GetSiteExtensionsListBySiteIds(siteIds);
        }

        public async Task UpdateSiteLastDailyReportDate(Guid siteId, DateTime lastDailyReportDate)
        {
            await _repository.UpdateSiteLastDailyReportDate(siteId, lastDailyReportDate);
        }

        public async Task<SiteExtensionEntity> UpsertSiteSettingsRequest(Guid siteId, UpsertSiteSettingsRequest request)
        {
            var siteExtensions = await _repository.UpsertSiteSettingsRequest(siteId, request);
            return siteExtensions;
        }

    }
}
