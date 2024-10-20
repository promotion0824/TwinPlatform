namespace Willow.CommandAndControl.Data;

using Microsoft.EntityFrameworkCore.Storage;

internal interface IDbTransactions
{
    Task RunAsync(IApplicationDbContext dbContext, Func<IDbContextTransaction, Task<bool>> act);
}
