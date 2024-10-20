using Microsoft.EntityFrameworkCore;
using SiteCore.Domain;
using SiteCore.Entities;
using SiteCore.Services.ImageHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiteCore.Services
{
    public interface IModulesService
    {
        Task<List<ModuleUrlResult>> GetModulesPathsAsync(IEnumerable<Guid> disciplineImageIds);
        Task<List<Module>> GetModulesByFloorAsync(Guid siteId, Guid floorId);
        Task<List<Module>> GetModulesBySiteAsync(Guid siteId);
        Task DeleteModulesBySiteAsync(Guid siteId);
    }

    public class ModulesService : IModulesService
    {
        private readonly SiteDbContext _dbContext;
        private readonly IImagePathHelper _pathHelper;

        public ModulesService(SiteDbContext dbContext, IImagePathHelper pathHelper)
        {
            _dbContext = dbContext;
            _pathHelper = pathHelper;
        }

        public async Task<List<Module>> GetModulesByFloorAsync(Guid siteId, Guid floorId)
        {
            var floorCode = floorId != Guid.Empty ? string.Empty : "_SITE";

            var imageEntities = await _dbContext.Modules
                .Where(di => (di.FloorId == floorId || di.Floor.Code == floorCode) && di.Floor.SiteId == siteId)
                .Include(di => di.ModuleType)
                .Include(x => x.ModuleType.ModuleGroup)
                .ToListAsync();

            var images = ModuleEntity.MapToDomainObjects(imageEntities);
            var imageUrls = (await GetModulesPathsAsync(images.Select(im => im.Id).ToList())).ToDictionary(iu => iu.DisciplineImageId, iu => iu.Path);
            foreach (var disciplineImage in images)
            {
                disciplineImage.Path = imageUrls[disciplineImage.Id];
            }

            return images;
        }

        public async Task<List<Module>> GetModulesBySiteAsync(Guid siteId)
        {
            return await GetModulesByFloorAsync(siteId, Guid.Empty);
        }

        public async Task DeleteModulesBySiteAsync(Guid siteId)
        {
            var floorCode = "_SITE";

            var modules = await _dbContext.Modules
                .Where(di => di.Floor.Code == floorCode && di.Floor.SiteId == siteId)
                .ToListAsync();

            _dbContext.Modules.RemoveRange(modules);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ModuleUrlResult>> GetModulesPathsAsync(IEnumerable<Guid> disciplineImageIds)
        {
            var imagesData = await _dbContext.Modules
                .Select(di => new { di.Id, di.FloorId, di.Floor.SiteId, di.Floor.Site.CustomerId, di.VisualId, di.ModuleType.Is3D, di.Url })
                .Where(di => disciplineImageIds.Contains(di.Id))
                .ToListAsync();

            return imagesData.Select(id => new ModuleUrlResult
            {
                DisciplineImageId = id.Id,
                Path = id.Is3D ? id.Url : _pathHelper.GetFloorModulePath(id.CustomerId, id.SiteId, id.FloorId)
            }).ToList();
        }
    }

    public class ModuleUrlResult
    {
        public Guid DisciplineImageId { get; set; }

        public string Path { get; set; }
    }
}
