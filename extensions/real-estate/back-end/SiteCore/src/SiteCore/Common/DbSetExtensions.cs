using System;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore
{
    public static class DbSetExtensions
    {
        public static void RemoveRange<TEntity>(
            this DbSet<TEntity> dbSet, 
            Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            var entities = dbSet.Where(predicate);
            dbSet.RemoveRange(entities);
        }
    }
}
