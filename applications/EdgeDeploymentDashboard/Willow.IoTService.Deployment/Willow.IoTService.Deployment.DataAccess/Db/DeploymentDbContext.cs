using Microsoft.EntityFrameworkCore;
using Willow.IoTService.Deployment.DataAccess.Entities;

namespace Willow.IoTService.Deployment.DataAccess.Db;

public class DeploymentDbContext : DbContext, IDeploymentDbContext
{
    private readonly BaseEntitySaveChangesInterceptor? _interceptor;

    public DeploymentDbContext(DbContextOptions<DeploymentDbContext> options, BaseEntitySaveChangesInterceptor? interceptor = null) : base(options)
    {
        _interceptor = interceptor;
    }

    public DbSet<DeploymentEntity> Deployments => Set<DeploymentEntity>();
    public DbSet<ModuleEntity> Modules => Set<ModuleEntity>();
    public DbSet<ModuleConfigEntity> ModuleConfigs => Set<ModuleConfigEntity>();
    public DbSet<ModuleTypeVersionEntity> ModuleTypeVersions => Set<ModuleTypeVersionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeploymentDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_interceptor != null) optionsBuilder.AddInterceptors(_interceptor);

        base.OnConfiguring(optionsBuilder);
    }
}
