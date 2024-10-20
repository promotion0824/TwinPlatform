using System;
using System.Threading.Tasks;
using Willow.Api.Client;
using WorkflowCore.Models;

namespace WorkflowCore.Services.Apis
{

    public interface IMarketPlaceApiService
    {
        Task<App> GetApp(Guid appId);
    }

    public class MarketPlaceApiService : IMarketPlaceApiService
    {
        private readonly IRestApi _restApi;

        public MarketPlaceApiService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public Task<App> GetApp(Guid appId)
        {
            return _restApi.Get<App>($"apps/{appId}");
        }
    }
}
