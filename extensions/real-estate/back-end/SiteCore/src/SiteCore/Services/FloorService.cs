using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LazyCache;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SiteCore.Domain;
using SiteCore.Entities;
using SiteCore.Options;
using SiteCore.Requests;
using SiteCore.Services.ImageHub;
using Image = SixLabors.ImageSharp.Image;
using NotFoundException = Willow.ExceptionHandling.Exceptions.NotFoundException;

namespace SiteCore.Services
{
    public class FloorService : IFloorService
    {
        private readonly IAppCache _appCache;
        private readonly ILogger<FloorService> _logger;
        private readonly SiteDbContext _dbContext;
        private readonly IImageHubService _imageHubService;
        private readonly IModuleGroupsService _moduleGroupsService;
        private readonly FloorModuleOptions _moduleOptions;
        private readonly string floor = "floor";

        public FloorService(
            IAppCache appCache,
            ILogger<FloorService> logger,
            SiteDbContext dbContext,
            IImageHubService imageHubService,
            IOptions<FloorModuleOptions> moduleOptions,
            IModuleGroupsService moduleGroupsService)
        {
            _appCache = appCache;
            _logger = logger;
            _dbContext = dbContext;
            _imageHubService = imageHubService;
            _moduleOptions = moduleOptions.Value;
            _moduleGroupsService = moduleGroupsService;
        }

        private readonly string GETALLFLOORENTITIESCACHEKEY = "get-all-floorentities";
        private readonly string GETALLFLOORSCACHEKEY = "get-all-floors";

        private void ResetCache()
        {
            _appCache.Remove(GETALLFLOORENTITIESCACHEKEY);
            _appCache.Remove(GETALLFLOORSCACHEKEY);
        }

        private async Task<List<FloorEntity>> GetFloorEntities()
        {
            var floors = await _appCache.GetOrAddAsync(GETALLFLOORENTITIESCACHEKEY, async (cache) =>
            {
                cache.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddHours(6));
                return await _dbContext.Floors.Include(x => x.Modules).ThenInclude(x => x.ModuleType).ToListAsync();
            });

            return floors;
        }

        private async Task<List<Floor>> GetFloors()
        {
            var floors = await _appCache.GetOrAddAsync(GETALLFLOORSCACHEKEY, async (cache) =>
            {
                cache.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddHours(6));
                var floorEntities = await GetFloorEntities();
                return FloorEntity.MapToDomainObjects(floorEntities);
            });

