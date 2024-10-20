using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Willow.Common;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Entities;
using WorkflowCore.Infrastructure.Configuration;
using WorkflowCore.Models;
using WorkflowCore.Services.Apis;
using WorkflowCore.Services.MappedIntegration.Dtos;
using WorkflowCore.Services.MappedIntegration.Dtos.Requests;
using WorkflowCore.Services.MappedIntegration.Dtos.Responses;
using WorkflowCore.Services.MappedIntegration.Interfaces;
using WorkflowCore.Services.MappedIntegration.Models;

namespace WorkflowCore.Services.MappedIntegration.Services;

public class MappedService : IMappedService
{
    private readonly IWorkflowSequenceNumberService _workflowSequenceNumberService;
    private readonly IDigitalTwinServiceApi _digitalTwinServiceApi;
    private readonly WorkflowContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly IValidator<MappedTicketUpsertRequest> _validator;
    private readonly IMappedIdentityService _mappedIdentityService;
    private readonly ITicketStatusTransitionsService _ticketStatusTransitionsService;
    private readonly IWorkflowService _workflowService;
    private readonly AppSettings _appSettings;

    public MappedService(IWorkflowSequenceNumberService workflowSequenceNumberService,
                         IDigitalTwinServiceApi digitalTwinServiceApi,
                         WorkflowContext context,
                         IDateTimeService dateTimeService,
                         IValidator<MappedTicketUpsertRequest> validator,
                         IMappedIdentityService mappedIdentityService,
                         ITicketStatusTransitionsService ticketStatusTransitionsService,
                         IWorkflowService workflowService,
                         IConfiguration configuration)
    {

        _workflowSequenceNumberService = workflowSequenceNumberService;
        _digitalTwinServiceApi = digitalTwinServiceApi;
        _context = context;
        _dateTimeService = dateTimeService;
        _validator = validator;
        _mappedIdentityService = mappedIdentityService;
        _ticketStatusTransitionsService = ticketStatusTransitionsService;
        _workflowService = workflowService;
        _appSettings = configuration.Get<AppSettings>();
    }


