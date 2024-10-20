using Microsoft.EntityFrameworkCore;
using SiteCore.Domain;
using SiteCore.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SiteCore.Services
{
    public interface IModuleGroupsService
    {
        Task<ModuleGroupEntity> GetModuleGroupEntityAsync(Guid siteId, string name, bool createIfNotExist = true);
        Task<ModuleGroup> GetModuleGroupAsync(Guid id);
        Task<ModuleGroup> GetModuleGroupByNameAsync(Guid siteId, string name);
        Task<ModuleGroup> UpdateModuleGroupAsync(ModuleGroup moduleGroup);
    }

    public class ModuleGroupsService : IModuleGroupsService
    {
        private readonly SiteDbContext _context;
        public ModuleGroupsService(SiteDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<ModuleGroupEntity> GetModuleGroupEntityAsync(Guid siteId, string name, bool createIfNotExist = true)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var moduleGroup = await _context.ModuleGroups.FirstOrDefaultAsync(x => x.SiteId == siteId && x.Name.ToLower() == name.ToLower());
            if (moduleGroup == null && createIfNotExist)
            {
                var siteModuleGroups = await _context.ModuleGroups.Where(x => x.SiteId == siteId).ToListAsync();

                moduleGroup = new ModuleGroupEntity
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    SortOrder = siteModuleGroups.Any() ? siteModuleGroups.Max(x => x.SortOrder) + 1 : 0,
                    SiteId = siteId
                };
                _context.ModuleGroups.Add(moduleGroup);
                await _context.SaveChangesAsync();
            }
            return moduleGroup;
        }

        public async Task<ModuleGroup> UpdateModuleGroupAsync(ModuleGroup moduleGroup)
        {
            var moduleGroupEntity = new ModuleGroupEntity
            {
                Id = moduleGroup.Id,
                Name = moduleGroup.Name,
                SortOrder = moduleGroup.SortOrder,
                SiteId = moduleGroup.SiteId
            };
            _context.ModuleGroups.Update(moduleGroupEntity);
            await _context.SaveChangesAsync();

            return moduleGroup;
        }

        public async Task<ModuleGroup> GetModuleGroupByNameAsync(Guid siteId, string name)
        {
            var moduleGroup = await GetModuleGroupEntityAsync(siteId, name, false);

            return moduleGroup != null ? new ModuleGroup
            {
                Id = moduleGroup.Id,
                SiteId = moduleGroup.SiteId,
                Name = moduleGroup.Name,
                SortOrder = moduleGroup.SortOrder
            } : null;
        }

        public async Task<ModuleGroup> GetModuleGroupAsync(Guid id)
        {
            var moduleGroup = await _context.ModuleGroups.FirstOrDefaultAsync(x => x.Id == id);
            
            return moduleGroup != null ? new ModuleGroup { 
                Id = moduleGroup.Id,
                SiteId = moduleGroup.SiteId,
                Name = moduleGroup.Name,
                SortOrder = moduleGroup.SortOrder
            } : null;
        }
    }
}
