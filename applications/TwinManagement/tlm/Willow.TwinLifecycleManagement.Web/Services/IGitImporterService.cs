using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Model.Requests;

namespace Willow.TwinLifecycleManagement.Web.Services
{
    public interface IGitImporterService
    {
        Task<JobsEntry> ImportAsync(UpgradeModelsRepoRequest gitInfo, string userData, string userId);
    }
}
