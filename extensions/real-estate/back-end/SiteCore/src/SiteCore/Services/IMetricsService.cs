using SiteCore.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiteCore.Requests;

namespace SiteCore.Services
{
    public interface IMetricsService
    {
        Task<List<SiteMetrics>> GetCurrentMetricsForAllSitesAsync();
        Task<SiteMetrics> GetCurrentMetricsForSiteAsync(Guid siteId);
        Task<List<SiteMetrics>> GetMetricsForAllSitesAsync(DateTime start, DateTime end);
        Task<SiteMetrics> GetMetricsForSiteAsync(Guid siteId, DateTime start, DateTime end);
        Task ImportSiteMetricsAsync(Guid siteId, ImportSiteMetricsRequest request);
    }
}
