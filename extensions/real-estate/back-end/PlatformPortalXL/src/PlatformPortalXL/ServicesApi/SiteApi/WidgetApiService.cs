using PlatformPortalXL.Requests.SiteCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.Api.Client;
using Willow.Platform.Models;

namespace PlatformPortalXL.ServicesApi.SiteApi
{
    public interface IWidgetApiService
    {
        Task<List<Widget>> GetWidgetsByScopeId(string scopeId);
        Task AddWidgetToScope(string scopeId, AddWidgetRequest request);
        Task DeleteWidgetFromScope(string scopeId, Guid widgetId);
        Task<List<Widget>> GetWidgetsBySiteId(Guid siteId);
        Task<List<Widget>> GetWidgetsBySiteId(string siteId);
        Task AddWidgetToSite(Guid siteId, AddWidgetRequest request);
        Task DeleteWidgetFromSite(Guid siteId, Guid widgetId);
        Task<List<Widget>> GetWidgetsByPortfolioId(Guid portfolioId, bool? includeSiteWidgets);
        Task<List<Widget>> GetWidgetsByPortfolioId(string portfolioId, bool? includeSiteWidgets);
        Task<List<Widget>> GetWidgetsByPortfolioId(string portfolioId);
        Task AddWidgetToPortfolio(Guid portfolioId, AddWidgetRequest request);
        Task DeleteWidgetFromPortfolio(Guid portfolioId, Guid widgetId);
        Task<Widget> CreateWidget(CreateUpdateWidgetRequest request);
        Task<Widget> UpdateWidget(Guid widgetId, CreateUpdateWidgetRequest request);
        Task DeleteWidget(Guid widgetId, bool? resetLinked);
    }

    public class WidgetApiService : IWidgetApiService
    {
        private readonly IRestApi _siteApi;

        public WidgetApiService(IRestApi siteApi)
        {
            _siteApi = siteApi;
        }

        public Task<List<Widget>> GetWidgetsByScopeId(string scopeId)
        {
            return _siteApi.Get<List<Widget>>($"scopes/{scopeId}/widgets");
        }

        public Task AddWidgetToScope(string scopeId, AddWidgetRequest request)
        {
            return _siteApi.PostCommand($"scopes/{scopeId}/widgets", request);
        }

        public Task DeleteWidgetFromScope(string scopeId, Guid widgetId)
        {
            return _siteApi.Delete($"scopes/{scopeId}/widgets/{widgetId}");
        }

        public Task<List<Widget>> GetWidgetsBySiteId(Guid siteId)
        {
            return GetWidgetsBySiteId(siteId.ToString());
        }

        public Task<List<Widget>> GetWidgetsBySiteId(string siteId)
        {
            return _siteApi.Get<List<Widget>>($"sites/{siteId}/widgets");
        }

        public Task AddWidgetToSite(Guid siteId, AddWidgetRequest request)
        {
            return _siteApi.PostCommand($"sites/{siteId}/widgets", request);
        }

        public Task DeleteWidgetFromSite(Guid siteId, Guid widgetId)
        {
            return _siteApi.Delete($"sites/{siteId}/widgets/{widgetId}");
        }

        public Task<List<Widget>> GetWidgetsByPortfolioId(string portfolioId, bool? includeSiteWidgets)
        {
            var url = $"portfolios/{portfolioId}/widgets";

            if (includeSiteWidgets.HasValue)
            {
                url += $"?includeSiteWidgets={includeSiteWidgets}";
            }

            return _siteApi.Get<List<Widget>>(url);
        }

        public Task<List<Widget>> GetWidgetsByPortfolioId(string portfolioId)
        {
            return GetWidgetsByPortfolioId(portfolioId, null);
        }

        public Task<List<Widget>> GetWidgetsByPortfolioId(Guid portfolioId, bool? includeSiteWidgets)
        {
            return GetWidgetsByPortfolioId(portfolioId.ToString(), includeSiteWidgets);
        }

        public Task AddWidgetToPortfolio(Guid portfolioId, AddWidgetRequest request)
        {
            return _siteApi.PostCommand($"portfolios/{portfolioId}/widgets", request);
        }

        public Task DeleteWidgetFromPortfolio(Guid portfolioId, Guid widgetId)
        {
            return _siteApi.Delete($"portfolios/{portfolioId}/widgets/{widgetId}");
        }

        public Task<List<Widget>> GetAllWidgets()
        {
            return _siteApi.Get<List<Widget>>($"internal-management/widgets");
        }

        public Task<Widget> CreateWidget(CreateUpdateWidgetRequest request)
        {
            return _siteApi.Post<CreateUpdateWidgetRequest, Widget>($"internal-management/widgets", request);
        }

        public Task<Widget> UpdateWidget(Guid widgetId, CreateUpdateWidgetRequest request)
        {
            return _siteApi.Put<CreateUpdateWidgetRequest, Widget>($"internal-management/widgets/{widgetId}", request);
        }

        public Task DeleteWidget(Guid widgetId, bool? resetLinked)
        {
            return _siteApi.Delete($"internal-management/widgets/{widgetId}?resetLinked={resetLinked}");
        }
    }
}
