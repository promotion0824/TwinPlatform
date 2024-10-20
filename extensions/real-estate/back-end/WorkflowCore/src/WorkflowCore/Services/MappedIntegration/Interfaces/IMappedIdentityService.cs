using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Entities;
using WorkflowCore.Services.MappedIntegration.Dtos;

namespace WorkflowCore.Services.MappedIntegration.Interfaces;

public interface IMappedIdentityService
{
    Task SetIdentitiesAsync(MappedCreateTicketDto mappedTicketUpsertDto);
    Task SetIdentitiesAsync(MappedUpdateTicketDto mappedUpdateTicketDto, TicketEntity ticketEntity);
    Task SetIdentitiesAsync(MappedTicketEventDto mappedTicketEventDto);
    Task SetIdentitiesAsync(MappedTicketDto mappedTicketDto);
    Task SetIdentitiesAsync(List<MappedTicketDto> mappedTicketDtos);
}

