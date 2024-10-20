using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Willow.IoTService.Deployment.DataAccess.Db;
using Willow.IoTService.Deployment.DataAccess.Entities;

namespace Willow.IoTService.Deployment.DataAccess.Services;

public class ModuleDataService : IModuleDataService
{
    private readonly IDeploymentDbContext _dbContext;

    public ModuleDataService(IDeploymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ModuleDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var (module, deployment) = await GetModuleComponentsAsync(id, cancellationToken);

        return module == null
                   ? null
                   : ModuleDto.CreateFrom(module, deployment);
    }

    public async Task<PagedResult<(ModuleDto Module, IEnumerable<DeploymentDto>? Deployments)>> SearchAsync(ModuleSearchInput input, CancellationToken cancellationToken = default)
    {
        var skip = (input.Page - 1) * input.PageSize;
        var take = input.PageSize;
        var searched = _dbContext.Modules.Include(x => x.Config)
                                 .Include(x => x.Deployments)
                                 .Where(m => EF.Functions.Like(m.Name, $"%{input.Name}%")
                                             && (input.DeploymentIds == null || m.Deployments!.Any(d => input.DeploymentIds.Contains(d.Id)))
                                             && EF.Functions.Like(m.ModuleType, $"%{input.ModuleType}%")
                                             && (input.IsArchived == null || m.IsArchived == input.IsArchived)
                                             && (string.IsNullOrWhiteSpace(input.DeviceName)
                                                 || EF.Functions.Like(m.Config!.DeviceName, $"%{input.DeviceName}%")))
                                 .Select(x => new
                                  {
                                      Module = x,
                                      Deployment = x.Deployments!.OrderByDescending(y => y.DateTimeApplied)
                                                    .FirstOrDefault(),
                                      MatchingDeployments = input.DeploymentIds == null
                                                                ? null
                                                                : x.Deployments!.Where(d => input.DeploymentIds.Contains(d.Id))
                                                                   .OrderByDescending(y => y.DateTimeApplied)
                                                                   .AsEnumerable()
                                  });

        var count = await searched.CountAsync(cancellationToken);
        var list = await searched.OrderByDescending(x => x.Module.UpdatedOn)
                                 .Skip(skip)
                                 .Take(take)
                                 .Select(x => new
                                  {
                                      Module = ModuleDto.CreateFrom(x.Module, x.Deployment),
                                      x.MatchingDeployments
                                  })
                                 .ToListAsync(cancellationToken);
        var items = list.Select(m => (m.Module, m.MatchingDeployments?.Select(DeploymentDto.CreateFrom)));
        return new PagedResult<(ModuleDto Module, IEnumerable<DeploymentDto>? Deployments)>
        {
            Items = items,
            TotalCount = count
        };
    }

