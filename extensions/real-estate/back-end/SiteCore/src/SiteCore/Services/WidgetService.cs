using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteCore.Domain;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Services.DigitalTwinCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.ExceptionHandling.Exceptions;

namespace SiteCore.Services
{
    public interface IWidgetService
    {
        Task AddScopedFromSiteWidgetsAsync(int batchSize, CancellationToken stoppingToken);
        Task AddScopedFromPortfolioWidgetsAsync(int batchSize, CancellationToken stoppingToken);

        Task AddWidgetToScope(string scopeId, Guid widgetId, string position);
        Task DeleteWidgetFromScope(string scopeId, Guid widgetId);
        Task<List<Widget>> GetWidgetsByScopeId(string scopeId);

        Task AddWidgetToSite(Guid siteId, Guid widgetId, string position);
        Task AddWidgetToPortfolio(Guid portfolioId, Guid widgetId, int position);
        Task DeleteWidgetFromSite(Guid siteId, Guid widgetId);
        Task DeleteWidgetFromPortfolio(Guid portfolioId, Guid widgetId);
        Task<List<Widget>> GetAllWidgets();
        Task<List<Widget>> GetWidgetsBySiteId(Guid siteId);
        Task<List<Widget>> GetWidgetsByPortfolioId(Guid portfolioId);
        Task<Widget> GetWidget(Guid widgetId);
        Task<Widget> CreateWidget(CreateUpdateWidgetRequest request);
        Task<Widget> UpdateWidget(Guid widgetId, CreateUpdateWidgetRequest request);
        Task DeleteWidget(Guid widgetId, bool? resetLinked = null);
    }

    public class WidgetService : IWidgetService
    {
        private readonly SiteDbContext _dbContext;
        private readonly IDigitalTwinCoreApiService _digitalTwinCoreApi;
        private readonly ILogger<WidgetService> _logger;

        public WidgetService(SiteDbContext dbContext, IDigitalTwinCoreApiService digitalTwinCoreApi, ILogger<WidgetService> logger)
        {
            _dbContext = dbContext;
            _digitalTwinCoreApi = digitalTwinCoreApi;
            _logger = logger;
        }

        public async Task<List<SiteWidgetEntity>> GetPagedSiteWidgetsAsync(int pageNumber, int batchSize)
        {
            return await _dbContext.SiteWidgets.OrderBy(x => x.SiteId).Skip((pageNumber - 1) * batchSize).Take(batchSize).ToListAsync();
        }

        public async Task<List<PortfolioWidgetEntity>> GetPagedPortfolioWidgetsAsync(int pageNumber, int batchSize)
        {
            return await _dbContext.PortfolioWidgets.OrderBy(x => x.PortfolioId).Skip((pageNumber - 1) * batchSize).Take(batchSize).ToListAsync();
        }

        public async Task AddScopedFromSiteWidgetsAsync(int batchSize, CancellationToken stoppingToken)
        {
            var pageNumber = 1;

            var widgets = await GetPagedSiteWidgetsAsync(pageNumber, batchSize);

            while (widgets != null && widgets.Any())
            {
                var widgetsGroupedBySite = widgets.GroupBy(c => c.SiteId);

                foreach (var site in widgetsGroupedBySite)
                {
                    try
                    {
                        var siteWidgets = site.ToList();
                        var siteTwinIds = await _digitalTwinCoreApi.GetTwinIdsByUniqueIdsAsync(site.Key, siteWidgets.Select(c => c.SiteId).Distinct());

                        if (siteTwinIds != null && siteTwinIds.Any())
                        {
                            foreach (var widget in siteWidgets)
                            {
                                var scopeWidget = new ScopeWidgetEntity()
                                {
                                    WidgetId = widget.WidgetId,
                                    ScopeId = siteTwinIds.FirstOrDefault(c => c.UniqueId.Equals(widget.SiteId.ToString(), StringComparison.InvariantCultureIgnoreCase))?.Id,
                                    Position = "0"
                                };

                                if (_dbContext.ScopeWidgets.FirstOrDefault(x => x.ScopeId == scopeWidget.ScopeId && x.WidgetId == scopeWidget.WidgetId) == null)
                                {
                                    _dbContext.ScopeWidgets.Add(scopeWidget);
                                }
                            }
                        }
                        await _dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to get twinId in AddScopedFromSiteWidgetsAsync {Message}", ex.Message);
                    }

                    pageNumber += 1;
                    widgets = await GetPagedSiteWidgetsAsync(pageNumber, batchSize);
                }
            }
        }