    public async Task<BaseResponse> TicketUpsert(MappedTicketUpsertRequest mappedTicketUpsertDto)
    {
        var validationResult = await _validator.ValidateAsync(mappedTicketUpsertDto);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
            return UpsertTicketResponse.CreateFailure(errors);
        }
        if (string.Equals(mappedTicketUpsertDto.EventType, TicketEventType.Create.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            if (_appSettings.MappedIntegrationConfiguration.IsReadOnly)
            {
                return await CreateReadOnlyTicketAsync(mappedTicketUpsertDto.Data);
            } else
            {
                return await CreateTicketAsync(mappedTicketUpsertDto.Data);
            }
            
        }
        else if (string.Equals(mappedTicketUpsertDto.EventType, TicketEventType.Update.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            if (_appSettings.MappedIntegrationConfiguration.IsReadOnly)
            {
                return await UpdateReadOnlyTicketAsync(mappedTicketUpsertDto.Data);
            } else
            {
                return await UpdateTicketAsync(mappedTicketUpsertDto.Data);
            }
               
        }
        else
        {
            return UpsertTicketResponse.CreateFailure("Invalid event type");
        }
    }

    public async Task<GetTicketsResponse> GetTicketsAsync(Guid siteId, MappedGetTicketsRequest request)
    {
        var ticketsQuery = _context.Tickets
                                   .Include(x => x.Category)
                                   .Include(x => x.JobType)
                                   .Include(x => x.ServiceNeeded)
                                   .Include(x => x.SubStatus)
                                   .Where(x => x.SiteId == siteId);
        if (!string.IsNullOrWhiteSpace(request.ExternalId))
        {
            ticketsQuery = ticketsQuery.Where(x => x.ExternalId == request.ExternalId);
        }
        if (request.SourceId is not null)
        {
            ticketsQuery = ticketsQuery.Where(x => x.SourceId == request.SourceId);
        }
        if (request.CreatedAfter is not null)
        {
            ticketsQuery = ticketsQuery.Where(x => x.CreatedDate >= request.CreatedAfter);
        }

        var tickets = await ticketsQuery.ToListAsync();
        var ticketsDto = MappedTicketDto.MapFromList(tickets);
        await _mappedIdentityService.SetIdentitiesAsync(ticketsDto);
        return GetTicketsResponse.CreateSuccess(ticketsDto);
    }

    public async Task<GetTicketResponse> GetTicketAsync(Guid siteId, Guid ticketId)
    {
        var ticket = await _context.Tickets
                            .Include(x => x.Category)
                            .Include(x => x.JobType)
                            .Include(x => x.ServiceNeeded)
                            .Include(x => x.SubStatus)
                            .Where(x => x.SiteId == siteId && x.Id == ticketId)
                            .FirstOrDefaultAsync();


        if (ticket is null)
        {
            return null;
        }
        var ticketDto = MappedTicketDto.MapFrom(ticket);
        await _mappedIdentityService.SetIdentitiesAsync(ticketDto);
        return GetTicketResponse.CreateSuccess(ticketDto);
    }

    public async Task<TicketCategoricalDataResponse> GetCustomerCategoricalData()
    {
        var ticketCategoricalDataResponse = new  TicketCategoricalDataResponse
        {
            Priorities = Enum.GetNames<Priority>().ToList(),
            TicketStatus = await _context.TicketStatuses
                                         .Select(x => x.Status)
                                         .ToListAsync(),
            TicketSubStatus = _context.TicketSubStatus.Select(x => x.Name).ToList(),
            JobTypes = await _context.JobTypes.Select(x => JobTypeEntity.MapToModel(x)).ToListAsync(),
            AssigneeTypes = Enum.GetNames<AssigneeType>().ToList(),
            RequestTypes = await _context.TicketCategories
                                         .Where(x => x.SiteId == null)
                                         .Select(x => x.Name)
                                         .ToListAsync(),


            ServicesNeeded = await _context.ServiceNeededSpaceTwin
                                            .Include(x => x.ServiceNeeded)
                                            .GroupBy(x => x.SpaceTwinId)
                                            .Select(x => new TicketSpaceTwinServiceNeeded
                                            {
                                                SpaceTwinId = x.Key,
                                                ServiceNeededList = x.Select(y => ServiceNeededEntity.MapToModel(y.ServiceNeeded)).ToList()
                                            })
                                            .ToListAsync()


        };
        // if there is no space twin mapping, return list of service needed
        if (!ticketCategoricalDataResponse.ServicesNeeded.Any())
        {
            var serviceNeededList = await _context.ServiceNeeded.Select(x => ServiceNeededEntity.MapToModel(x)).ToListAsync();
            ticketCategoricalDataResponse.ServicesNeeded =
            [
                new() {
                    SpaceTwinId = string.Empty,
                    ServiceNeededList = serviceNeededList
                }
            ];

        }

        return ticketCategoricalDataResponse;
    }


    #region Private Methods
    private async Task<UpsertTicketResponse> CreateTicketAsync(TicketData request)
    {
        var mappedTicketDto = TicketData.MapToCreateTicketDto(request);
        mappedTicketDto.SequenceNumber = await _workflowSequenceNumberService.GenerateSequenceNumber(request.SequenceNumberPrefix);
        mappedTicketDto.CategoryId = (await _context.TicketCategories
                                              .Where(x => (x.SiteId == request.SiteId || x.SiteId == null)
                                                           && x.Name == mappedTicketDto.RequestType)
                                              .FirstOrDefaultAsync())?.Id;
        mappedTicketDto.JobTypeId = (await _context.JobTypes
                                                   .FirstOrDefaultAsync(x => x.Name == mappedTicketDto.JobType))?.Id;

        mappedTicketDto.ServiceNeededId = (await _context.ServiceNeeded
                                                         .FirstOrDefaultAsync(x => x.Name == mappedTicketDto.ServiceNeeded))?.Id;

        if (!string.IsNullOrWhiteSpace(mappedTicketDto.SubStatus))
        {
            mappedTicketDto.SubStatusId = (await _context.TicketSubStatus
                                                         .FirstOrDefaultAsync(x => x.Name == mappedTicketDto.SubStatus))?.Id;
        }
        if (!string.IsNullOrWhiteSpace(request.TwinId))
        {
            var assetTwin = await _digitalTwinServiceApi.GetTwinById(request.SiteId.Value, request.TwinId);
            mappedTicketDto.IssueId = assetTwin.UniqueId;
            mappedTicketDto.IssueName = assetTwin.Name;
        }

        await _mappedIdentityService.SetIdentitiesAsync(mappedTicketDto);
        var ticketEntity = mappedTicketDto.MapTo(mappedTicketDto, _dateTimeService);
        _context.Tickets.Add(ticketEntity);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return UpsertTicketResponse.CreateDBFailure(ex, request);
        }

        return UpsertTicketResponse.CreateSuccess(new UpsertedTicketData(ticketEntity.Id));
    }

