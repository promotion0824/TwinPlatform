using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Memory
{
    public static class MemoryCacheExtensions
    {
        private static readonly Dictionary<object, SemaphoreSlim> Semaphores = new Dictionary<object, SemaphoreSlim>();

        private static SemaphoreSlim GetSemaphore(object key)
        {
            lock (Semaphores)
            {
                if (Semaphores.TryGetValue(key, out var semaphore))
                {
                    return semaphore;
                }

                semaphore = new SemaphoreSlim(1, 1);
                Semaphores[key] = semaphore;
                return semaphore;
            }
        }

        public static async Task<TItem> GetOrCreateWithLockAsync<TItem>(this IMemoryCache cache, object key, Func<ICacheEntry, Task<TItem>> factory)
        {
            if (cache.TryGetValue(key, out TItem data))
            {
                return data;
            }

            var semaphore = GetSemaphore(key);
            await semaphore.WaitAsync();
            try
            {
                var result = await cache.GetOrCreateAsync(key, factory);
                return result;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}