using System;
using System.Threading.Tasks;
using WorkflowCore.Services.MappedIntegration.Dtos.Requests;
using WorkflowCore.Services.MappedIntegration.Dtos.Responses;

namespace WorkflowCore.Services.MappedIntegration.Interfaces;

public interface IMappedService
{
    Task<BaseResponse> TicketUpsert(MappedTicketUpsertRequest request);
    Task<GetTicketsResponse> GetTicketsAsync(Guid siteId, MappedGetTicketsRequest request);

    Task<GetTicketResponse> GetTicketAsync(Guid siteId, Guid ticketId);
    Task<TicketCategoricalDataResponse> GetCustomerCategoricalData();
}

