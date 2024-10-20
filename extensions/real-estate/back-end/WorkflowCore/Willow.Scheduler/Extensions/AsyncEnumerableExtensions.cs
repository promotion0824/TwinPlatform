using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Willow.Scheduler
{
    public static class AsyncEnumerableExtensions
    {
        public static async Task<IList<T>> ToList<T>(this IAsyncEnumerable<T> e)
        {
            var list = new List<T>();

            await foreach(var item in e)
                list.Add(item);

            return list;
        }
    }
}
