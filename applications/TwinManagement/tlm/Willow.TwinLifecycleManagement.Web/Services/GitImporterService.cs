using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Model.Async;
using Willow.Model.Requests;

namespace Willow.TwinLifecycleManagement.Web.Services
{
    public class GitImporterService : IGitImporterService
    {
        private readonly IModelsService _modelsService;

        public GitImporterService(IModelsService modelsService)
        {
            _modelsService = modelsService;
        }

        public async Task<JobsEntry> ImportAsync(UpgradeModelsRepoRequest gitInfo, string userData, string userId)
        {
            return await _modelsService.PostModelsFromGitAsync(gitInfo, userData, userId);
        }
    }
}
