using System;
using System.Linq;
using Willow.Expressions;

namespace Willow.Units
{
    /// <summary>
    /// Linq extensions for filtering and sorting by TokenExpressions
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// OrderBy a TokenExpression
        /// </summary>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, TokenExpression sortExpression, bool ascending)
        {
            if (sortExpression.Type == typeof(string))
            {
                var sortOrder = sortExpression.Convert<T, string>();
                if (ascending)
                    return Queryable.OrderBy(source, sortOrder);
                else
                    return Queryable.OrderByDescending(source, sortOrder);
            }
            else if (sortExpression.Type == typeof(DateTime))
            {
                var sortOrder = sortExpression.Convert<T, DateTime>();
                if (ascending)
                    return Queryable.OrderBy(source, sortOrder);
                else
                    return Queryable.OrderByDescending(source, sortOrder);
            }
            else if (sortExpression.Type == typeof(DateTimeOffset))
            {
                var sortOrder = sortExpression.Convert<T, DateTimeOffset>();
                if (ascending)
                    return Queryable.OrderBy(source, sortOrder);
                else
                    return Queryable.OrderByDescending(source, sortOrder);
            }
            else if (sortExpression.Type == typeof(double))
            {
                var sortOrder = sortExpression.Convert<T, double>();
                if (ascending)
                    return Queryable.OrderBy(source, sortOrder);
                else
                    return Queryable.OrderByDescending(source, sortOrder);
            }
            else if (sortExpression.Type == typeof(decimal))
            {
                var sortOrder = sortExpression.Convert<T, decimal>();
                if (ascending)
                    return Queryable.OrderBy(source, sortOrder);
                else
                    return Queryable.OrderByDescending(source, sortOrder);
            }
            else if (sortExpression.Type == typeof(int))
            {
                var sortOrder = sortExpression.Convert<T, int>();
                if (ascending)
                    return Queryable.OrderBy(source, sortOrder);
                else
                    return Queryable.OrderByDescending(source, sortOrder);
            }
            else
            {
                throw new ArgumentException($"Linq extension does not support {sortExpression.Type.Name} yet for {sortExpression.Serialize()}");
            }
        }

        /// <summary>
        /// OrderBy a TokenExpression
        /// </summary>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, TokenExpression sortExpression)
        {
            return OrderBy(source, sortExpression, true);
        }

        /// <summary>
        /// OrderByDescending a TokenExpression
        /// </summary>
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, TokenExpression sortExpression)
        {
            return OrderBy(source, sortExpression, false);
        }

        /// <summary>
        /// Where a TokenExpression
        /// </summary>
        public static IQueryable<T> Where<T>(this IQueryable<T> source, TokenExpression filterExpression)
        {
            var filter = filterExpression.Convert<T, bool>();
            return Queryable.Where(source, filter);
        }
    }
}
