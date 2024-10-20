using Microsoft.EntityFrameworkCore;
using SiteCore.Domain;
using SiteCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.ExceptionHandling.Exceptions; 
using Willow.Infrastructure;


namespace SiteCore.Services
{
    public interface IModuleTypesService
    {
        Task<List<ModuleType>> GetModuleTypesAsync(Guid? siteId = null);
        Task DeleteModuleTypeAsync(Guid moduleTypeId);
        Task<ModuleType> GetModuleTypeAsync(Guid moduleTypeId);
        Task<ModuleType> CreateModuleTypeAsync(Guid siteId, ModuleType moduleType);
        Task<ModuleType> UpdateModuleTypeAsync(Guid siteId, Guid id, ModuleType moduleType);
        Task<List<ModuleType>> CreateDefaultModuleTypesAsync(Guid siteId);
        Task<bool> IsValidPrefix(Guid siteId, string prefix, bool is3D, Guid? currentEntityId = null);
    }

    public class ModuleTypesService : IModuleTypesService
    {
        private readonly SiteDbContext _context;
        private const string DefaultDisciplinesFile = "defaultmoduletypes.json";
        private readonly IModuleGroupsService _moduleGroupsService;

        public ModuleTypesService(SiteDbContext dbContext, IModuleGroupsService moduleGroupsService)
        {
            _context = dbContext;
            _moduleGroupsService = moduleGroupsService;
        }

        public async Task<List<ModuleType>> GetModuleTypesAsync(Guid? siteId = null)
        {
            var moduleTypes = await _context.ModuleTypes.Include(x => x.ModuleGroup).ToListAsync();

            if (siteId.HasValue && siteId.Value != Guid.Empty)
                moduleTypes = moduleTypes.Where(x => x.SiteId == siteId.Value).ToList();

            return ModuleTypeEntity.MapToDomainObjects(moduleTypes);
        }

        public async Task<bool> IsValidPrefix(Guid siteId, string prefix, bool is3D, Guid? currentEntityId = null)
        {
            var existingEntity = await _context.ModuleTypes.FirstOrDefaultAsync(x => x.SiteId == siteId && x.Is3D == is3D && x.Prefix.ToLower() == prefix.ToLower());

            if (existingEntity == null)
                return true;

            if (!currentEntityId.HasValue)
                return false;
            
            return existingEntity.Id == currentEntityId.Value;
        }

        public async Task<List<ModuleType>> CreateDefaultModuleTypesAsync(Guid siteId)
        {
            var jsonString = System.IO.File.ReadAllText(DefaultDisciplinesFile);
            var moduleTypes = JsonSerializerExtensions.Deserialize<List<ModuleType>>(jsonString);

            var returnTypes = new List<ModuleType>();

            foreach (var moduleType in moduleTypes)
            {
                var result = await CreateModuleTypeAsync(siteId, moduleType);
                if(result != null)
                    returnTypes.Add(result);
            }

            return returnTypes;
        }

        public async Task<ModuleType> CreateModuleTypeAsync(Guid siteId, ModuleType moduleType)
        {
            if (!await IsValidPrefix(siteId, moduleType.Prefix, moduleType.Is3D))
                return null;

            var moduleGroup = await _moduleGroupsService.GetModuleGroupEntityAsync(siteId, moduleType.ModuleGroup);

            var moduleTypeEntity =
                new ModuleTypeEntity
                {
                    Id = Guid.NewGuid(),
                    CanBeDeleted = moduleType.CanBeDeleted,
                    Is3D = moduleType.Is3D,
                    ModuleGroupId = moduleGroup?.Id,
                    Name = moduleType.Name,
                    Prefix = moduleType.Prefix,
                    SiteId = siteId,
                    SortOrder = moduleType.SortOrder,
                    IsDefault = moduleType.IsDefault
                };

            await _context.ModuleTypes.AddAsync(moduleTypeEntity);
            await _context.SaveChangesAsync();
            _context.Entry(moduleTypeEntity).State = EntityState.Detached;

            moduleTypeEntity.ModuleGroup = moduleGroup;

            return ModuleTypeEntity.MapToDomainObject(moduleTypeEntity);
        }

        public async Task<ModuleType> UpdateModuleTypeAsync(Guid siteId, Guid id, ModuleType moduleType)
        {
            if (!await IsValidPrefix(siteId, moduleType.Prefix, moduleType.Is3D, id))
                return null;

            var moduleGroup = await _moduleGroupsService.GetModuleGroupEntityAsync(siteId, moduleType.ModuleGroup);

            var moduleTypeEntity =
                new ModuleTypeEntity
                {
                    Id = id,
                    CanBeDeleted = moduleType.CanBeDeleted,
                    Is3D = moduleType.Is3D,
                    ModuleGroupId = moduleGroup?.Id,
                    Name = moduleType.Name,
                    Prefix = moduleType.Prefix,
                    SiteId = siteId,
                    SortOrder = moduleType.SortOrder,
                    IsDefault = moduleType.IsDefault
                };

            _context.ModuleTypes.Update(moduleTypeEntity);
            await _context.SaveChangesAsync();

            moduleTypeEntity.ModuleGroup = moduleGroup;

            return ModuleTypeEntity.MapToDomainObject(moduleTypeEntity);
        }

        private async Task<ModuleTypeEntity> GetModuleTypeEntityAsync(Guid moduleTypeId)
        {
            return await _context.ModuleTypes
                .Include(x => x.Modules)
                .FirstOrDefaultAsync(g => g.Id == moduleTypeId);
        }

        public async Task<ModuleType> GetModuleTypeAsync(Guid moduleTypeId)
        {
            var moduleType = await _context.ModuleTypes
                .Include(x => x.Modules)
                .Include(x => x.ModuleGroup)
                .FirstOrDefaultAsync(g => g.Id == moduleTypeId);

            return ModuleTypeEntity.MapToDomainObject(moduleType);
        }

        public async Task DeleteModuleTypeAsync(Guid moduleTypeId)
        {
            var moduleType = await GetModuleTypeEntityAsync(moduleTypeId);
            if (moduleType == null)
            {
                throw new NotFoundException(new { ModuleTypeId = moduleTypeId });
            }

            _context.Remove(moduleType);

            await _context.SaveChangesAsync();
        }
    }
}
