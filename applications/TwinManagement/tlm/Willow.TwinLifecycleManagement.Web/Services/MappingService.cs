using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Model.Async;
using Willow.TwinLifecycleManagement.Web.Models.Mapped;

namespace Willow.TwinLifecycleManagement.Web.Services
{
    public interface IMappingService
    {
        public Task<MappedEntryResponse> GetMappedEntriesAsync(MappedEntryRequest request);
        public Task<CombinedMappedEntriesGroupCount> GetCombinedMappedEntriesGroupCountAsync();
        public Task<MappedEntry> CreateMappedEntry(CreateMappedEntry entry);

        public Task<MappedEntry> UpdateMappedEntry(UpdateMappedEntry entry);

        public Task<int> GetMappedEntriesCountAsync(IEnumerable<Status> statuses, ICollection<string> prefixToMatchId = null, bool? excludePrefixes = false);

        public Task<int> UpdateMappedEntryStatus(UpdateMappedEntryStatusRequest request);

        public Task<int> UpdateAllMappedEntryStatus(MappedEntryAllRequest request, Status status);

        public Task<int> DeleteBulk(IEnumerable<string> mappedIds);

        public Task<int> DeleteAll(MappedEntryAllRequest request);

        public Task<UpdateMappedTwinRequest> CreateUpdateTwinRequest(string willowTwinId, List<JsonPatchOperation> jsonPatch);

        public Task<UpdateMappedTwinRequest> UpdateTwinUpdateRequest(Guid id, List<JsonPatchOperation> jsonPatch);

        public Task<UpdateMappedTwinRequest> UpsertUpdateTwinRequest(string willowTwinId, List<JsonPatchOperation> jsonPatch);

        public Task<ICollection<UpdateMappedTwinRequestResponse>> GetUpdateTwinRequestsAsync(int offset, int pageSize);

        public Task<int> GetUpdateTwinRequestsCountAsync();

        public Task<int> DeleteBulkUpdateTwinRequests(IEnumerable<Guid> ids);

        public Task<int> DeleteAllUpdateTwinRequests();

        public Task<MtiAsyncJob> CreateMtiAsyncJob(MtiAsyncJobRequest request);

        public Task<IEnumerable<MtiAsyncJob>> FindMtiAsyncJobs(string jobId = null, AsyncJobStatus? status = null);

        public Task<MtiAsyncJob> UpdateMtiAsyncJobStatus(MtiAsyncJob job, AsyncJobStatus status);

        public Task<MtiAsyncJob> GetLatestMtiAsyncJob(AsyncJobStatus? status);
    }

    public class MappingService (IMappingClient mappingClient, ITwinsClient twinsClient) : IMappingService
    {
        /// <summary>
        /// Get mapped entries.
        /// </summary>
        /// <returns>Get mapped entries</returns>
        public async Task<MappedEntryResponse> GetMappedEntriesAsync(MappedEntryRequest request)
        {
            return await mappingClient.GetMappedEntriesAsync(request);
        }

        /// <summary>
        /// Get dropdown options for the building and connector filters.
        /// </summary>
        public async Task<CombinedMappedEntriesGroupCount> GetCombinedMappedEntriesGroupCountAsync()
        {
            var buildingTask = mappingClient.GetGroupedCountAsync("BuildingId", Status.Pending);
            var connectorTask = mappingClient.GetGroupedCountAsync("ConnectorId", Status.Pending);

            await Task.WhenAll(buildingTask, connectorTask);

            return new CombinedMappedEntriesGroupCount
            {
                BuildingIdGroupedEntries = await buildingTask,
                ConnectorIdGroupedEntries = await connectorTask
            };
        }

        /// <summary>
        /// Create mapped entry.
        /// </summary>
        /// <param name="entry">Mapped entry object</param>
        /// <returns>Created Mapped entry object</returns>
        public async Task<MappedEntry> CreateMappedEntry(CreateMappedEntry entry)
        {
            return await mappingClient.CreateMappedEntryAsync(entry);
        }

        /// <summary>
		/// Update a mapped entry.
		/// </summary>
        /// <param name="entry">Mapped entry object</param>
        /// <returns>Updated Mapped entry object</returns>
        public async Task<MappedEntry> UpdateMappedEntry(UpdateMappedEntry entry)
        {
            // Perform duplicate twin check. If selected willow twin id has externalID already assigned, then set to ignore and set isExistingTwin true.
            var newWillowTwinId = entry.WillowId;
            try
            {
                var twin = await twinsClient.GetTwinByIdAsync(newWillowTwinId);
                if (twin.Twin.Contents.ContainsKey("externalID") && twin.Twin.Contents["externalID"] != null)
                {
                    entry.IsExistingTwin = true;
                    entry.Status = Status.Ignore;
                }
                else
                {
                    entry.IsExistingTwin = false;
                }
            }
            catch
            {
                // GetTwinByIdAsync returned 404, so the twin does not exist.
                entry.IsExistingTwin = false;
            }

            // Update the mapped entry.
            return await mappingClient.UpdateMappedEntryAsync(entry);
        }