    private async Task<BaseResponse> UpdateTicketAsync(TicketData request)
    {
        var existingTicket = _context.Tickets
                                 .AsTracking()
                                 .Include(x => x.Category)
                                 .Include(x => x.JobType)
                                 .Include(x => x.ServiceNeeded)
                                 .Include(x => x.SubStatus)
                                 .Where(x => x.SiteId == request.SiteId && x.Id == request.TicketId)
                                 .FirstOrDefault();


        if (existingTicket is null)
        {
            return BaseResponse.CreateFailure($"Ticket with  id {request.TicketId} not found");
        }
        var mappedUpdateTicketDto = TicketData.MapToUpdateTicketDto(request);

        // validate status transition
        var requestedTicketStatus =(int)Enum.Parse<TicketStatusEnum>(request.Status, true);
        if (requestedTicketStatus != existingTicket.Status)
        {
            var isValidStatus = await _ticketStatusTransitionsService
                                            .IsValidStatusTransitionAsync(fromStatus: existingTicket.Status,
                                                                          toStatus: requestedTicketStatus);
            if (!isValidStatus)
            {
                return BaseResponse.CreateFailure($"Invalid status transition from {(TicketStatusEnum)existingTicket.Status} to {(TicketStatusEnum) requestedTicketStatus}");
            }

        }

        // check ticket category and job type
        if (existingTicket.JobType?.Name != request.JobType)
        {
            mappedUpdateTicketDto.JobTypeId = (await _context.JobTypes
                                                             .FirstOrDefaultAsync(x => x.Name == request.JobType))?.Id;
        }
        if (existingTicket.Category?.Name != request.RequestType)
        {
            mappedUpdateTicketDto.CategoryId = (await _context.TicketCategories
                                              .Where(x => (x.SiteId == request.SiteId || x.SiteId == null)
                                                           && x.Name == request.RequestType)
                                              .FirstOrDefaultAsync())?.Id;
        }
        if (existingTicket.ServiceNeeded?.Name != request.ServiceNeeded)
        {
            mappedUpdateTicketDto.ServiceNeededId = (await _context.ServiceNeeded
                                                                  .FirstOrDefaultAsync(x => x.Name == request.ServiceNeeded))?.Id;
        }

        if (existingTicket.SubStatus?.Name != request.SubStatus)
        {
            mappedUpdateTicketDto.SubStatusId = (await _context.TicketSubStatus
                                                         .FirstOrDefaultAsync(x => x.Name == request.SubStatus))?.Id;
        }
        await _mappedIdentityService.SetIdentitiesAsync(mappedUpdateTicketDto, existingTicket);
        mappedUpdateTicketDto.MapToTicketEntity(existingTicket);

        // only update ticket if it is modified
        if (_context.Entry(existingTicket).State == EntityState.Modified)
        {
            existingTicket.UpdatedDate = _dateTimeService.UtcNow;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return UpsertTicketResponse.CreateDBFailure(ex, request);
            }
        }       

