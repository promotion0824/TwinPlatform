using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Willow.Common
{
    public static class AsyncEnumerableExtensions
    {
        /// <summary>
        /// Converts an IAsyncEnumerable to an IList
        /// </summary>
        public static async Task<IList<T>> ToList<T>(this IAsyncEnumerable<T> e)
        {
            var list = new List<T>();

            await foreach(var item in e)
                list.Add(item);

            return list;
        }
    }
}
