using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Willow.Data
{
    public static class RepositoryExtensions
    {
        public static Task<T> Get<T>(this IReadRepository<Guid, T> repo, Guid id)
        {
            return repo.Get(id);
        }

        public static IAsyncEnumerable<T> Get<T>(this IReadRepository<Guid, T> repo, IEnumerable<Guid> ids)
        {
            return repo.Get(ids);
        }

        public static Task<T> Get<T>(this IReadRepository<SiteObjectIdentifier, T> repo, Guid siteId, Guid id)
        {
            return repo.Get(new SiteObjectIdentifier(siteId, id));
        }

        public static IAsyncEnumerable<T> Get<T>(this IReadRepository<SiteObjectIdentifier, T> repo, Guid siteId, IEnumerable<Guid> ids)
        {
            return repo.Get(ids.Select( id=> new SiteObjectIdentifier(siteId, id)));
        }
    }
}
