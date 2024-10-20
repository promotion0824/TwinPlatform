using System.Threading.Tasks;
using WorkflowCore.Infrastructure.Configuration;
using WorkflowCore.Services.MappedIntegration.Dtos;
using WorkflowCore.Services.MappedIntegration.Dtos.Responses;

namespace WorkflowCore.Services.MappedIntegration.Interfaces;

public interface IMappedApiService
{
    Task SendTicketDataAsync(MappedIntegrationConfiguration mappedConfiguration, MappedTicketEventDto ticketEvent);
    Task<TicketMetadataResponse> GetTicketMetaDataAsync(MappedIntegrationConfiguration mappedConfiguration);
}

