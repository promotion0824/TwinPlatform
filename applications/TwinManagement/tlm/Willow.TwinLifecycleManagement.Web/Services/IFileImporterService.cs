using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Model.Async;
using Willow.Model.Requests;
using Willow.Model.Responses;
using Willow.TwinLifecycleManagement.Web.Models;
using BlobUploadInfo = Willow.AzureDigitalTwins.SDK.Client.BlobUploadInfo;

namespace Willow.TwinLifecycleManagement.Web.Services
{
    public interface IFileImporterService
    {
        /// <summary>Initializing the import process.</summary>
        /// <returns>A string representing jobId of initialized import process - can be used for polling status.</returns>
        Task<JobsEntry> ImportAsync(IEnumerable<IFormFile> formFiles, string siteId, bool includeRelationships, string userData, bool includeTwinProperties);

        Task<string> ImportTimeSeriesFromBlobAsync(string sasUrl, string userData);

        Task<IEnumerable<Document>> GetDocumentsAsync();

        Task<IEnumerable<CreateDocumentResponse>> CreateFileTwinsAsync(Models.CreateDocumentRequest createDocumentRequest);

        Task<UpdateDocumentResponse> UpdateDocumentType(string twinId, string fileName, string documentType);

        Task<JobsEntry> DeleteAllModelsAsync(string userId, bool includeDependencies = false, string userData = null);

        Task<JobsEntry> DeleteSiteIdTwinsAsync(string siteId, string userId, string userData);

        Task<JobsEntry> DeleteAllTwinsAsync(string userId, string userData = null);

        Task<JobsEntry> DeleteTwinsByFileAsync(BulkDeleteTwinsRequest request, string userData);

        Task<JobsEntry> DeleteRelationshipsAsync(BulkDeleteRelationshipsRequest relationships, string userData);

        Task<BlobUploadInfo> GetBlobUploadInfoAsync(string[] fileNames);

        Task<BlobUploadInfo> GetTimeSeriesBlobUploadInfoAsync(string[] fileNames);

        Task<CreateDocumentResponse> ClientCreateFileTwinAsync(string fileName, string blobPath, string siteId);

        Task<string> ClientCreateFileTimeSeriesAsync(ImportTimeSeriesHistoricalRequest request);
    }
}
