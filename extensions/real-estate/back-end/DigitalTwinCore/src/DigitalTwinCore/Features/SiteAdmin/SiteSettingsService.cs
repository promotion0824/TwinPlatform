using System;
using System.Threading;
using System.Threading.Tasks;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Features.DirectoryCore;
using DigitalTwinCore.Services.AdtApi;

namespace DigitalTwinCore.Features.SiteAdmin
{
    public interface ISiteSettingsService
    {
        Task<SiteAdtSettings> AddSiteSettings(NewAdtSiteRequest request, CancellationToken cancellationToken);
        Task<SiteAdtSettings> GetSiteSettings(Guid siteId);
    }

    public class SiteSettingsService: ISiteSettingsService
    {
        private readonly DigitalTwinDbContext _dtDbContext;
        private readonly IDirectoryCoreClient _directoryCoreClient;

        public SiteSettingsService(DigitalTwinDbContext dtDbContext, IDirectoryCoreClient directoryCoreClient)
        {
            _dtDbContext = dtDbContext;
            _directoryCoreClient = directoryCoreClient;
        }

        public async Task<SiteAdtSettings> AddSiteSettings(NewAdtSiteRequest request, CancellationToken cancellationToken)
        {
            await _directoryCoreClient.SiteExists(request.SiteId, cancellationToken);

            var siteSetting = await _dtDbContext.SiteSettings.FindAsync(request.SiteId);

            if (siteSetting != null)
            {
                return null;
            }

            var entity = new SiteSettingEntity
            {
                SiteId = request.SiteId,
                AdxDatabase = request.AdxDatabase,
                InstanceUri = request.InstanceUri.AbsoluteUri,
                SiteCodeForModelId = request.SiteCode
            };

            await _dtDbContext.SiteSettings.AddAsync(entity, cancellationToken);

            await _dtDbContext.SaveChangesAsync(cancellationToken);

            return SiteAdtSettings.CreateInstance(request.SiteId, entity);
        }

        public async Task<SiteAdtSettings> GetSiteSettings(Guid siteId)
        {
            var entity = await _dtDbContext.SiteSettings.FindAsync(siteId);
            if (entity == null)
            {
                return null;
            }
            return SiteAdtSettings.CreateInstance(siteId, entity);
        }
    }
}
