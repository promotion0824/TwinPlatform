using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using WorkflowCore.Models;
using WorkflowCore.Services.Apis;

using Willow.Data;

namespace WorkflowCore.Services
{
    public interface ISiteService
    { 
        Task<Site> GetSite(Guid siteId);
    }

    public class SiteService : ISiteService
    {
        private readonly ConcurrentDictionary<Guid, Site> _sites = new ConcurrentDictionary<Guid, Site>();
        private readonly IReadRepository<Guid, Site> _siteRepo;

        public SiteService(IReadRepository<Guid, Site> siteRepo)
        {
            _siteRepo = siteRepo;
        }

        public async Task<Site> GetSite(Guid siteId)
        {
            if(_sites.TryGetValue(siteId, out Site outSite))
                return outSite;

            var site = await _siteRepo.Get(siteId);

            _sites.TryAdd(siteId, site);

            return site;
        }
    }
}
