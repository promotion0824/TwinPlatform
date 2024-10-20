using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

namespace Willow.Data
{
    public class CachedRepository<TID, TVALUE> : IReadRepository<TID, TVALUE>
    {
        private readonly IReadRepository<TID, TVALUE> _sourceRepo;
        private readonly IMemoryCache   _cache;
        private readonly TimeSpan       _absoluteExpiration;
        private readonly string         _prefix;

        public CachedRepository(IReadRepository<TID, TVALUE> sourceRepo, IMemoryCache cache, TimeSpan absoluteExpiration, string prefix = null)
        {
            _sourceRepo         = sourceRepo ?? throw new ArgumentNullException(nameof(sourceRepo));
            _cache              = cache ?? throw new ArgumentNullException(nameof(cache));
            _absoluteExpiration = absoluteExpiration;
            _prefix             = prefix;
        }

        public async Task<TVALUE> Get(TID id)
        {
            var key = (_prefix ?? "") + id.ToString();

            if(_cache.TryGetValue<TVALUE>(key, out var cachedItem))
            {
               return cachedItem;
            }

            var item = await _sourceRepo.Get(id);

            _cache.Set(key, item, DateTime.UtcNow + _absoluteExpiration);

            return item;
        }

        public virtual async IAsyncEnumerable<TVALUE> Get(IEnumerable<TID> ids)
        {
            foreach(var id in ids)
                yield return await Get(id);
        }
    }
}
