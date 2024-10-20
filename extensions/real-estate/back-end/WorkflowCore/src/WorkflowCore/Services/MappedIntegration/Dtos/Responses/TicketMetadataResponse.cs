using System.Collections.Generic;
using System;

namespace WorkflowCore.Services.MappedIntegration.Dtos.Responses;

public class TicketMetadataResponse
{
    public List<JobType> JobTypes { get; set; }
    public List<RequestType> RequestTypes { get; set; }
    public List<ServiceNeeded> ServiceNeededList { get; set; }
    public List<SpaceServiceNeeded> SpaceServiceNeededList { get; set; }
}

/// <summary>
///  from api/nuvoloJobTypes section
/// </summary>
/// <param name="Id">id mapped form sys_id</param>
/// <param name="Name"> name mapped from jobType </param>
public record JobType(Guid Id, string Name);

/// <summary>
///  from api/nuvoloRequestTypes section
/// </summary>
/// <param name="Id"> id mapped from sys_id</param>
/// <param name="Name">name mapped from requestType</param>
public record RequestType(Guid Id, string Name);


/// <summary>
///  from api/nuvoloServicesNeeded section
/// </summary>
/// <param name="Id">mapped from u_service_needed_sys_id</param>
/// <param name="RequestTypeId">mapped from u_request_type</param>
/// <param name="Name">mapped from u_service_needed</param>
public record ServiceNeeded(Guid Id, Guid RequestTypeId, string Name);

/// <summary>
///  from api/nuvoloSpacesServicesNeeded section
/// </summary>
/// <param name="BuildingId"> mapped from building_sys_id</param>
/// <param name="SpaceId">mapped from space_sys_id</param>
/// <param name="ServiceNeededIds">mapped from u_service_needed_sys_ids</param>
public record SpaceServiceNeeded(Guid BuildingId, Guid SpaceId, List<Guid> ServiceNeededIds);