        public async Task AddScopedFromPortfolioWidgetsAsync(int batchSize, CancellationToken stoppingToken)
        {
            var pageNumber = 1;

            var widgets = await GetPagedPortfolioWidgetsAsync(pageNumber, batchSize);

            while (widgets != null && widgets.Any())
            {
                var widgetsGroupedByPortfolio = widgets.GroupBy(c => c.PortfolioId);

                foreach (var portfolio in widgetsGroupedByPortfolio)
                {
                    try
                    {
                        var portfolioWidgets = portfolio.ToList();

                        foreach (var widget in portfolioWidgets)
                        {
                            var scopeWidget = new ScopeWidgetEntity()
                            {
                                WidgetId = widget.WidgetId,
                                ScopeId = portfolio.Key.ToString(),
                                Position = "0"
                            };

                            if (_dbContext.ScopeWidgets.FirstOrDefault(x => x.ScopeId == scopeWidget.ScopeId && x.WidgetId == scopeWidget.WidgetId) == null)
                            {
                                _dbContext.ScopeWidgets.Add(scopeWidget);
                            }
                        }
                        await _dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed in AddScopedFromPortfolioWidgetsAsync {Message}", ex.Message);
                    }

                    pageNumber += 1;
                    widgets = await GetPagedPortfolioWidgetsAsync(pageNumber, batchSize);
                }
            }
        }

