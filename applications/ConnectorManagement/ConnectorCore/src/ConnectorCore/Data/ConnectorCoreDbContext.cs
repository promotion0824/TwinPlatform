namespace ConnectorCore.Data;

using ConnectorCore.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// DbContext for Connector Core.
/// </summary>
/// <param name="options">Db Context Option.</param>
internal class ConnectorCoreDbContext(DbContextOptions<ConnectorCoreDbContext> options) : DbContext(options), IConnectorCoreDbContext
{
    public DbSet<Connector> Connectors { get; set; }

    public DbSet<ConnectorType> ConnectorTypes { get; set; }

    public DbSet<Log> Logs { get; set; }

    public DbSet<Scan> Scans { get; set; }

    public DbSet<Schema> Schemas { get; set; }

    public DbSet<SchemaColumn> SchemaColumns { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Connector>(entity =>
        {
            entity.ToTable(nameof(Connector));
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.Configuration)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.RegistrationId)
                .HasMaxLength(64);

            entity.Property(e => e.RegistrationKey)
                .HasMaxLength(256);

            entity.Property(e => e.ConnectionType)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.LastUpdatedAt)
                .HasDefaultValueSql("sysutcdatetime()");

            entity.Property(e => e.IsArchived)
                .HasDefaultValue(false);

            entity.HasOne(d => d.ConnectorType)
                .WithMany(p => p.Connectors)
                .HasForeignKey(d => d.ConnectorTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasIndex(e => e.ConnectorTypeId)
                .HasDatabaseName("Idx_Connector_ConnectorTypeId");

            entity.HasIndex(e => e.SiteId)
                .HasDatabaseName("Idx_Connector_SiteId");
        });

        modelBuilder.Entity<ConnectorType>(entity =>
        {
            entity.ToTable(nameof(ConnectorType));

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(64);

            entity.HasIndex(e => e.ConnectorConfigurationSchemaId)
                .HasDatabaseName("Idx_ConnectorType_ConnectorConfigurationSchemaId");

            entity.HasIndex(e => e.DeviceMetadataSchemaId)
                .HasDatabaseName("Idx_ConnectorType_DeviceMetadataSchemaId");

            entity.HasIndex(e => e.PointMetadataSchemaId)
                .HasDatabaseName("Idx_ConnectorType_PointMetadataSchemaId");

            entity.HasIndex(e => e.ScanConfigurationSchemaId)
                .HasDatabaseName("Idx_ConnectorType_ScanConfigurationSchemaId");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Errors)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.Source)
                .HasColumnType("nvarchar(max)");

            entity.HasOne(d => d.Connector)
                .WithMany(p => p.Logs)
                .HasForeignKey(d => d.ConnectorId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasIndex(e => e.ConnectorId)
                .HasDatabaseName("Idx_Logs_ConnectorId");

            entity.HasIndex(e => e.EndTime)
                .HasDatabaseName("IX_Logs_EndTime")
                .IncludeProperties(e => new { e.ConnectorId, e.PointCount, e.ErrorCount });

            entity.HasIndex(e => e.StartTime)
                .HasDatabaseName("IX_Logs_StartTime")
                .IncludeProperties(e => new { e.ConnectorId, e.Source });
        });

        modelBuilder.Entity<Scan>(entity =>
        {
            entity.ToTable(nameof(Scan));

            entity.Property(e => e.Status)
                .HasMaxLength(32);

            entity.Property(e => e.Message)
                .HasMaxLength(1024);

            entity.HasIndex(e => e.ConnectorId)
                .HasDatabaseName("Idx_Scan_ConnectorId");
        });

        modelBuilder.Entity<Schema>(entity =>
        {
            entity.ToTable(nameof(Schema));

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.Type)
                .HasMaxLength(64);

            entity.HasIndex(e => e.ClientId)
                .HasDatabaseName("Idx_Schema_ClientId");
        });

        modelBuilder.Entity<SchemaColumn>(entity =>
        {
            entity.ToTable(nameof(SchemaColumn));

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.DataType)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.UnitOfMeasure)
                .IsRequired()
                .HasMaxLength(64)
                .HasDefaultValue(string.Empty);
        });
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await Database.MigrateAsync(cancellationToken);
    }

    public IQueryable<T> SqlQuery<T>(FormattableString sql)
    {
        return Database.SqlQuery<T>(sql);
    }

    public Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken = default)
    {
        return Database.ExecuteSqlAsync(sql, cancellationToken);
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
