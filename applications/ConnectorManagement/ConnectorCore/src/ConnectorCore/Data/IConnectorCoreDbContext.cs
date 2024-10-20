namespace ConnectorCore.Data;

using ConnectorCore.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

internal interface IConnectorCoreDbContext
{
    DbSet<Connector> Connectors { get; }

    DbSet<Scan> Scans { get; }

    DbSet<Log> Logs { get; }

    DbSet<ConnectorType> ConnectorTypes { get; }

    DbSet<Schema> Schemas { get; }

    DbSet<SchemaColumn> SchemaColumns { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task MigrateAsync(CancellationToken cancellationToken = default);

    IQueryable<T> SqlQuery<T>(FormattableString sql);

    Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    IExecutionStrategy CreateExecutionStrategy();
}
