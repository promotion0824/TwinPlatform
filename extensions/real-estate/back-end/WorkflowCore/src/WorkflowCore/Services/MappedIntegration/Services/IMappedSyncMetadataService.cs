using System.Threading.Tasks;
using WorkflowCore.Services.MappedIntegration.Dtos.Responses;

namespace WorkflowCore.Services.MappedIntegration.Services;

public interface IMappedSyncMetadataService
{
    Task SyncTicketMetadata();
}