        return UpsertTicketResponse.CreateSuccess(new UpsertedTicketData(existingTicket.Id));
    }

    private async Task<BaseResponse> CreateReadOnlyTicketAsync(TicketData request)
    {
        var mappedTicketDto = TicketData.MapToCreateTicketDto(request);
        mappedTicketDto.SequenceNumber = await _workflowSequenceNumberService.GenerateSequenceNumber(request.SequenceNumberPrefix);
        // set category
        if (!string.IsNullOrWhiteSpace(mappedTicketDto.RequestType))
        {
            mappedTicketDto.CategoryId = (await _context.TicketCategories
                                             .Where(x => (x.SiteId == request.SiteId || x.SiteId == null)
                                                          && x.Name == mappedTicketDto.RequestType)
                                             .FirstOrDefaultAsync())?.Id;
            if (mappedTicketDto.CategoryId is null)
            {
                var ticketCategory = await _workflowService.
                    CreateTicketCategory(mappedTicketDto.SiteId, new CreateTicketCategoryRequest { Name = mappedTicketDto.RequestType });

                mappedTicketDto.CategoryId = ticketCategory.Id;
            }

        }
        // set job type
        if (!string.IsNullOrWhiteSpace(mappedTicketDto.JobType))
        {
            mappedTicketDto.JobTypeId = (await _context.JobTypes
                                                  .FirstOrDefaultAsync(x => x.Name == mappedTicketDto.JobType))?.Id;

            if (mappedTicketDto.JobTypeId is null)
            {
                var jobType = new JobTypeEntity
                {
                    Id = Guid.NewGuid(),
                    Name = mappedTicketDto.JobType,
                };
                _context.JobTypes.Add(jobType);
                await _context.SaveChangesAsync();
                mappedTicketDto.JobTypeId = jobType.Id;
            }

           
        }

        // set serviceNeeded
        if (!string.IsNullOrWhiteSpace(mappedTicketDto.ServiceNeeded))
        {
            mappedTicketDto.ServiceNeededId = (await _context.ServiceNeeded
                                                         .FirstOrDefaultAsync(x => x.Name == mappedTicketDto.ServiceNeeded))?.Id;
            if (mappedTicketDto.ServiceNeededId is null)
            {
                var serviceNeeded = new ServiceNeededEntity
                {
                    Id = Guid.NewGuid(),
                    Name = mappedTicketDto.ServiceNeeded 
                };
                _context.ServiceNeeded.Add(serviceNeeded);
                await _context.SaveChangesAsync();
                mappedTicketDto.ServiceNeededId = serviceNeeded.Id;
            }

        }
        // set sub status
        if (!string.IsNullOrWhiteSpace(mappedTicketDto.SubStatus))
        {
            mappedTicketDto.SubStatusId = (await _context.TicketSubStatus
                                                         .FirstOrDefaultAsync(x => x.Name == mappedTicketDto.SubStatus))?.Id;

            if (mappedTicketDto.SubStatusId is null)
            {
                var ticketSubStatus = new TicketSubStatusEntity
                {
                    Id = Guid.NewGuid(),
                    Name = mappedTicketDto.SubStatus
                };
                _context.TicketSubStatus.Add(ticketSubStatus);
                await _context.SaveChangesAsync();
                mappedTicketDto.SubStatusId = ticketSubStatus.Id;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.TwinId))
        {
            var assetTwin = await _digitalTwinServiceApi.GetTwinById(request.SiteId.Value, request.TwinId);
            mappedTicketDto.IssueId = assetTwin.UniqueId;
            mappedTicketDto.IssueName = assetTwin.Name;
        }

        // map priority
        mappedTicketDto.Priority = MapPriority(mappedTicketDto.Priority);

        // map assignee Type
        mappedTicketDto.AssigneeType = MapAssigneeType(mappedTicketDto.AssigneeType);

        await _mappedIdentityService.SetIdentitiesAsync(mappedTicketDto);

        var ticketEntity = mappedTicketDto.MapTo(mappedTicketDto, _dateTimeService);
        _context.Tickets.Add(ticketEntity);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return UpsertTicketResponse.CreateDBFailure(ex, request);
        }

        return UpsertTicketResponse.CreateSuccess(new UpsertedTicketData(ticketEntity.Id));
    }

    private async Task<BaseResponse> UpdateReadOnlyTicketAsync(TicketData request)
    {
        var existingTicket = _context.Tickets
                               .AsTracking()
                               .Include(x => x.Category)
                               .Include(x => x.JobType)
                               .Include(x => x.ServiceNeeded)
                               .Include (x => x.SubStatus)
                               .Where(x => x.SiteId == request.SiteId && x.ExternalId == request.ExternalId)
                               .FirstOrDefault();


        if (existingTicket is null)
        {
            return BaseResponse.CreateFailure($"Ticket with external id {request.ExternalId} not found");
        }
        var mappedUpdateTicketDto = TicketData.MapToUpdateTicketDto(request);

         // set category
        if (existingTicket.Category?.Name != mappedUpdateTicketDto.RequestType)
        {
            if (string.IsNullOrEmpty(mappedUpdateTicketDto.RequestType))
            {
                mappedUpdateTicketDto.CategoryId = null;
            } else
            {
                mappedUpdateTicketDto.CategoryId = (await _context.TicketCategories
                                             .Where(x => (x.SiteId == request.SiteId || x.SiteId == null)
                                                          && x.Name == mappedUpdateTicketDto.RequestType)
                                             .FirstOrDefaultAsync())?.Id;
                if (mappedUpdateTicketDto.CategoryId is null)
                {
                    var ticketCategory = await _workflowService.
                    CreateTicketCategory(mappedUpdateTicketDto.SiteId, new CreateTicketCategoryRequest { Name = mappedUpdateTicketDto.RequestType });

                    mappedUpdateTicketDto.CategoryId = ticketCategory.Id;
                }
            }
           
        }

        // set job type
        if (existingTicket.JobType?.Name != mappedUpdateTicketDto.JobType)
        {
            if (string.IsNullOrWhiteSpace(mappedUpdateTicketDto.JobType))
            {
                mappedUpdateTicketDto.JobTypeId = null;
            }
            else
            {
                mappedUpdateTicketDto.JobTypeId = (await _context.JobTypes
                                                             .FirstOrDefaultAsync(x => x.Name == mappedUpdateTicketDto.JobType))?.Id;
                if (mappedUpdateTicketDto.JobTypeId is null)
                {
                    var jobType = new JobTypeEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = mappedUpdateTicketDto.JobType,
                    };
                    _context.JobTypes.Add(jobType);
                    await _context.SaveChangesAsync();
                    mappedUpdateTicketDto.JobTypeId = jobType.Id;
                }
            }
        }

        // set service needed

        if (existingTicket.ServiceNeeded?.Name != mappedUpdateTicketDto.ServiceNeeded)
        {
            if (string.IsNullOrWhiteSpace(mappedUpdateTicketDto.ServiceNeeded))
            {
                mappedUpdateTicketDto.ServiceNeededId = null;
            } else
            {
                mappedUpdateTicketDto.ServiceNeededId = (await _context.ServiceNeeded
                                                                 .FirstOrDefaultAsync(x => x.Name == mappedUpdateTicketDto.ServiceNeeded))?.Id;

                if (mappedUpdateTicketDto.ServiceNeededId is null)
                {
                    var serviceNeeded = new ServiceNeededEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = mappedUpdateTicketDto.ServiceNeeded ?? string.Empty,
                    };
                    _context.ServiceNeeded.Add(serviceNeeded);
                    await _context.SaveChangesAsync();
                    mappedUpdateTicketDto.ServiceNeededId = serviceNeeded?.Id;
                }
            }
           
        }

        // set sub status
        if(existingTicket.SubStatus?.Name != mappedUpdateTicketDto.SubStatus)
        {
            if (string.IsNullOrWhiteSpace(mappedUpdateTicketDto.SubStatus))
            {
                mappedUpdateTicketDto.SubStatusId = null;
            }
            else
            {
                mappedUpdateTicketDto.SubStatusId = (await _context.TicketSubStatus
                                                                 .FirstOrDefaultAsync(x => x.Name == mappedUpdateTicketDto.SubStatus))?.Id;

                if (mappedUpdateTicketDto.SubStatusId is null)
                {
                    var ticketSubStatus = new TicketSubStatusEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = mappedUpdateTicketDto.SubStatus,
                    };
                    _context.TicketSubStatus.Add(ticketSubStatus);
                    await _context.SaveChangesAsync();
                    mappedUpdateTicketDto.SubStatusId = ticketSubStatus?.Id;
                }
            }
        }
        // map priority
        mappedUpdateTicketDto.Priority = MapPriority(mappedUpdateTicketDto.Priority);
        // map assignee Type
        mappedUpdateTicketDto.AssigneeType = MapAssigneeType(mappedUpdateTicketDto.AssigneeType);

       await _mappedIdentityService.SetIdentitiesAsync(mappedUpdateTicketDto, existingTicket);
        mappedUpdateTicketDto.MapToTicketEntity(existingTicket);

        // only update the ticket in database if it is modified
        if (_context.Entry(existingTicket).State == EntityState.Modified)
        {
            existingTicket.UpdatedDate = _dateTimeService.UtcNow;
            await _context.SaveChangesAsync();
        }

        return UpsertTicketResponse.CreateSuccess(new UpsertedTicketData(existingTicket.Id));
    }

    private string MapPriority(string ticketPriority)
    {
        ticketPriority = ticketPriority.ToLower().Trim();
        string priority = ticketPriority switch
        {
            "pe-emergency-onsite w/i 2 hours" => Priority.Urgent.ToString(),
            "pe-emergency-onsite w/i 4 hours" => Priority.Urgent.ToString(),
            "p1-onsite w/i 24 hours" => Priority.High.ToString(),
            "p2-onsite w/i 48 hours" => Priority.Medium.ToString(),
            "p3-onsite w/i 3 days" => Priority.Medium.ToString(),
            "p5-onsite w/i 5 days" => Priority.Medium.ToString(),
            "p7-onsite w/i 7 days" => Priority.Medium.ToString(),
            "scheduled service" => Priority.Low.ToString(),
            "tech initiated" => Priority.Low.ToString(),
            "p21-onsite w/i 21 days" => Priority.Low.ToString(),
            "low" => Priority.Low.ToString(),
            "medium" => Priority.Medium.ToString(),
            "high" => Priority.High.ToString(),
            "urgent" => Priority.Urgent.ToString(),


            _ => throw new NotImplementedException($"unrecognized Ticket Priority value : {ticketPriority}")
        };

        return priority;
    }

    private string MapAssigneeType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return AssigneeType.NoAssignee.ToString();
        }
        else
        {
            return value;
        }
    }
    #endregion
}

