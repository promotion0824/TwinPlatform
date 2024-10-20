using System;
using System.Threading;
using System.Threading.Tasks;
using DigitalTwinCore.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace DigitalTwinCore.Repositories;

public interface IUnitOfWork
{

    Task<int> SaveChangesAsync(CancellationToken cancellationToken );
    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
public class UnitOfWork:IUnitOfWork,IDisposable
{
    private readonly DigitalTwinDbContext _dbContext;
    private IDbContextTransaction _dbContextTransaction;
    private bool _disposed;
    private int _transactionCount;

    public UnitOfWork(DigitalTwinDbContext dbContext)=>_dbContext=dbContext;
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _transactionCount);
        if(_transactionCount==1)
        { _dbContextTransaction=await _dbContext.Database.BeginTransactionAsync(cancellationToken);}
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        Interlocked.Decrement(ref _transactionCount);
        await SaveChangesAsync(cancellationToken);

        if (_transactionCount == 0 && _dbContextTransaction != null)
        {
            await _dbContextTransaction.CommitAsync(cancellationToken);
            _dbContextTransaction.Dispose();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        Interlocked.Add(ref _transactionCount, -1 * _transactionCount);

        if (_dbContextTransaction != null)
        {
            await _dbContextTransaction.RollbackAsync(cancellationToken);
            _dbContextTransaction.Dispose();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _dbContext.Dispose();
        }

        _disposed = true;
    }
    public void Dispose()
    { 
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}