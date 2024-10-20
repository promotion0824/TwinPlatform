using Willow.AzureDigitalTwins.SDK.Client;

namespace Willow.TwinLifecycleManagement.Web.Services
{
    public interface IDeletionService
    {
        public Task<JobsEntry> DeleteAllModels(string userId, bool includeDependencies = false, string userData = null);

        public Task<JobsEntry> DeleteSiteIdTwins(string siteId, string userId, string userData, bool isRelationshipsRequest);

        public Task<JobsEntry> DeleteTwinsOrRelationshipsByFile(IEnumerable<IFormFile> formFiles, bool isRelationshipsRequest, string userData = null);

        public Task<JobsEntry> DeleteAllTwins(string userId, bool deleteOnlyRelationships, string userData);
    }
}