        /// <summary>
        /// Get mapped entries count.
        /// </summary>
        /// <param name="statuses">query based on mapped entries' status</param>
        /// <param name="prefixToMatchId">Prefixes to match with the first few characters of Mapped Id</param>
        /// <param name="excludePrefixes">Exclude records where prefixes match with the first few characters of Mapped Id</param>
        /// <returns>Return counts based on query</returns>
        public async Task<int> GetMappedEntriesCountAsync(IEnumerable<Status> statuses, ICollection<string> prefixToMatchId = null, bool? excludePrefixes = false)
        {
            return await mappingClient.GetMappedEntriesCountAsync(statuses, prefixToMatchId, excludePrefixes);
        }

        public async Task<int> UpdateMappedEntryStatus(UpdateMappedEntryStatusRequest request)
        {
            return await mappingClient.UpdateMappedEntryStatusAsync(request);
        }

        public async Task<int> UpdateAllMappedEntryStatus(MappedEntryAllRequest request, Status status)
        {
            return await mappingClient.UpdateAllMappedEntryStatusAsync(request, status);
        }

        public async Task<int> DeleteBulk(IEnumerable<string> mappedIds)
        {
            return await mappingClient.DeleteMappedEntriesAsync(mappedIds);
        }

        public async Task<int> DeleteAll(MappedEntryAllRequest request)
        {
            return await mappingClient.DeleteAllMappedEntriesAsync(request);
        }

        /// <summary>
        /// Create twin update request.
        /// </summary>
        /// <param name="willowTwinId">Willow twin id</param>
        /// <param name="jsonPatch">json patch</param>
        /// <returns> created twin update request</returns>
        public async Task<UpdateMappedTwinRequest> CreateUpdateTwinRequest(string willowTwinId, List<JsonPatchOperation> jsonPatch)
        {
            return await mappingClient.CreateUpdateTwinRequestAsync(jsonPatch, willowTwinId);
        }

        /// <summary>
        /// Update twin update request.
        /// </summary>
        /// <param name="id">id of twin update request</param>
        /// <param name="jsonPatch">json patch</param>
        /// <returns> updated twin update request</returns>
        public async Task<UpdateMappedTwinRequest> UpdateTwinUpdateRequest(Guid id, List<JsonPatchOperation> jsonPatch)
        {
            return await mappingClient.UpdateTwinUpdateRequestAsync(jsonPatch, id);
        }


        /// <summary>
        /// Upsert update twin request. If record with willowTwinId exists, update it. Otherwise, create a new record.
        /// </summary>
        /// <param name="willowTwinId">Willow twin id</param>
        /// <param name="jsonPatch">json patch</param>
        /// <returns> twin update request</returns>
        public async Task<UpdateMappedTwinRequest> UpsertUpdateTwinRequest(string willowTwinId, List<JsonPatchOperation> jsonPatch)
        {
            return await mappingClient.UpsertUpdateTwinRequestAsync(jsonPatch, willowTwinId);
        }

        /// <summary>
        /// Get update twin requests.
        /// </summary>
        /// <param name="offset">fetch record from offset.</param>
        /// <param name="pageSize">amount of record to fetch.</param>
        /// <returns>list of update twin requests.</returns>
        public async Task<ICollection<UpdateMappedTwinRequestResponse>> GetUpdateTwinRequestsAsync(int offset, int pageSize)
        {
            return await mappingClient.GetUpdateTwinRequestsAsync(offset, pageSize);
        }

        /// <summary>
        /// Get update twin requests count.
        /// </summary>
        /// <returns>Count of update twin requests.</returns>
        public async Task<int> GetUpdateTwinRequestsCountAsync()
        {
            return await mappingClient.GetUpdateTwinRequestsCountAsync();
        }

        /// <summary>
        ///  Delete update twin request based on id.
        /// </summary>
        /// <param name="ids">List of updated twin request id</param>
        public async Task<int> DeleteBulkUpdateTwinRequests(IEnumerable<Guid> ids)
        {
            return await mappingClient.DeleteUpdateTwinRequestsAsync(ids);
        }

        /// <summary>
        ///  Delete all update twins requests.
        /// </summary>
        public async Task<int> DeleteAllUpdateTwinRequests()
        {
            return await mappingClient.DeleteAllUpdateTwinRequestsAsync();
        }

        public async Task<MtiAsyncJob> CreateMtiAsyncJob(MtiAsyncJobRequest request)
        {
            return await mappingClient.CreateMtiAsyncJobAsync(request);
        }

        public async Task<IEnumerable<MtiAsyncJob>> FindMtiAsyncJobs(string jobId = null, AsyncJobStatus? status = null)
        {
            return await mappingClient.FindMtiAsyncJobsAsync(jobId, status);
        }

        public async Task<MtiAsyncJob> UpdateMtiAsyncJobStatus(MtiAsyncJob job, AsyncJobStatus status)
        {
            return await mappingClient.UpdateMtiAsyncJobStatusAsync(job, status);
        }

        public async Task<MtiAsyncJob> GetLatestMtiAsyncJob(AsyncJobStatus? status)
        {
            return await mappingClient.GetLatestMtiAsyncJobAsync(status);
        }
    }
}