            return floors;
        }

        public async Task<List<Floor>> GetFloors(List<Guid> floorIds)
		    {
            return (await GetFloors()).Where(f => floorIds.Contains(f.Id) && !f.IsDecomissioned).ToList();
		    }

        public async Task<List<Floor>> GetFloors(Guid siteId, bool all)
        {
            var floorsQuery = (await GetFloorEntities()).Where(f => f.SiteId == siteId && !f.IsDecomissioned);

            if (!all)
            {
                // Return all floors which have 2D "base"/"Architecture" image or any 3D module files
                floorsQuery = floorsQuery.Where(
                    f => f.Modules.Any(m => m.ModuleType.Is3D || m.ModuleType.Name == ModuleConstants.ModuleBaseName));
            }

            return FloorEntity.MapToDomainObjects(floorsQuery);
        }

        private Floor FloorOrNotFound(Floor floor, dynamic floordata)
        {
            if (floor == null)
            {
                throw new NotFoundException(floordata);
            }

            return floor;
        }

        public async Task<Floor> GetFloorById(Guid siteId, Guid floorId)
        {
            var floor = (await GetFloors()).FirstOrDefault(f => f.SiteId == siteId && f.Id == floorId);

            return FloorOrNotFound(floor, new { SiteId = siteId, FloorId = floorId });
        }

        public async Task<Floor> GetFloorByCode(Guid siteId, string floorCode)
        {
            var floor = (await GetFloors()).FirstOrDefault(f => f.SiteId == siteId && f.Code == floorCode);

            return FloorOrNotFound(floor, new { SiteId = siteId, FloorCode = floorCode });
        }

        public async Task<Floor> GetFloorBySiteId(Guid siteId)
        {
            var floorCode = "_SITE";

            var floor = (await GetFloors()).FirstOrDefault(f => f.SiteId == siteId && f.Code == floorCode);

            if (floor == null)
            {
                var _siteFloor = await CreateFloor(siteId, new CreateFloorRequest()
                {
                    Name = floorCode,
                    Code = floorCode
                });

                await DeleteFloor(siteId, _siteFloor.Id);

                return _siteFloor;
            }

            return floor;
        }

        public async Task UpdateSortOrder(Guid siteId, Guid[] floorIds)
        {
            var floors = await _dbContext.Floors.Where(f => f.SiteId == siteId && !f.IsDecomissioned).AsTracking().ToListAsync();

            if (floors.Count != floorIds.Length)
            {
                throw new ArgumentException("floorIds doesn't match floors count.");
            }

            floors.ForEach((f) =>
            {
                var index = Array.IndexOf(floorIds, f.Id);
                f.SortOrder = index;
            });

            await _dbContext.SaveChangesAsync();

            ResetCache();
        }

        private List<ModuleTypeEntity> _moduleTypeInstanceCache;
        private async Task<ModuleTypeEntity> DetermineModuleTypeAsync(Guid siteId, string fileName, bool is3D, bool skipFilenameMatch = false)
        {
            _moduleTypeInstanceCache = _moduleTypeInstanceCache ?? await _dbContext.ModuleTypes.Where(x => x.SiteId == siteId).ToListAsync();

            var moduleTypes = _moduleTypeInstanceCache.Where(m => m.Is3D == is3D).ToList();

            ModuleTypeEntity bestMatch = null;

            if (skipFilenameMatch)
            {
                bestMatch = moduleTypes.FirstOrDefault();
            }
            else
            {
                foreach (var moduleType in moduleTypes)
                {
                    if (fileName.StartsWith(moduleType.Prefix, StringComparison.InvariantCultureIgnoreCase) &&
                        (bestMatch == null || moduleType.Prefix.Length > bestMatch.Prefix.Length))
                    {
                        bestMatch = moduleType;
                    }
                }
            }

            if (bestMatch != null)
            {
                return bestMatch;
            }

            _logger.LogWarning(
                $"Cannot determine module for file {fileName} (3D: {is3D}, SiteId {siteId}).",
                fileName,
                is3D,
                _moduleTypeInstanceCache.Count,
                moduleTypes.Count,
                siteId);
            return null;
        }

        private class ProcessingItem
        {
            public IFormFile InputFile { get; set; }

            public string Url { get; set; }

            public string ModuleName { get; set; }

            public ModuleTypeEntity DeterminedModuleType { get; set; }

            public ModuleEntity ExistingModule { get; set; }

            public int? ImageWidth { get; set; }

            public int? ImageHeight { get; set; }

            public bool Is3D { get; set; }
        }

        private class ErrorDescriptor
        {
            public string Name { get; set; }

            public string Message { get; set; }
        }

        private static async Task<(int width, int height)> GetImageDimensions(IFormFile file)
        {
            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var bitmap = await Image.LoadAsync(ms);
            return (bitmap.Width, bitmap.Height);
        }

        private async Task<List<ErrorDescriptor>> ValidateModulesList(
            ICollection<ProcessingItem> processingItems,
            ICollection<ModuleEntity> existingModules)
        {
            var errors = new List<ErrorDescriptor>();

            if (!processingItems.Any())
            {
                return errors;
            }

            int? packageWidth = null;
            int? packageHeight = null;

            foreach (var processingItem in processingItems)
            {
                if (processingItem.DeterminedModuleType == null)
                {
                    errors.Add(
                        new ErrorDescriptor {
                            Name = processingItem.ModuleName,
                            Message = "Could not determine module type for provided file"
                        });
                    continue;
                }

                if (!processingItem.Is3D)
                {
                    var fileExtension = Path.GetExtension(processingItem.InputFile.FileName).ToLowerInvariant();
                    if (_moduleOptions.Modules2D.AllowedExtensions.Contains(fileExtension))
                    {
                        try
                        {
                            var dimensions = await GetImageDimensions(processingItem.InputFile);
                            processingItem.ImageHeight = dimensions.height;
                            processingItem.ImageWidth = dimensions.width;

                            packageWidth = packageWidth ?? processingItem.ImageWidth;
                            packageHeight = packageHeight ?? processingItem.ImageHeight;
                        }
                        catch (Exception e)
                        {
                            errors.Add(
                                new ErrorDescriptor {
                                    Name = processingItem.InputFile.FileName,
                                    Message = "Invalid data: " + e.Message
                                });
                        }
                    }
                    else
                    {
                        errors.Add(
                            new ErrorDescriptor {
                                Name = processingItem.InputFile.FileName,
                                Message = "File extension is not allowed"
                            });
                    }

                    if (processingItem.ImageHeight.HasValue && processingItem.ImageWidth.HasValue)
                    {
                        var existingItemsWithDifferentSize = existingModules.Where(ex =>
                            ex.ImageHeight.HasValue && ex.ImageHeight != processingItem.ImageHeight ||
                            ex.ImageWidth.HasValue && ex.ImageWidth != processingItem.ImageWidth).ToList();

                        if (existingItemsWithDifferentSize.Any())
                        {
                            var firstItem = existingItemsWithDifferentSize.First();
                            errors.Add(new ErrorDescriptor
                            {
                                Name = processingItem.InputFile.FileName,
                                Message = $"Update failed. New image should have the same dimensions as the existing image ({firstItem.ImageWidth}x{firstItem.ImageHeight}) pixels"
                            });
                        }

                        if (processingItem.ImageHeight.Value > _moduleOptions.Modules2D.MaxHeight ||
                            processingItem.ImageWidth.Value > _moduleOptions.Modules2D.MaxWidth)
                        {
                            errors.Add(new ErrorDescriptor
                            {
                                Name = processingItem.InputFile.FileName,
                                Message = $"Image dimensions exceed limitation of {_moduleOptions.Modules2D.MaxWidth}x{_moduleOptions.Modules2D.MaxHeight} pixels"
                            });
                        }

                        if (packageHeight.HasValue
                            && packageWidth.HasValue
                            && (processingItem.ImageHeight != packageHeight ||
                                processingItem.ImageWidth != packageWidth))
                        {
                            errors.Add(
                                new ErrorDescriptor {
                                    Name = processingItem.InputFile.FileName,
                                    Message = $"All uploading images should have same dimensions"
                                });
                        }

                        if (processingItem.ExistingModule != null &&
                            processingItem.ExistingModule.ImageHeight.HasValue &&
                            processingItem.ExistingModule.ImageWidth.HasValue
                            && (processingItem.ImageHeight != processingItem.ExistingModule.ImageHeight ||
                                processingItem.ImageWidth != processingItem.ExistingModule.ImageWidth))
                        {
                            errors.Add(new ErrorDescriptor
                            {
                                Name = processingItem.InputFile.FileName,
                                Message = $"When updating new one should have same dimensions as existing: {processingItem.ExistingModule.ImageWidth}x{processingItem.ExistingModule.ImageHeight} pixels"
                            });
                        }
                    }

                    if (processingItem.InputFile.Length > _moduleOptions.Modules2D.MaxSizeBytes)
                    {
                        errors.Add(
                            new ErrorDescriptor {
                                Name = processingItem.InputFile.FileName,
                                Message = $"File size exceeds limitation of {_moduleOptions.Modules2D.MaxSizeBytes} bytes"
                            });
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(processingItem.Url))
                    {
                        errors.Add(
                            new ErrorDescriptor {
                                Name = processingItem.ModuleName,
                                Message = $"3D modules should contain non-empty URL."
                            });
                    }
                }
            }

            var duplicateModuleTypes = processingItems.Where(item => item.DeterminedModuleType != null)
                .GroupBy(item => item.DeterminedModuleType.Id)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.Select(item => new { item.ModuleName, ModuleTypeName = item.DeterminedModuleType.Name }))
                .ToList();

            foreach (var duplicateModuleType in duplicateModuleTypes)
            {
                errors.Add(
                    new ErrorDescriptor {
                        Name = duplicateModuleType.ModuleName,
                        Message = $"Impossible to upload multiple modules of the same type: {duplicateModuleType.ModuleTypeName}"
                    });
            }

            return errors;
        }

        public async Task<Floor> Upload3DFloorModules(Guid siteId, Guid floorId, CreateUpdateModule3DRequest request)
        {
            FloorEntity floorEntity = _dbContext.Floors
                .Where(f => f.Id == floorId)
                .Where(f => f.SiteId == siteId)
                .Include(f => f.Site)
                .FirstOrDefault();

            if (floorEntity == null)
            {
                throw new NotFoundException(new { SiteId = siteId, FloorId = floorId });
            }

            await Generate3DModuleTypes(siteId, request.Modules3D);

            var existingModules = await _dbContext.Modules
                .Include(di => di.ModuleType)
                .Where(di => di.FloorId == floorId)
                .ToListAsync();

            var processingList = new List<ProcessingItem>();

            foreach (var moduleInfo in request.Modules3D)
            {
                var item = new ProcessingItem();
                item.Url = moduleInfo.Url;
                item.ModuleName = moduleInfo.ModuleName;
                item.DeterminedModuleType = await DetermineModuleTypeAsync(siteId, moduleInfo.ModuleName, true, skipFilenameMatch: floorEntity.Code == "_SITE");
                item.ExistingModule = existingModules.FirstOrDefault(m => item.DeterminedModuleType != null && m.ModuleTypeId == item.DeterminedModuleType.Id);
                item.Is3D = true;
                processingList.Add(item);
            }

            var errors = await ValidateModulesList(processingList, existingModules);
            if (errors.Any())
            {
                throw new ArgumentException("Can not process provided data");
            }

            foreach (var processingItem in processingList)
            {
                var dbModule = processingItem.ExistingModule;
                if (dbModule == null)
                {
                    dbModule = new ModuleEntity
                    {
                        Name = processingItem.ModuleName,
                        Id = Guid.NewGuid(),
                        FloorId = floorId,
                        ModuleTypeId = processingItem.DeterminedModuleType.Id,
                    };
                    _dbContext.Modules.Add(dbModule);
                }
                else
                {
                    _dbContext.Modules.Update(dbModule);
                }

                dbModule.Url = processingItem.Url;
                dbModule.Name = processingItem.ModuleName;
            }

            await _dbContext.SaveChangesAsync();

            ResetCache();

            return FloorEntity.MapToDomainObject(floorEntity);
        }

        private async Task Generate3DModuleTypes(Guid siteId, List<Module3DInfo> modules3D)
        {
            foreach (var moduleInfo in modules3D)
            {
                var moduleType = await DetermineModuleTypeAsync(siteId, moduleInfo.ModuleName, true);
                if (moduleType != null)
                {
                    continue;
                }

                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(moduleInfo.ModuleName);
                var pairs = fileNameWithoutExtension.Split("__");
                if (pairs.Length != 2)
                {
                    continue;
                }
                pairs[0] = pairs[0].Trim();
                pairs[1] = pairs[1].Trim();
                var moduleGroupName = pairs[0].Equals("default", StringComparison.InvariantCultureIgnoreCase) ? string.Empty : pairs[0];
                var name = pairs[1];
                var doesModuleTypeExist = await _dbContext.ModuleTypes
                    .Where(x => x.SiteId == siteId)
                    .Include(x => x.ModuleGroup)
                    .AnyAsync(x => x.Name == name &&
                                    (x.ModuleGroup == null ? string.Empty : x.ModuleGroup.Name).ToLower() == moduleGroupName.ToLower() &&
                                    x.Prefix == fileNameWithoutExtension && x.Is3D);

                if (doesModuleTypeExist)
                {
                    continue;
                }

                var moduleGroup = await _moduleGroupsService.GetModuleGroupEntityAsync(siteId, moduleGroupName);

                var newModuleType = new ModuleTypeEntity
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Prefix = fileNameWithoutExtension,
                    ModuleGroupId = moduleGroup?.Id,
                    SortOrder = 1,
                    CanBeDeleted = true,
                    Is3D = true,
                    SiteId = siteId
                };
                _dbContext.ModuleTypes.Add(newModuleType);
                await _dbContext.SaveChangesAsync();
                ResetCache();
            }
            _moduleTypeInstanceCache = null;
        }

        public async Task<Floor> Upload2DFloorModules(Guid siteId, Guid floorId, IFormFileCollection files)
        {
            FloorEntity floorEntity = _dbContext.Floors.Where(f => f.Id == floorId)
                .Where(f => f.SiteId == siteId)
                .Include(f => f.Site)
                .FirstOrDefault();

            if (floorEntity == null)
            {
                throw new NotFoundException(new { SiteId = siteId, FloorId = floorId });
            }

            var existingModules = await _dbContext.Modules
                .Include(di => di.ModuleType)
                .Where(di => di.FloorId == floorId)
                .ToListAsync();

            var processingList = new List<ProcessingItem>();

            foreach (var formFile in files)
            {
                var item = new ProcessingItem();
                item.InputFile = formFile;
                item.ModuleName = formFile.FileName;
                item.DeterminedModuleType = await DetermineModuleTypeAsync(siteId, formFile.FileName, false);
                item.ExistingModule = existingModules.FirstOrDefault(m => m.ModuleTypeId == item.DeterminedModuleType?.Id);
                item.Is3D = false;
                processingList.Add(item);
            }

            var errors = await ValidateModulesList(processingList, existingModules);

            var dbBaseModuleExists = existingModules.Any(m => m.ModuleType.Name.Equals(
                                        ModuleConstants.ModuleBaseName, StringComparison.InvariantCultureIgnoreCase));
            var requestBaseModuleAdded = processingList.Any(item =>
                item.DeterminedModuleType?.Name.Equals(ModuleConstants.ModuleBaseName,
                    StringComparison.InvariantCultureIgnoreCase) ?? false);

            if (!(dbBaseModuleExists || requestBaseModuleAdded))
            {
                var baseError = new ErrorDescriptor {
                    Name = "",
                    Message = $"The floor should either contain [{ModuleConstants.ModuleBaseName}] module or it should be uploaded along others"
                };
                errors.Add(baseError);
            }

            if (errors.Any())
            {
                throw new ArgumentException("Can not process provided data");
            }

            foreach (var processingItem in processingList)
            {
                var dbModule = processingItem.ExistingModule;
                if (dbModule == null)
                {
                    dbModule = new ModuleEntity
                    {
                        Name = processingItem.InputFile.FileName,
                        Id = Guid.NewGuid(),
                        FloorId = floorId,
                        ModuleTypeId = processingItem.DeterminedModuleType.Id,
                        ImageHeight = processingItem.ImageHeight,
                        ImageWidth = processingItem.ImageWidth
                    };
                    _dbContext.Modules.Add(dbModule);
                }
                else
                {
                    await _imageHubService.DeleteFloorModule(floorEntity.Site.CustomerId, floorEntity.SiteId, floorId, dbModule.VisualId);
                    _dbContext.Modules.Update(dbModule);
                }

                using (var stream = processingItem.InputFile.OpenReadStream())
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    var createResult = await _imageHubService.CreateFloorModule(floorEntity.Site.CustomerId, floorEntity.SiteId, floorId, memoryStream.ToArray(), processingItem.InputFile.FileName);

                    dbModule.VisualId = createResult.ImageId;
                    dbModule.Name = processingItem.InputFile.FileName;
                }
            }

            await _dbContext.SaveChangesAsync();

            ResetCache();

            return FloorEntity.MapToDomainObject(floorEntity);
        }

        public async Task<Floor> DeleteModule(Guid floorId, Guid moduleId)
        {
            var dbImage = await _dbContext.Modules
                .Where(di => di.FloorId == floorId)
                .Where(di => di.Id == moduleId)
                .Include(di => di.ModuleType)
                .Include(di => di.Floor)
                .ThenInclude(f => f.Site)
                .FirstOrDefaultAsync();

            if (dbImage == null)
            {
                throw new NotFoundException( new { FloorId = floorId, ModuleEntityName = nameof(ModuleEntity), ModuleId = moduleId});
            }

            if (!dbImage.ModuleType.CanBeDeleted)
            {
                throw new InvalidOperationException($"Module {moduleId} can't be deleted since it's of type {dbImage.ModuleType.Name} which can't be deleted");
            }

            if (!dbImage.ModuleType.Is3D)
            {
                await _imageHubService.DeleteFloorModule(dbImage.Floor.Site.CustomerId, dbImage.Floor.SiteId, floorId, dbImage.VisualId);
            }

            _dbContext.Modules.Remove(dbImage);

            await _dbContext.SaveChangesAsync();

            ResetCache();

            return await GetFloorAsync(dbImage.FloorId);
        }

        public async Task<Floor> GetFloorAsync(Guid floorId)
        {
            return (await GetFloors()).FirstOrDefault(f => f.Id == floorId);
        }

        public async Task<Floor> UpdateFloorAsync(Guid floorId, UpdateFloorRequest updateFloorRequest)
        {
            var entity = await _dbContext.Floors.FirstOrDefaultAsync(f => f.Id == floorId);
            if (entity == null)
            {
	            throw new ArgumentNullException("Floor does not exist!");
            }
            if (await _dbContext.Floors.AnyAsync(x => x.SiteId == entity.SiteId && x.Id != floorId && x.Code == updateFloorRequest.Code))
            {
                throw new ArgumentNullException("Floor code exists!");
            }
            if (updateFloorRequest.Name != null)
            {
                entity.Name = updateFloorRequest.Name;
            }
            if (updateFloorRequest.Code != null)
            {
                entity.Code = updateFloorRequest.Code;
            }
            if (updateFloorRequest.ModelReference != null)
            {
                if (updateFloorRequest.ModelReference == string.Empty)
                {
                    entity.ModelReference = null;
                }
                else
                {
	                var modelReference = Guid.Parse(updateFloorRequest.ModelReference);

					await CheckForDuplicates(entity.SiteId,modelReference, floorId);

                    entity.ModelReference = modelReference;
                }
            }

            if (updateFloorRequest.IsSiteWide.HasValue)
            {
                entity.IsSiteWide = updateFloorRequest.IsSiteWide.Value;
            }

            _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync();

            ResetCache();

            return FloorEntity.MapToDomainObject(entity);
        }

        private async Task CheckForDuplicates(Guid siteId, Guid? modelReference, Guid? floorId = null)
        {
            if (modelReference == null || modelReference == Guid.Empty)
            {
                return;
            }

            if (await _dbContext.Floors.AnyAsync(x => x.SiteId == siteId && x.ModelReference == modelReference && (!floorId.HasValue || x.Id != floorId)))
            {
                throw new ArgumentException("Floor model reference exists!");
            }
        }

        public async Task<Floor> UpdateFloorGeometryAsync(Guid floorId, UpdateFloorGeometryRequest request)
        {
            var entity = await _dbContext.Floors.FirstOrDefaultAsync(f => f.Id == floorId);
            entity.Geometry = request.Geometry;
            _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync();

            ResetCache();

            return FloorEntity.MapToDomainObject(entity);
        }

        public async Task InitializeSiteFloors(Guid siteId, IList<string> floorCodes)
        {
            var sortOrder = 0;
            foreach (var floorCode in floorCodes)
            {
                var floorEntity = new FloorEntity
                {
                    Id = Guid.NewGuid(),
                    SiteId = siteId,
                    Name = floorCode,
                    Code = floorCode,
                    SortOrder = sortOrder,
                    Geometry = string.Empty
                };
                sortOrder++;
                _dbContext.Add(floorEntity);
            }
            await _dbContext.SaveChangesAsync();

            ResetCache();
        }

        public async Task<bool> IsFloorExistByCode(Guid siteId, string floorCode)
        {
            var floorEntity = (await GetFloorEntities()).FirstOrDefault(
                                        f => f.SiteId == siteId && f.Code == floorCode);

            return floorEntity != null;
        }

        public async Task<Floor> CreateFloor(Guid siteId, CreateFloorRequest createFloorRequest)
        {
	        Guid? modelReference = string.IsNullOrEmpty(createFloorRequest.ModelReference)
		        ? null
		        : Guid.Parse(createFloorRequest.ModelReference);

			await CheckForDuplicates(siteId, modelReference);

            var floorEntity = new FloorEntity
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                Name = createFloorRequest.Name,
                Code = createFloorRequest.Code,
                SortOrder = 0,
                Geometry = string.Empty,
                ModelReference = modelReference,
                IsSiteWide = createFloorRequest.IsSiteWide
            };
            _dbContext.Add(floorEntity);

            await _dbContext.SaveChangesAsync();

            ResetCache();

            return FloorEntity.MapToDomainObject(floorEntity);
        }

        public async Task DeleteFloor(Guid siteId, Guid floorId)
        {
            var floorEntity = await _dbContext.Floors.AsTracking().FirstOrDefaultAsync(f => f.SiteId == siteId && f.Id == floorId);

            if (floorEntity == null)
            {
                throw new NotFoundException(new { SiteId = siteId, FloorId = floorId });
            }

            floorEntity.IsDecomissioned = true;
            await _dbContext.SaveChangesAsync();

            ResetCache();
        }
    }
}
