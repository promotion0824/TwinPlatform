namespace Willow.Batch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Extensions for <see cref="IQueryable{T}"/>.
    /// </summary>
    public static class IQueryableExtensions
    {
        /// <summary>
        /// Filters a queryable collection of items by a set of filter specifications.
        /// </summary>
        /// <typeparam name="T">The type of the object in the enumerable.</typeparam>
        /// <param name="query">The IEnumerable to query.</param>
        /// <param name="filterSpecifications">A collection of filter specifications.</param>
        /// <param name="whereExpression">A where clause to apply to the collection.</param>
        /// <returns>A filtered collection of type T.</returns>
        public static IQueryable<T> FilterBy<T>(this IQueryable<T> query, IEnumerable<FilterSpecificationDto> filterSpecifications, Expression<Func<T, bool>> whereExpression = null)
        {
            if (filterSpecifications != null && filterSpecifications.Any())
            {
                foreach (var filterSpecification in filterSpecifications)
                {
                    var filterExpression = filterSpecification.Build<T>();

                    whereExpression = whereExpression == null ? filterExpression : whereExpression.And(filterExpression);
                }

                query = whereExpression is null ? query : query.Where(whereExpression);
            }

            return query;
        }

        /// <summary>
        /// Sorts a collection of items by a set of sort specifications.
        /// </summary>
        /// <typeparam name="T">The type of the object in the enumerable.</typeparam>
        /// <param name="items">The items in the collection.</param>
        /// <param name="specs">The sort specifications.</param>
        /// <returns>The sorted collection.</returns>
        public static IQueryable<T> SortBy<T>(this IQueryable<T> items, SortSpecificationDto[] specs)
        {
            if (specs != null && specs.Any() && items != null && items.Any())
            {
                foreach (var spec in specs ?? System.Array.Empty<SortSpecificationDto>())
                {
                    items = (items.Expression.Type == typeof(IOrderedQueryable<T>))
                        ? (items as IOrderedQueryable<T>).ApplyOrder(spec.Field, spec.IsSortDescending ? "ThenByDescending" : "ThenBy")
                        : items.ApplyOrder(spec.Field, spec.IsSortDescending ? "OrderByDescending" : "OrderBy");
                }
            }

            return items;
        }

        // https://stackoverflow.com/questions/41244/dynamic-linq-orderby-on-ienumerablet-iqueryablet
        private static IOrderedQueryable<T> ApplyOrder<T>(this IQueryable<T> items, string propertyName, string methodName)
        {
            var parameter = Expression.Parameter(typeof(T), "x");

            var property = parameter.GetProperty(propertyName);

            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), property.Type);

            var lambda = Expression.Lambda(delegateType, property, parameter);

            object result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), property.Type)
                    .Invoke(null, new object[] { items, lambda });

            return (IOrderedQueryable<T>)result;
        }

        /// <summary>
        /// Paginates a collection of items.
        /// </summary>
        /// <typeparam name="T">The type of the object in the enumerable.</typeparam>
        /// <typeparam name="TU">The type of the objects in the mapper.</typeparam>
        /// <param name="items">The collection of items to paginate.</param>
        /// <param name="page">The page number to return.</param>
        /// <param name="take">The number of items per page.</param>
        /// <param name="mapper">A collection of batch mappers.</param>
        /// <returns>A batch of DTOs.</returns>
        public static Task<BatchDto<TU>> Paginate<T, TU>(this IQueryable<T> items, int? page, int? take, Func<IEnumerable<T>, IEnumerable<TU>> mapper)
        {
            var batched = new BatchDto<TU>();

            if (items != null)
            {
                batched.Total = items.Count();

                if (page.HasValue && take.HasValue && page.Value > 0)
                {
                    batched.Before = (page.Value - 1) * take.Value;

                    items = items.Skip(batched.Before);
                }

                if (take.HasValue && take.Value > 0 && take.Value < 1000000)
                {
                    items = items.Take(take.Value);
                }

                batched.Items = mapper(items).ToArray();
                batched.After = batched.Total - (batched.Before + batched.Items.Count());
            }

            return Task.FromResult(batched);
        }

        /// <summary>
        /// Paginates a queryable collection.
        /// </summary>
        /// <typeparam name="TU">The type of the objects in the collection.</typeparam>
        /// <param name="items">The collection of objects.</param>
        /// <param name="page">The page number.</param>
        /// <param name="take">How many items per page.</param>
        /// <returns>A paginated collection of items.</returns>
        public static Task<BatchDto<TU>> Paginate<TU>(this IQueryable<TU> items, int? page, int? take)
        {
            return items.Paginate(page, take, (x) => x);
        }
    }
}
