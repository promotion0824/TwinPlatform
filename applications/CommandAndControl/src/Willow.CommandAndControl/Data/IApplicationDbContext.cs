namespace Willow.CommandAndControl.Data;

using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// DB Context for Command and Control API.
/// </summary>
internal interface IApplicationDbContext : IDisposable
{
    DbSet<ResolvedCommand> ResolvedCommands { get; }

    DbSet<RequestedCommand> RequestedCommands { get; }

    DbSet<ActivityLog> ActivityLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    IExecutionStrategy CreateExecutionStrategy();
}
