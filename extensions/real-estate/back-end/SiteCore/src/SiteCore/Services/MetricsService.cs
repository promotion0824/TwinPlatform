using Microsoft.EntityFrameworkCore;
using SiteCore.Domain;
using SiteCore.Entities;
using SiteCore.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiteCore.Services
{
    public class MetricsService : IMetricsService
    {
        private readonly SiteDbContext _dbContext;

        public MetricsService(SiteDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SiteMetrics>> GetCurrentMetricsForAllSitesAsync()
        {
            var metricEntities = await _dbContext.Metrics.AsNoTracking().OrderBy(m => m.ParentId).ThenBy(m => m.Id).ToListAsync();

            var siteMetricValueEntities = await _dbContext.SiteMetricValues
                .Where(v => v.TimeStamp ==
                    (_dbContext.SiteMetricValues
                        .Where(v2 =>
                             v2.SiteId == v.SiteId &&
                             v2.MetricId == v.MetricId)
                        .Max(g => g.TimeStamp)
                    )
                )
                .AsNoTracking()
                .ToListAsync();

            return siteMetricValueEntities
                .GroupBy(v => v.SiteId)
                .Select(g => new SiteMetrics
                {
                    SiteId = g.Key,
                    Metrics = BuildMetricTree(metricEntities, g.ToList())
                })
                .ToList();
        }

        public async Task<SiteMetrics> GetCurrentMetricsForSiteAsync(Guid siteId)
        {
            var metricEntities = await _dbContext.Metrics.AsNoTracking().OrderBy(m => m.ParentId).ThenBy(m => m.Id).ToListAsync();

            var siteMetricValueEntities = await _dbContext.SiteMetricValues
                .Where(v => v.SiteId == siteId && v.TimeStamp ==
                    (_dbContext.SiteMetricValues
                        .Where(v2 =>
                             v2.SiteId == siteId &&
                             v2.MetricId == v.MetricId)
                        .Max(g => g.TimeStamp)
                    )
                )
                .AsNoTracking()
                .ToListAsync();

            return new SiteMetrics
            {
                SiteId = siteId,
                Metrics = BuildMetricTree(metricEntities, siteMetricValueEntities)
            };
        }

        public async Task<List<SiteMetrics>> GetMetricsForAllSitesAsync(DateTime start, DateTime end)
        {
            var metricEntities = await _dbContext.Metrics.AsNoTracking().OrderBy(m => m.ParentId).ThenBy(m => m.Id).ToListAsync();

            var siteMetricValueEntities = await _dbContext.SiteMetricValues
                .Where(v => v.TimeStamp >= start && v.TimeStamp < end)
                .AsNoTracking()
                .ToListAsync();

            return siteMetricValueEntities
                .GroupBy(v => v.SiteId)
                .Select(g => new SiteMetrics
                {
                    SiteId = g.Key,
                    Metrics = BuildMetricTree(metricEntities, g.ToList())
                })
                .ToList();
        }

        public async Task<SiteMetrics> GetMetricsForSiteAsync(Guid siteId, DateTime start, DateTime end)
        {
            var metricEntities = await _dbContext.Metrics.AsNoTracking().OrderBy(m => m.ParentId).ThenBy(m => m.Id).ToListAsync();

            var siteMetricValueEntities = await _dbContext.SiteMetricValues
                .Where(v => v.SiteId == siteId && v.TimeStamp >= start && v.TimeStamp < end)
                .AsNoTracking()
                .ToListAsync();

            return new SiteMetrics
            {
                SiteId = siteId,
                Metrics = BuildMetricTree(metricEntities, siteMetricValueEntities)
            };
        }

        public async Task ImportSiteMetricsAsync(Guid siteId, ImportSiteMetricsRequest request)
        {
            var metricEntities = await _dbContext.Metrics
                .AsNoTracking()
                .OrderBy(m => m.ParentId)
                .ThenBy(m => m.Id)
                .ToListAsync();

            foreach (var item in request.Metrics)
            {
                var metricEntity = FindMetricEntity(metricEntities, item.Key);

                if (metricEntity != null)
                {
                    _dbContext.SiteMetricValues.Add(new SiteMetricValueEntity 
                    { 
                        Id = Guid.NewGuid(), 
                        MetricId = metricEntity.Id, 
                        SiteId = siteId, 
                        TimeStamp = request.TimeStamp, 
                        Value = item.Value 
                    });
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        private MetricEntity FindMetricEntity(List<MetricEntity> metricEntities, string key)
        {
            MetricEntity metric = null;
            foreach (var keyPart in key.Split('/'))
            {
                metric = metricEntities.SingleOrDefault(m => m.ParentId == metric?.Id && m.Key.Equals(keyPart, StringComparison.InvariantCultureIgnoreCase));
                if (metric == null)
                {
                    return null;
                }
            }

            return metric;
        }

        private static List<Metric> BuildMetricTree(List<MetricEntity> metricEntities, List<SiteMetricValueEntity> siteMetricValueEntities)
        {
            var output = new List<Metric>();

            foreach (var entity in metricEntities)
            {
                var model = Metric.MapFrom(entity);
                if (entity.ParentId.HasValue)
                {
                    var parent = output.Single(m => m.Id == entity.ParentId);
                    parent.Metrics.Add(model);
                }
                else
                {
                    output.Add(model);
                }

                model.Values = siteMetricValueEntities
                    .Where(v => v.MetricId == model.Id)
                    .Select(MetricValue.MapFrom)
                    .ToList();
            }

            return output;
        }
    }
}