    public async Task<ModuleDto> UpsertAsync(ModuleUpsertInput input, CancellationToken cancellationToken = default)
    {
        var (module, deployment) = await GetModuleComponentsAsync(input.Id, cancellationToken);
        if (module == null)
        {
            module = new ModuleEntity { Id = input.Id ?? Guid.NewGuid() };
            await _dbContext.Modules.AddAsync(module, cancellationToken);
            // IsSynced has default value to be false
            // ef core will overwrite the value upon inserting
            // so we need to create a new entity then editing the property to avoid this
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        module.Name = input.Name;
        module.ModuleType = input.ModuleType;
        module.IsArchived = input.IsArchived;
        module.IsSynced = input.IsSynced;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ModuleDto.CreateFrom(module, deployment);
    }

    public async Task<ModuleDto> UpdateConfigurationAsync(ModuleUpdateConfigurationInput input, CancellationToken cancellationToken = default)
    {
        var (module, deployment) = await GetModuleComponentsAsync(input.Id, cancellationToken);
        Guard.Against.Null(module);

        var isNew = false;
        var moduleConfig = module.Config;
        if (moduleConfig == null)
        {
            isNew = true;
            moduleConfig = new ModuleConfigEntity { ModuleId = module.Id };
        }

        moduleConfig.Environment = input.Environment ?? moduleConfig.Environment;
        moduleConfig.IoTHubName = input.IoTHubName ?? moduleConfig.IoTHubName;
        moduleConfig.DeviceName = input.DeviceName ?? moduleConfig.DeviceName;
        moduleConfig.IsAutoDeployment = input.IsAutoDeployment ?? moduleConfig.IsAutoDeployment;
        moduleConfig.Platform = input.Platform ?? moduleConfig.Platform;

        if (isNew) await _dbContext.ModuleConfigs.AddAsync(moduleConfig, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ModuleDto.CreateFrom(module, deployment);
    }

    public async Task<PagedResult<(string moduleType, string latestVersion)>> GetModuleTypesAsync(ModuleTypesSearchInput input, CancellationToken cancellationToken = default)
    {
        var skip = (input.Page - 1) * input.PageSize;
        var take = input.PageSize;
        var query = from module in _dbContext.Modules
                    join moduleTypeVersion in _dbContext.ModuleTypeVersions on module.ModuleType equals moduleTypeVersion.ModuleType into moduleTypeVersions
                    from moduleVersion in moduleTypeVersions.DefaultIfEmpty()
                    where EF.Functions.Like(module.ModuleType, $"%{input.ModuleType}%")
                    select new
                    {
                        module.ModuleType,
                        moduleVersion.Major,
                        moduleVersion.Minor,
                        moduleVersion.Patch
                    }
                    into mv
                    group mv by mv.ModuleType
                    into g
                    orderby g.Key
                    select new
                    {
                        ModuleType = g.Key,
                        LatestMajor = g.Max(x => x.Major),
                        LatestMinor = g.Max(x => x.Minor),
                        LatestPatch = g.Max(x => x.Patch)
                    };

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip)
                               .Take(take)
                               .ToListAsync(cancellationToken);

        return new PagedResult<(string moduleType, string latestVersion)>
        {
            Items = items.Select(x => (x.ModuleType, x.LatestMajor == null ? "" : $"{x.LatestMajor}.{x.LatestMinor}.{x.LatestPatch}")),
            TotalCount = total
        };
    }

    public async Task<IEnumerable<string>> GetModuleTypeVersionsAsync(string moduleType, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ModuleTypeVersions.Where(x => x.ModuleType == moduleType)
                              .Select(x => x.Major == null ? "" : $"{x.Major}.{x.Minor}.{x.Patch}");
        var results = await query.ToListAsync(cancellationToken);

        return results;
    }

    public async Task<IEnumerable<string>> AddModuleTypeVersionsAsync(string moduleType,
                                                                      string version,
                                                                      CancellationToken cancellationToken = default)
    {
        var versionSplit = version.Split('.').Select(int.Parse).ToArray();
        int? major = null, minor = null, patch = null;
        if (versionSplit?.Length == 3) (major, minor, patch) = (versionSplit[0], versionSplit[1], versionSplit[2]);
        var exist = _dbContext.ModuleTypeVersions.Where(x => x.ModuleType == moduleType && x.Major == major && x.Minor == minor && x.Patch == patch);
        var query = _dbContext.ModuleTypeVersions.Where(x => x.ModuleType == moduleType)
                              .Select(x => x.Major == null ? string.Empty : $"{x.Major}.{x.Minor}.{x.Patch}");
        if (await exist.AnyAsync(cancellationToken)) return await query.ToListAsync(cancellationToken);

        await _dbContext.ModuleTypeVersions.AddAsync(new ModuleTypeVersionEntity
                                                     {
                                                         Id = Guid.NewGuid(),
                                                         ModuleType = moduleType,
                                                         Major = major,
                                                         Minor = minor,
                                                         Patch = patch
                                                     },
                                                     cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await query.ToListAsync(cancellationToken);
    }

    private async Task<(ModuleEntity? Module, DeploymentEntity? LatestDeployment)> GetModuleComponentsAsync(Guid? id, CancellationToken cancellationToken)
    {
        if (id == null) return (null, null);

        var query = _dbContext.Modules.Include(x => x.Config)
                              .Include(x => x.Deployments)
                              .Select(x => new
                               {
                                   Module = x,
                                   Deployment = x.Deployments!.OrderByDescending(y => y.DateTimeApplied)
                                                 .FirstOrDefault()
                               });

        var result = await query.SingleOrDefaultAsync(x => x.Module.Id == id, cancellationToken);

        return (result?.Module, result?.Deployment);
    }
}

public interface IModuleDataService
{
    Task<ModuleDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<(ModuleDto Module, IEnumerable<DeploymentDto>? Deployments)>> SearchAsync(ModuleSearchInput input, CancellationToken cancellationToken = default);
    Task<ModuleDto> UpsertAsync(ModuleUpsertInput input, CancellationToken cancellationToken = default);
    Task<ModuleDto> UpdateConfigurationAsync(ModuleUpdateConfigurationInput input, CancellationToken cancellationToken = default);
    Task<PagedResult<(string moduleType, string latestVersion)>> GetModuleTypesAsync(ModuleTypesSearchInput input, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetModuleTypeVersionsAsync(string moduleType, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> AddModuleTypeVersionsAsync(string moduleType,
                                                         string version,
                                                         CancellationToken cancellationToken = default);
}
