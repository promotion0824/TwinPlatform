namespace Willow.CommandAndControl.Data;

using Microsoft.EntityFrameworkCore.Storage;

internal class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
                            BaseEntitySaveChangesInterceptor? interceptor)
    : DbContext(options), IApplicationDbContext
{
    private readonly BaseEntitySaveChangesInterceptor? interceptor = interceptor;

    public virtual DbSet<ResolvedCommand> ResolvedCommands { get; set; }

    public virtual DbSet<RequestedCommand> RequestedCommands { get; set; }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ResolvedCommand>(action => action.ToTable(typeof(ResolvedCommand).Name));
        modelBuilder.Entity<RequestedCommand>(action => action.ToTable(typeof(RequestedCommand).Name));
        modelBuilder.Entity<ActivityLog>(action => action
        .ToTable(typeof(ActivityLog).Name));

        modelBuilder.Entity<RequestedCommand>(entity =>
        {
            entity.ToTable(typeof(RequestedCommand).Name);
            entity.HasIndex(rc => rc.TwinId);
            entity.HasIndex(rc => new
            {
                rc.TwinId,
                rc.IsCapabilityOf,
                rc.IsHostedBy,
                rc.Location,
                rc.ConnectorId,
                rc.ExternalId,
                rc.Unit,
            });
            entity.OwnsMany(nav => nav.Locations, action => action.ToJson());
        });

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (interceptor != null)
        {
            optionsBuilder.AddInterceptors(interceptor);
        }

        base.OnConfiguring(optionsBuilder);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }

    public IExecutionStrategy CreateExecutionStrategy()
    {
        return Database.CreateExecutionStrategy();
    }
}
