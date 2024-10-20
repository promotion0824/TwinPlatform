namespace Willow.CommandAndControl.Data;

using Microsoft.EntityFrameworkCore.Storage;

internal class DbTransactions : IDbTransactions
{
    public async Task RunAsync(IApplicationDbContext dbContext, Func<IDbContextTransaction, Task<bool>> act)
    {
        if (dbContext != null && act != null)
        {
            var executionStrategy = dbContext.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await dbContext.BeginTransactionAsync();
                if (transaction != null)
                {
                    try
                    {
                        if (await act.Invoke(transaction))
                        {
                            await transaction.CommitAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("Error during transaction, rolling back", e);
                    }
                }
                else
                {
                    throw new Exception("Error while starting transaction");
                }
            });
        }
    }
}