        public async Task AddWidgetToScope(string scopeId, Guid widgetId, string position)
        {
            if (!(await _dbContext.Widgets.AnyAsync(c => c.Id == widgetId)))
            {
                throw new NotFoundException(new { ScopeId = scopeId, WidgetId = widgetId });
            }

            _dbContext.ScopeWidgets.Add(new ScopeWidgetEntity { ScopeId = scopeId, WidgetId = widgetId, Position = position });

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteWidgetFromScope(string scopeId, Guid widgetId)
        {
            if (!(await _dbContext.Widgets.AnyAsync(c => c.Id == widgetId)))
            {
                throw new NotFoundException(new { ScopeId = scopeId, WidgetId = widgetId });
            }

            var scopeWidget = _dbContext.ScopeWidgets.FirstOrDefault(x => x.ScopeId == scopeId && x.WidgetId == widgetId);

            if (scopeWidget != null)
            {
                _dbContext.ScopeWidgets.Remove(scopeWidget);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Widget>> GetWidgetsByScopeId(string scopeId)
        {
            var scopeWidgetEntities = await _dbContext.ScopeWidgets
                                    .Where(sw => sw.ScopeId == scopeId)
                                    .Include(sw => sw.Widget)
                                    .ToListAsync();
            return ScopeWidgetEntity.MapToDomainObjects(scopeWidgetEntities);
        }

        private async Task ResetScopeWidgets(Guid widgetId, IEnumerable<WidgetPosition> positions = null)
        {
            var siteWidgetEntities = await _dbContext.ScopeWidgets.Where(s => s.WidgetId == widgetId).ToListAsync();
            _dbContext.ScopeWidgets.RemoveRange(siteWidgetEntities);

            if (positions != null)
            {
                var scopePositions = positions.Where(x => x.ScopeId != null)
                    .Select(x => new ScopeWidgetEntity { ScopeId = x.ScopeId, WidgetId = widgetId, Position = x.Position.ToString() })
                    .ToList();

                _dbContext.ScopeWidgets.AddRange(scopePositions);
            }
        }

        private async Task ResetSiteWidgets(Guid widgetId, IEnumerable<WidgetPosition> positions = null)
        {
            var siteWidgetEntities = await _dbContext.SiteWidgets.Where(s => s.WidgetId == widgetId).ToListAsync();
            _dbContext.SiteWidgets.RemoveRange(siteWidgetEntities);

            if (positions != null)
            {
                var sitePositions = positions.Where(x => x.SiteId != null && _dbContext.Sites.Any(s => s.Id == x.SiteId))
                    .Select(x => new SiteWidgetEntity { SiteId = x.SiteId.Value, WidgetId = widgetId, Position = x.Position.ToString() })
                    .ToList();

                _dbContext.SiteWidgets.AddRange(sitePositions);
            }
        }

        private async Task ResetPortfolioWidgets(Guid widgetId, IEnumerable<WidgetPosition> positions = null)
        {
            var portfolioWidgetEntities = await _dbContext.PortfolioWidgets.Where(s => s.WidgetId == widgetId).ToListAsync();
            _dbContext.PortfolioWidgets.RemoveRange(portfolioWidgetEntities);

            if (positions != null)
            {
                var portfolioPositions = positions.Where(x => x.PortfolioId != null)
                    .Select(x => new PortfolioWidgetEntity { PortfolioId = x.PortfolioId.Value, WidgetId = widgetId, Position = x.Position })
                    .ToList();

                _dbContext.PortfolioWidgets.AddRange(portfolioPositions);
            }
        }

        public async Task AddWidgetToSite(Guid siteId, Guid widgetId, string position)
        {
            if (!(await _dbContext.Sites.AnyAsync(c => c.Id == siteId)))
            {
                throw new NotFoundException(new { SiteId = siteId });
            }

            if (!(await _dbContext.Widgets.AnyAsync(c => c.Id == widgetId)))
            {
                throw new NotFoundException(new { SiteId = siteId, WidgetId = widgetId });
            }

            _dbContext.SiteWidgets.Add(new SiteWidgetEntity { SiteId = siteId, WidgetId = widgetId, Position = position });

            await _dbContext.SaveChangesAsync();
        }

        public async Task AddWidgetToPortfolio(Guid portfolioId, Guid widgetId, int position)
        {
            if (!(await _dbContext.Widgets.AnyAsync(c => c.Id == widgetId)))
            {
                throw new NotFoundException(new { PortfolioId = portfolioId, WidgetId = widgetId });
            }

            _dbContext.PortfolioWidgets.Add(new PortfolioWidgetEntity { PortfolioId = portfolioId, WidgetId = widgetId, Position = position });

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteWidgetFromSite(Guid siteId, Guid widgetId)
        {
            if (!(await _dbContext.Sites.AnyAsync(c => c.Id == siteId)))
            {
                throw new NotFoundException(new { SiteId = siteId });
            }

            if (!(await _dbContext.Widgets.AnyAsync(c => c.Id == widgetId)))
            {
                throw new NotFoundException(new { SiteId = siteId, WidgetId = widgetId });
            }

            _dbContext.SiteWidgets.Remove(new SiteWidgetEntity { SiteId = siteId, WidgetId = widgetId });

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteWidgetFromPortfolio(Guid portfolioId, Guid widgetId)
        {
            if (!(await _dbContext.Widgets.AnyAsync(c => c.Id == widgetId)))
            {
                throw new NotFoundException(new { PortfolioId = portfolioId, WidgetId = widgetId });
            }

            _dbContext.PortfolioWidgets.Remove(new PortfolioWidgetEntity { PortfolioId = portfolioId, WidgetId = widgetId });

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Widget>> GetAllWidgets()
        {
            var widgetEntities = await _dbContext.Widgets
                                .Include(x => x.SiteWidgets)
                                .Include(x => x.PortfolioWidgets)
                                .Include(x => x.ScopeWidgets)
                                .ToListAsync();
            return WidgetEntity.MapToDomainObjects(widgetEntities);
        }

        public async Task<List<Widget>> GetWidgetsBySiteId(Guid siteId)
        {
            var siteWidgetEntities = await _dbContext.SiteWidgets
                                    .Where(sw => sw.SiteId == siteId)
                                    .Include(sw => sw.Widget)
                                    .Include(sw => sw.Site)
                                    .ToListAsync();
            return SiteWidgetEntity.MapToDomainObjects(siteWidgetEntities);
        }

        public async Task<List<Widget>> GetWidgetsByPortfolioId(Guid portfolioId)
        {
            var portfolioWidgetEntities = await _dbContext.PortfolioWidgets
                                            .Where(pw => pw.PortfolioId == portfolioId)
                                            .Include(pw => pw.Widget)
                                            .ToListAsync();

            return PortfolioWidgetEntity.MapToDomainObjects(portfolioWidgetEntities);
        }

        public async Task<Widget> GetWidget(Guid widgetId)
        {
            var entity = await _dbContext.Widgets
                        .Where(x => x.Id == widgetId)
                        .Include(x => x.SiteWidgets)
                        .Include(x => x.PortfolioWidgets)
                        .FirstOrDefaultAsync();

            if (entity == null)
            {
                throw new NotFoundException( new { WidgetId = widgetId });
            }

            return WidgetEntity.MapToDomainObject(entity);
        }

        public async Task<Widget> CreateWidget(CreateUpdateWidgetRequest request)
        {
            var widget = CreateUpdateWidgetRequest.MapToDomainObject(request);

            var widgetEntity = WidgetEntity.MapFrom(widget);

            _dbContext.Widgets.Add(widgetEntity);

            await ResetScopeWidgets(widgetEntity.Id, request.Positions);
            await ResetSiteWidgets(widgetEntity.Id, request.Positions);
            await ResetPortfolioWidgets(widgetEntity.Id, request.Positions);

            await _dbContext.SaveChangesAsync();

            return WidgetEntity.MapToDomainObject(widgetEntity);
        }

        public async Task<Widget> UpdateWidget(Guid widgetId, CreateUpdateWidgetRequest request)
        {
            var entity = await _dbContext.Widgets.AsTracking().FirstOrDefaultAsync(x => x.Id == widgetId);

            if (entity == null)
            {
                throw new NotFoundException( new { WidgetId = widgetId });
            }

            request.MapTo(entity);

            await ResetScopeWidgets(widgetId, request.Positions);
            await ResetSiteWidgets(widgetId, request.Positions);
            await ResetPortfolioWidgets(widgetId, request.Positions);

            await _dbContext.SaveChangesAsync();

            return WidgetEntity.MapToDomainObject(entity);
        }

        public async Task DeleteWidget(Guid widgetId, bool? resetLinked = null)
        {
            var isLinked = await _dbContext.SiteWidgets.AnyAsync(sr => sr.WidgetId == widgetId) || await _dbContext.PortfolioWidgets.AnyAsync(sr => sr.WidgetId == widgetId);

            if (isLinked)
            {
                if (!(resetLinked ?? false))
                {
                    throw new ArgumentException("Can't delete Widget that is linked to a Site or a Portfolio.");
                }

                await ResetScopeWidgets(widgetId);
                await ResetSiteWidgets(widgetId);
                await ResetPortfolioWidgets(widgetId);
            }

            var widgetEntity = new WidgetEntity
            {
                Id = widgetId
            };

            _dbContext.Widgets.Remove(widgetEntity);

            await _dbContext.SaveChangesAsync();
        }
    }
}
