using System;
using System.Threading;
using System.Threading.Tasks;
using DigitalTwinCore.Features.DirectoryCore.Dtos;
using Willow.Api.Client;

namespace DigitalTwinCore.Features.DirectoryCore
{
    public interface IDirectoryCoreClient
    {
        Task<bool> SiteExists(Guid siteId, CancellationToken cancellationToken);
        Task<UserDto> GetUser(Guid userId);
    }

    public class DirectoryCoreClient : IDirectoryCoreClient
    {
        private readonly IRestApi _directoryApi;

        public DirectoryCoreClient(IRestApi directoryApi)
        {
            _directoryApi = directoryApi;
        }

        public async Task<bool> SiteExists(Guid siteId, CancellationToken cancellationToken)
        {
            var site = await GetSite(siteId);

            return site != null;
        }

        private async Task<Site> GetSite(Guid siteId)
        {
            return await _directoryApi.Get<Site>($"/sites/{siteId}");
        }

        public async Task<UserDto> GetUser(Guid userId)
        {
            return await _directoryApi.Get<UserDto>($"/users/{userId}");
        }
    }
}
