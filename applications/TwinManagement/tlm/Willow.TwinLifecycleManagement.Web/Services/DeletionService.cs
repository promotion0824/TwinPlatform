using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Exceptions.Exceptions;
using Willow.Model.Requests;
using Willow.TwinLifecycleManagement.Web.Helpers.Converters;

namespace Willow.TwinLifecycleManagement.Web.Services
{
    public class DeletionService : IDeletionService
    {
        private readonly ITwinsService _twinsService;
        private readonly IFileImporterService _importerService;

        public DeletionService(ITwinsService twinsService, IFileImporterService importerService)
        {
            _twinsService = twinsService;
            _importerService = importerService;
        }

        public async Task<JobsEntry> DeleteAllModels(string userId, bool includeDependencies = false, string userData = null)
            => await _importerService.DeleteAllModelsAsync(userId, includeDependencies, userData);

        public async Task<JobsEntry> DeleteAllTwins(string userId, bool isRelationshipsRequest, string userData = null)
        {
            if (!isRelationshipsRequest)
            {
                return await _importerService.DeleteAllTwinsAsync(userId, userData);
            }
            else
            {
                var allTwins = await _twinsService.GetAllTwinsAsync();
                var deleteRelationshipsReqeust = new BulkDeleteRelationshipsRequest()
                {
                    DeleteAll = false,
                    TwinIds = allTwins.Select(x => x.Twin.Id),
                };
                return await _importerService.DeleteRelationshipsAsync(deleteRelationshipsReqeust, userData);
            }
        }

        public async Task<JobsEntry> DeleteSiteIdTwins(string siteId, string userId, string userData, bool isRelationshipsRequest)
        {
            if (!isRelationshipsRequest)
            {
                return await _importerService.DeleteSiteIdTwinsAsync(siteId, userId, userData);
            }
            else
            {
                var twinsAndRelationships = await _twinsService.GetAllTwinsAsync(
                                                                                locationId: siteId,
                                                                                includeRelationships: true,
                                                                                includeIncomingRelationships: true);

                var relationshipsIds = new List<string>();
                foreach (var twinAndRelationship in twinsAndRelationships)
                {
                    if (twinAndRelationship.IncomingRelationships != null)
                    {
                        relationshipsIds.AddRange(twinAndRelationship.IncomingRelationships.Select(x => x.Id));
                    }
                    if (twinAndRelationship.OutgoingRelationships != null)
                    {
                        relationshipsIds.AddRange(twinAndRelationship.OutgoingRelationships.Select(x => x.Id));
                    }
                }

                var bulkDeletionRequest = new BulkDeleteRelationshipsRequest()
                {
                    TwinIds = twinsAndRelationships.Select(x => x.Twin.Id),
                    RelationshipIds = relationshipsIds
                };

                return await _importerService.DeleteRelationshipsAsync(bulkDeletionRequest, userData);
            }
        }

        public async Task<JobsEntry> DeleteTwinsOrRelationshipsByFile(IEnumerable<IFormFile> formFiles, bool isRelationshipsRequest, string userData = null)
        {

            var twinIds = FileConverterHelper.GetConvertedTwinsIds(formFiles);

            if (!twinIds.Any())
                throw new BadRequestException("File does not contain twins");

            if (!isRelationshipsRequest)
            {
                var deleteTwinsReqeust = new BulkDeleteTwinsRequest()
                {
                    DeleteAll = false,
                    TwinIds = twinIds
                };
                return await _importerService.DeleteTwinsByFileAsync(deleteTwinsReqeust, userData);
            }
            else
            {
                var deleteRelationshipsReqeust = new BulkDeleteRelationshipsRequest()
                {
                    DeleteAll = false,
                    TwinIds = twinIds
                };
                return await _importerService.DeleteRelationshipsAsync(deleteRelationshipsReqeust, userData);
            }
        }
    }
}
