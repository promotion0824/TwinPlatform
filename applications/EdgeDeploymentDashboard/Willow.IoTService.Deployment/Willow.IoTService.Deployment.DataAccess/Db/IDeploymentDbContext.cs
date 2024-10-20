using Microsoft.EntityFrameworkCore;
using Willow.IoTService.Deployment.DataAccess.Entities;

namespace Willow.IoTService.Deployment.DataAccess.Db;

public interface IDeploymentDbContext
{
    public DbSet<DeploymentEntity> Deployments { get; }
    public DbSet<ModuleEntity> Modules { get; }
    public DbSet<ModuleConfigEntity> ModuleConfigs { get; }
    public DbSet<ModuleTypeVersionEntity> ModuleTypeVersions { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
