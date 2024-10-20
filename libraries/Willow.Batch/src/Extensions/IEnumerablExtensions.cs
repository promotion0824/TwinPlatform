namespace Willow.Batch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Extensions for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class IEnumerablExtensions
    {
        /// <summary>
        /// Filters a collection of items by a set of filter specifications.
        /// </summary>
        /// <typeparam name="T">The type of the object in the enumerable.</typeparam>
        /// <param name="query">The IEnumerable to query.</param>
        /// <param name="filterSpecifications">A collection of filter specifications.</param>
        /// <param name="whereExpression">A where clause to apply to the collection.</param>
        /// <returns>A filtered collection of type T.</returns>
        public static IEnumerable<T> FilterBy<T>(this IEnumerable<T> query, IEnumerable<FilterSpecificationDto> filterSpecifications, Expression<Func<T, bool>> whereExpression = null)
        {
            if (filterSpecifications != null && filterSpecifications.Any())
            {
                foreach (var filterSpecification in filterSpecifications)
                {
                    var filterExpression = filterSpecification.Build<T>();

                    whereExpression = whereExpression == null ? filterExpression : whereExpression.And(filterExpression);
                }

                query = whereExpression is null ? query : query.Where(whereExpression.Compile());
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
        public static IEnumerable<T> SortBy<T>(this IEnumerable<T> items, SortSpecificationDto[] specs)
        {
            if (specs != null && specs.Any() && items != null && items.Any())
            {
                foreach (var spec in specs ?? System.Array.Empty<SortSpecificationDto>())
                {
                    items = items is IOrderedEnumerable<T> orderedItems ? spec.ThenTo(orderedItems) : spec.ApplyTo(items);
                }
            }

            return items;
        }

        private static IOrderedEnumerable<T> ApplyTo<T>(this SortSpecificationDto spec, IEnumerable<T> items)
        {
            return spec.IsSortDescending
                ? items?.OrderByDescending(x => x.GetType().GetProperty(spec.Field)?.GetValue(x))
                : items?.OrderBy(x => x.GetType().GetProperty(spec.Field)?.GetValue(x));
        }

        private static IOrderedEnumerable<T> ThenTo<T>(this SortSpecificationDto spec, IOrderedEnumerable<T> items)
        {
            return spec.IsSortDescending
                ? items?.ThenByDescending(x => x.GetType().GetProperty(spec.Field)?.GetValue(x))
                : items?.ThenBy(x => x.GetType().GetProperty(spec.Field)?.GetValue(x));
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
        public static BatchDto<TU> Paginate<T, TU>(this IEnumerable<T> items, int? page, int? take, Func<IEnumerable<T>, IEnumerable<TU>> mapper)
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

            return batched;
        }
    }
}
