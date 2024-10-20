using Azure.DigitalTwins.Core;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Requests;
using Willow.TwinLifecycleManagement.Web.Models;
using MultipleEntityResponse = Willow.AzureDigitalTwins.SDK.Client.MultipleEntityResponse;

namespace Willow.TwinLifecycleManagement.Web.Services
{
    public interface ITwinsService
    {
        Task<Page<TwinWithRelationships>> GetTwinsWithRelationshipsAsync(
            string locationId = null,
            string[] modelIds = null,
            bool exactModelMatch = false,
            int pageSize = 100,
            string searchString = "",
            string continuationToken = null,
            bool includeRelationships = false,
            bool includeIncomingRelationships = false,
            SourceType sourceType = SourceType.Adx,
                  DateTimeOffset? startTime = null,
                  DateTimeOffset? endTime = null,
                  bool includeTotalCount = false);

        Task<Page<TwinWithRelationships>> QueryTwinsWithRelationshipsAsync(
            GetTwinsInfoRequestBFF request,
            int pageSize = 100,
            string continuationToken = null,
            bool includeTotalCount = false);

        Task<IEnumerable<NestedTwin>> GetTwinsTreeAsync(
            string[] modelIds = null,
            string[] outgoingRelationships = null,
            string[] incomingRelationships = null,
            bool exactModelMatch = false);

        Task<MultipleEntityResponse> DeleteTwins(string[] twinIds = null, bool deleteRelationships = false);

        Task<int> GetTwinsCount(
        string locationId = null,
        string[] modelIds = null,
        bool exactModelMatch = false,
        SourceType sourceType = SourceType.Adx,
        string searchString = null,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null);

        Task<IEnumerable<TwinWithRelationships>> GetAllTwinsAsync(
            string locationId = null,
            string[] modelIds = null,
            bool exactModelMatch = false,
            bool includeRelationships = false,
            bool includeIncomingRelationships = false,
            SourceType sourceType = SourceType.Adx);

        Task<TwinWithRelationships> GetTwinAsync(
            string twinId,
            SourceType sourceType = SourceType.AdtQuery,
            bool includeRelationships = false);

        Task<IEnumerable<TwinWithRelationships>> GetTwinByIdsAsync(string[] twinIds, SourceType sourceType, bool includeRelationships);

        Task<JobsEntry> PostTwinsAndRelationshipsAsync(BulkImportTwinsRequest twinsRequest, string userData);


        Task PatchTwin(string twinId, IEnumerable<Operation> jsonPatch, bool includeAdxUpdate);

        Task<BasicDigitalTwin> PutTwin(BasicDigitalTwin twin);
    }
}
