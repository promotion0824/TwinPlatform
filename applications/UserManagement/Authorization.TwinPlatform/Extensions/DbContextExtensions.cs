using System.Linq.Expressions;
using Authorization.TwinPlatform.Persistence.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Authorization.TwinPlatform.Extensions;

/// <summary>
/// Extension helper class for DB Context
/// </summary>
public static class DbContextExtensions
{
    public static Task<bool> AnyAsync<T>(this DbContext context, Expression<Func<T, bool>> predicate) where T : class, IEntityBase
    {
        return context.Set<T>().AsNoTracking().AnyAsync(predicate);
    }

    /// <summary>
    /// Apply Filter and Pagination to DB Context entity
    /// </summary>
    /// <typeparam name="TEntity">DB Context Entity Type</typeparam>
    /// <param name="context">DBContext</param>
    /// <param name="predicate">Filter Expression</param>
    /// <param name="skip">Number of Records to Skip</param>
    /// <param name="take">Number of Records to Take</param>
    /// <returns>Entity IQueryable instance</returns>
    public static IQueryable<TEntity> ApplyFilter<TEntity>(
    this DbContext context,
    string? filterQuery,
    Expression<Func<TEntity, bool>> predicate,
    int? skip = null,
    int? take = null) where TEntity : class, IEntityBase
    {

        var query = context.Set<TEntity>()
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filterQuery))
        {
            query = query.Where(filterQuery);
        }

        if (predicate is not null)
            query = query.Where(predicate);

        if (skip.HasValue)
            query = query.Skip(skip.Value);

        if (take.HasValue)
            query = query.Take(take.Value);

        return query;
    }

    public static Task<List<TModel>> ProjectAsync<TEntity, TModel>(
        this DbContext context,
        IMapper mapper) where TEntity : class, IEntityBase
    {
        return context.Set<TEntity>()
            .AsNoTracking()
            .ProjectTo<TModel>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    /// <summary>
    /// Add Entity to the Database Context
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="context">Database context</param>
    /// <param name="entity">Entity instance</param>
    /// <param name="saveChanges">True to save; false if not</param>
    /// <returns>Id if saved; else return null</returns>
    public static async Task<Guid> AddEntityAsync<T>(
        this DbContext context,
        T entity, bool saveChanges = true, bool detachEntryPostSave = false) where T : class, IEntityBase
    {
        var entityEntry = await context.Set<T>().AddAsync(entity);

        if (saveChanges)
        {
            await context.SaveEntityChanges(entity);
        }

        if (detachEntryPostSave)
        {
            context.Set<T>().Entry(entity).State = EntityState.Detached;
        }

        return entityEntry.Entity.Id;
    }

    public static async Task RemoveRangeAsync<T>(
        this DbContext context,
        Expression<Func<T, bool>> predicate) where T : class, IEntityBase
    {
        var dbSet = context.Set<T>();
        dbSet.RemoveRange(dbSet.Where(predicate));

        await context.SaveChangesAsync();
    }

    public static async Task RemoveEntityAsync<T>(this DbContext context, T entity, bool saveChanges = true) where T : class, IEntityBase
    {
        context.Entry(entity).State = EntityState.Deleted;
        if (saveChanges)
        {
            await context.SaveEntityChanges(entity);
        }
    }

    public static Task<T?> SingleAsync<T>(this DbContext context, Expression<Func<T, bool>> predicate) where T : class, IEntityBase
    {
        return context.Set<T>().FirstOrDefaultAsync(predicate);
    }

    public static async Task<T> UpdateAsync<T>(this DbContext context, T entityToUpdate, bool saveChanges = true) where T : class, IEntityBase
    {
        context.Attach(entityToUpdate);
        context.Entry(entityToUpdate).State = EntityState.Modified;
        if (saveChanges)
        {
            await context.SaveEntityChanges(entityToUpdate);
        }
        return entityToUpdate;
    }

    private static async Task SaveEntityChanges<T>(this DbContext context, T entityToDetachOnFailure) where T : class, IEntityBase
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Remove the entity from the Database context that caused the exception 
            context.Entry(entityToDetachOnFailure!).State = EntityState.Detached;
            throw;
        }
    }
}
