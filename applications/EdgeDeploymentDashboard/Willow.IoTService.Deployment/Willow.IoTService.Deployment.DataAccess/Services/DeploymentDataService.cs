using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Willow.IoTService.Deployment.DataAccess.Db;
using Willow.IoTService.Deployment.DataAccess.Entities;

namespace Willow.IoTService.Deployment.DataAccess.Services;

public class DeploymentDataService : IDeploymentDataService
{
    private readonly IDeploymentDbContext _dbContext;

    public DeploymentDataService(IDeploymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DeploymentDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Deployments.Include(x => x.Module)
                                     .ThenInclude(x => x.Config)
                                     .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity == null
                   ? null
                   : DeploymentDto.CreateFrom(entity);
    }

    public async Task<PagedResult<DeploymentDto>> SearchAsync(DeploymentSearchInput input, CancellationToken cancellationToken = default)
    {
        var skip = (input.Page - 1) * input.PageSize;
        var take = input.PageSize;
        var searched = _dbContext.Deployments.Include(x => x.Module)
                                 .ThenInclude(x => x.Config)
                                 .Where(x => (input.ModuleId == null || x.Module.Id == input.ModuleId)
                                             && (input.Ids == null || input.Ids.Contains(x.Id))
                                             && (string.IsNullOrWhiteSpace(input.DeviceName)
                                                 || EF.Functions.Like(x.Module.Config!.DeviceName, $"%{input.DeviceName}%")));
        var count = await searched.CountAsync(cancellationToken);
        var list = await searched.OrderByDescending(x => x.DateTimeApplied)
                                 .Skip(skip)
                                 .Take(take)
                                 .Select(x => DeploymentDto.CreateFrom(x))
                                 .ToListAsync(cancellationToken);
        return new PagedResult<DeploymentDto>
        {
            Items = list,
            TotalCount = count
        };
    }

    public async Task<DeploymentDto> CreateAsync(DeploymentCreateInput input, CancellationToken cancellationToken = default)
    {
        var module = await _dbContext.Modules.SingleOrDefaultAsync(x => x.Id == input.ModuleId, cancellationToken);
        Guard.Against.Null(module);
        var id = Guid.NewGuid();
        var entity = new DeploymentEntity
        {
            Id = id,
            Module = module,
            Status = input.Status,
            StatusMessage = input.StatusMessage,
            Version = input.Version,
            AssignedBy = input.AssignedBy,
            DateTimeApplied = input.DateTimeApplied,
            Name = id.ToString("N")
        };

        await _dbContext.Deployments.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return DeploymentDto.CreateFrom(entity);
    }

    public async Task<DeploymentDto> UpdateStatusAsync(DeploymentStatusUpdateInput input, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Deployments.Include(x => x.Module)
                                       .SingleOrDefaultAsync(x => x.Id == input.Id, cancellationToken);
        Guard.Against.Null(existing);

        existing.Status = input.Status;
        existing.StatusMessage = input.StatusMessage;
        existing.DateTimeApplied = input.DateTimeApplied;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return DeploymentDto.CreateFrom(existing);
    }

    public async Task<IEnumerable<DeploymentDto>> CreateMultipleAsync(IEnumerable<DeploymentCreateInput> inputs, CancellationToken cancellationToken = default)
    {
        var moduleQuery = from input in inputs
                          join m in _dbContext.Modules on input.ModuleId equals m.Id into grouping
                          from m in grouping.DefaultIfEmpty()
                          where m != null
                          select new
                          {
                              input,
                              m
                          };

        var pairs = moduleQuery.ToList();

        var deploymentsToAdd = pairs.Select(pair =>
                                     {
                                         var id = Guid.NewGuid();
                                         var input = pair.input;
                                         var module = pair.m;
                                         var entity = new DeploymentEntity
                                         {
                                             Id = id,
                                             Module = module,
                                             Status = input.Status,
                                             StatusMessage = input.StatusMessage,
                                             Version = input.Version,
                                             AssignedBy = input.AssignedBy,
                                             DateTimeApplied = input.DateTimeApplied,
                                             Name = id.ToString("N")
                                         };
                                         return entity;
                                     })
                                    .ToList();

        await _dbContext.Deployments.AddRangeAsync(deploymentsToAdd, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return deploymentsToAdd.Select(DeploymentDto.CreateFrom);
    }
}

public interface IDeploymentDataService
{
    Task<DeploymentDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<DeploymentDto>> SearchAsync(DeploymentSearchInput input, CancellationToken cancellationToken = default);
    Task<DeploymentDto> CreateAsync(DeploymentCreateInput input, CancellationToken cancellationToken = default);
    Task<DeploymentDto> UpdateStatusAsync(DeploymentStatusUpdateInput input, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeploymentDto>> CreateMultipleAsync(IEnumerable<DeploymentCreateInput> inputs, CancellationToken cancellationToken = default);
}
