using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using PlatformPortalXL.Models;

using Willow.Api.Client;

namespace PlatformPortalXL.Services.MarketPlaceApi
{
    public interface IMarketPlaceApiService
    {
        Task<List<AppCategory>> GetCategories();
        Task<List<App>> GetApps();
        Task<List<App>> GetPrivateApps(Guid siteId);
        Task<App> GetApp(Guid appId, Guid? siteId = null);
        Task<List<Installation>> GetInstalledApps(Guid siteId);
        Task InstallApp(Guid siteId, Guid appId, Guid installedByUserId);
        Task UninstallApp(Guid siteId, Guid appId);

        Task<string> GetSignature(Guid appId, string payload);
    }

    public class MarketPlaceApiService : IMarketPlaceApiService
    {
        private readonly IRestApi _restApi;

        public MarketPlaceApiService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public Task<List<AppCategory>> GetCategories()
        {
            return _restApi.Get<List<AppCategory>>("categories");
        }

        public Task<List<App>> GetApps()
        {
            return _restApi.Get<List<App>>("apps");
        }

        public async Task<List<App>> GetPrivateApps(Guid siteId)
        {
            try
            {
                return await _restApi.Get<List<App>>($"/sites/{siteId}/privateApps");
            }
            catch (RestException)
            {
                return new List<App>();
            }
        }

        public Task<App> GetApp(Guid appId, Guid? siteId = null)
        {
            var url = $"apps/{appId}";

            if (siteId.HasValue)
                url = QueryHelpers.AddQueryString(url, "siteId", siteId.Value.ToString());

            return _restApi.Get<App>(url);
        }

        public Task<List<Installation>> GetInstalledApps(Guid siteId)
        {
            return _restApi.Get<List<Installation>>($"sites/{siteId}/installedApps");
        }

        public class InstallAppPayload
        {
            public Guid AppId { get; set; }
            public Guid InstalledByUserId { get; set; }
        }

        public async Task InstallApp(Guid siteId, Guid appId, Guid installedByUserId)
        {
            await _restApi.PostCommand<InstallAppPayload>($"sites/{siteId}/installedApps", new InstallAppPayload { AppId = appId, InstalledByUserId = installedByUserId });
        }

        public Task UninstallApp(Guid siteId, Guid appId)
        {
            return _restApi.Delete($"sites/{siteId}/installedApps/{appId}");
        }

        public async Task<string> GetSignature(Guid appId, string payload)
        {
            var url    = QueryHelpers.AddQueryString($"apps/{appId}/signatures", "payload", payload);
            var result = await _restApi.Get<GetSignatureResponse>(url);

            return result.Signature;
        }

        public class GetSignatureResponse
        {
            public string Signature { get; set; }
        }
    }
}
