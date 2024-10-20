using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Entities;

namespace WorkflowCore.Repository
{
    public interface ISettingsRepository
    {
        Task<SiteExtensionEntity> GetSiteExtensions(Guid siteId);
        Task<SiteExtensionEntity> UpsertSiteSettingsRequest(Guid siteId, UpsertSiteSettingsRequest request);
        Task<List<SiteExtensionEntity>> GetSiteExtensionsListBySiteIds(List<Guid> siteIds);
        Task UpdateSiteLastDailyReportDate(Guid siteId, DateTime lastDailyReportDate);
    }

    public class SettingsRepository : ISettingsRepository
    {
        private readonly WorkflowContext _context;

        public SettingsRepository(WorkflowContext context)
        {
            _context = context;
        }

        public async Task<SiteExtensionEntity> GetSiteExtensions(Guid siteId)
        {
            var siteExtensions = await _context.SiteExtensions.FirstOrDefaultAsync(x => x.SiteId == siteId);
            if (siteExtensions == null)
            {
                siteExtensions = new SiteExtensionEntity
                {
                    SiteId = siteId
                };
            }

            return siteExtensions;
        }

        public async Task<List<SiteExtensionEntity>> GetSiteExtensionsListBySiteIds(List<Guid> siteIds)
        {
            return await _context.SiteExtensions.Where(e => siteIds.Contains(e.SiteId)).ToListAsync();
        }

        public async Task UpdateSiteLastDailyReportDate(Guid siteId, DateTime lastDailyReportDate)
        {
            var siteExtensions = await _context.SiteExtensions.AsTracking().Where(x => x.SiteId == siteId).FirstOrDefaultAsync();

            siteExtensions.LastDailyReportDate = lastDailyReportDate;
            await _context.SaveChangesAsync();
        }

        public async Task<SiteExtensionEntity> UpsertSiteSettingsRequest(Guid siteId, UpsertSiteSettingsRequest request)
        {
            var siteExtensions = await _context.SiteExtensions.AsTracking().Where(x => x.SiteId == siteId).FirstOrDefaultAsync();
            if (siteExtensions == null)
            {
                siteExtensions = new SiteExtensionEntity();
                siteExtensions.SiteId = siteId;
                siteExtensions.InspectionDailyReportWorkgroupId = request.InspectionDailyReportWorkgroupId;
                await _context.SiteExtensions.AddAsync(siteExtensions);
            }
            else
            {
                siteExtensions.InspectionDailyReportWorkgroupId = request.InspectionDailyReportWorkgroupId;
            }

            await _context.SaveChangesAsync();

            return siteExtensions;
        }
    }
}
