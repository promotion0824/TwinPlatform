using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using WorkflowCore.Repository;
using WorkflowCore.Services.Apis;
using WorkflowCore.Services.Apis.Requests;
using WorkflowCore.Services.MappedIntegration.Dtos;
using WorkflowCore.Services.MappedIntegration.Interfaces;
using WorkflowCore.Services.MappedIntegration.Models;
namespace WorkflowCore.Services.MappedIntegration.Services;

public class MappedIdentityService : IMappedIdentityService
{
    private readonly WorkflowContext _context;
    private readonly IDirectoryApiService _directoryApiService;
    private readonly ITicketStatusService _ticketStatusService;
    private readonly ISessionService _sessionService;
    private readonly IAuditTrailRepository _auditTrailRepository;
    /// <summary>
    /// for each method in this service , we need to check if the identity is a willow user or an external profile
    /// first we collect all the emails/ids of the identities in the ticket that we need to sync
    /// then we get the user profiles from the directory api or the external profiles
    /// and set either the emails or the ids based on the user profile
    /// </summary>
    public MappedIdentityService(WorkflowContext context,
                                 IDirectoryApiService directoryApiService,
                                 ITicketStatusService ticketStatusService,
                                 ISessionService sessionService,
                                 IAuditTrailRepository auditTrailRepository)
    {
        _context = context;
        _directoryApiService = directoryApiService;
        _ticketStatusService = ticketStatusService;
        _sessionService = sessionService;
        _auditTrailRepository = auditTrailRepository;
    }
    /// <summary>
    /// set identities for create a ticket
    /// </summary>
    /// <param name="mappedCreateTicketDto"></param>
    /// <returns></returns>
    public async Task SetIdentitiesAsync(MappedCreateTicketDto mappedCreateTicketDto)
    {
        var userEmails = new List<string>();
        var isUserAssignee = mappedCreateTicketDto.IsUserAssignee();
        var isWorkgroupAssignee = mappedCreateTicketDto.IsWorkgroupAssignee();
        var isTicketClosed = false;

        userEmails.Add(mappedCreateTicketDto.Creator.Email);
        // the closed by identity is optional on ticket closed
        // we need to check first if the value is valid
        if (!string.IsNullOrWhiteSpace(mappedCreateTicketDto.ClosedBy?.Email))
        {
            isTicketClosed = await _ticketStatusService.IsTicketClosed(mappedCreateTicketDto.Status);
            if (isTicketClosed)
            {
                userEmails.Add(mappedCreateTicketDto.ClosedBy?.Email);
            }
        }

        if (isUserAssignee)
        {
            userEmails.Add(mappedCreateTicketDto.Assignee.Email);
        }
        var userProfilesResponse = await _directoryApiService.GetUsersProfilesAsync(new GetUsersProfilesRequest(null, userEmails));
        var userProfiles = MappedUserProfile.MapFromUserProfiles(userProfilesResponse);
        if (isUserAssignee)
        {
            SetAssigneeId(userProfiles, mappedCreateTicketDto.Assignee);
        }
        else if (isWorkgroupAssignee)
        {
            SetWorkgroupAssignee(mappedCreateTicketDto.SiteId, mappedCreateTicketDto.AssigneeWorkgroup);
        }
        // set identity id of the ticket creator
        SetIdentityId(userProfiles, mappedCreateTicketDto.Creator);
        SetReporter(mappedCreateTicketDto.CustomerId, mappedCreateTicketDto.SiteId, mappedCreateTicketDto.Reporter);

        // set identity id of the ticket closed by
        if (isTicketClosed)
        {
            SetIdentityId(userProfiles, mappedCreateTicketDto.ClosedBy);
            // set session data to track audit trail for closed status
            _sessionService.SetSessionData(SourceType.Mapped, mappedCreateTicketDto.ClosedBy?.Id);
        }
        else
        {
            _sessionService.SetSessionData(SourceType.Mapped);
        }

    }

    /// <summary>
    /// set identities for update ticket
    /// </summary>
    /// <param name="mappedUpdateTicketDto"></param>
    /// <param name="ticketEntity"></param>
    /// <returns></returns>
    public async Task SetIdentitiesAsync(MappedUpdateTicketDto mappedUpdateTicketDto, TicketEntity ticketEntity)
    {
        var userEmails = new List<string>();
        var isTicketClosed = false;
        var isUserAssignee = mappedUpdateTicketDto.IsUserAssignee() && mappedUpdateTicketDto.Assignee?.Name != ticketEntity.AssigneeName;
        var isWorkgroupAssignee = mappedUpdateTicketDto.IsWorkgroupAssignee() && mappedUpdateTicketDto.AssigneeWorkgroup?.Name != ticketEntity.AssigneeName;
        var userProfiles = new List<MappedUserProfile>();
        // the closed by identity is optional on ticket closed
        // we need to check first if the value is valid
        if (!string.IsNullOrWhiteSpace(mappedUpdateTicketDto.ClosedBy?.Email))
        {
            isTicketClosed = await _ticketStatusService.IsTicketClosed(mappedUpdateTicketDto.Status);
            if (isTicketClosed)
            {
                userEmails.Add(mappedUpdateTicketDto.ClosedBy?.Email);
            }
        }
        if (isUserAssignee)
        {
            userEmails.Add(mappedUpdateTicketDto.Assignee.Email);
        }
        // get user profiles
        if (userEmails.Any())
        {
            var userProfilesResponse = await _directoryApiService.GetUsersProfilesAsync(new GetUsersProfilesRequest(null, userEmails));
            userProfiles = MappedUserProfile.MapFromUserProfiles(userProfilesResponse);
        }

        // set user profiles
        if (isUserAssignee)
        {
            SetAssigneeId(userProfiles, mappedUpdateTicketDto.Assignee);
        }
        if (isWorkgroupAssignee)
        {
            SetWorkgroupAssignee(ticketEntity.SiteId, mappedUpdateTicketDto.AssigneeWorkgroup);
        }

        if (!string.IsNullOrWhiteSpace(ticketEntity.ReporterEmail)
            &&!ticketEntity.ReporterEmail.Equals(mappedUpdateTicketDto.Reporter.ReporterEmail, StringComparison.CurrentCultureIgnoreCase))
        {
            SetReporter(ticketEntity.CustomerId, ticketEntity.SiteId, mappedUpdateTicketDto.Reporter);
        }

        if (isTicketClosed)
        {
            SetIdentityId(userProfiles, mappedUpdateTicketDto.ClosedBy);
            // set session data to track audit trail for closed status
            _sessionService.SetSessionData(SourceType.Mapped, mappedUpdateTicketDto.ClosedBy?.Id);
        }
        else
        {
            _sessionService.SetSessionData(SourceType.Mapped);
        }

    }

    /// <summary>
    /// set identities for the ticket event
    /// </summary>
    /// <param name="mappedTicketEventDto"></param>
    /// <returns></returns>
    public async Task SetIdentitiesAsync(MappedTicketEventDto mappedTicketEventDto)
    {
        // this value is optional when ticket closed,
        // we need to check if it has a value before sync the identities
        Guid? closedById = null;

        var userIds = new List<Guid>
        {
            mappedTicketEventDto.Data.Creator.Id
        };

        // check closed by identity
        var ticketClosedStatus = await _ticketStatusService.GetClosedStatus();
        if (ticketClosedStatus.Contains((int)Enum.Parse<TicketStatusEnum>(mappedTicketEventDto.Data.Status,true)))
        {
            var closedStatusAuditTrail = await GetClosedStatusAuditTrail(mappedTicketEventDto.Data.Id.Value, ticketClosedStatus);

            if (closedStatusAuditTrail is not null && closedStatusAuditTrail.SourceId.HasValue)
            {
                closedById = closedStatusAuditTrail.SourceId.Value;
                userIds.Add(closedById.Value);
            }
        }
        var isUserAssignee = mappedTicketEventDto.Data.IsUserAssignee();
        var isWorkgroupAssignee = mappedTicketEventDto.Data.IsWorkgroupAssignee();

        if (isUserAssignee)
        {
            userIds.Add(mappedTicketEventDto.Data.Assignee.Id);
        }
        else if (isWorkgroupAssignee)
        {
            var workgroupUserIds = _context.WorkgroupMembers
                            .Where(x => x.WorkgroupId == mappedTicketEventDto.Data.AssigneeWorkgroup.Id)
                            .Select(x => x.MemberId)
                            .ToList();

            userIds.AddRange(workgroupUserIds);
        }
        // get user profiles
        var userProfilesResponse = await _directoryApiService.GetUsersProfilesAsync(new GetUsersProfilesRequest(userIds, null));
        var userProfiles = MappedUserProfile.MapFromUserProfiles(userProfilesResponse);

        // set creator profile
        var ticketCreator = userProfiles.Where(x => x.Id == mappedTicketEventDto.Data.Creator.Id).FirstOrDefault();
        mappedTicketEventDto.Data.Creator = ticketCreator;

        // set assignee profile
        if (isUserAssignee)
        {
            var ticketAssignee = userProfiles.Where(x => x.Id == mappedTicketEventDto.Data.Assignee.Id).FirstOrDefault();
            mappedTicketEventDto.Data.Assignee = new MappedAssignee
            {
                Id = ticketAssignee.Id,
                Name = ticketAssignee.Name,
                Email = ticketAssignee.Email
            };
        }
        else if (isWorkgroupAssignee)
        {
            foreach (var workgroupMember in mappedTicketEventDto.Data.AssigneeWorkgroup.Assignees)
            {
                var ticketAssignee = userProfiles.Where(x => x.Id == workgroupMember.Id).FirstOrDefault();
                workgroupMember.Name = ticketAssignee.Name;
                workgroupMember.Email = ticketAssignee.Email;

            }
        }

        // set closed by identity
        if (closedById.HasValue)
        {
            mappedTicketEventDto.Data.ClosedBy = GetIdentity(closedById.Value, userProfiles);
        }
    }

    /// <summary>
    /// set identities for getting single ticket.
    /// </summary>
    /// <param name="mappedTicketDto"></param>
    /// <returns></returns>
    public async Task SetIdentitiesAsync(MappedTicketDto mappedTicketDto)
    {
        var isUserAssignee = mappedTicketDto.IsUserAssignee();
        var isWorkgroupAssignee = mappedTicketDto.IsWorkgroupAssignee();
        // this value is optional when ticket closed,
        // we need to check if it has a value before sync the identities
        Guid? closedById = null;

        var userIds = new List<Guid>
        {
            mappedTicketDto.Creator.Id
        };
        if (isUserAssignee)
        {
            userIds.Add(mappedTicketDto.Assignee.Id);
        }
        else if (isWorkgroupAssignee)
        {
            var workgroupUserIds = _context.WorkgroupMembers
                            .Where(x => x.WorkgroupId == mappedTicketDto.AssigneeWorkgroup.Id)
                            .Select(x => x.MemberId)
                            .ToList();
            userIds.AddRange(workgroupUserIds);
        }
        // check closed by identity
        var ticketClosedStatus = await _ticketStatusService.GetClosedStatus();
        if (ticketClosedStatus.Contains((int)Enum.Parse<TicketStatusEnum>(mappedTicketDto.Status,true)))
        {
            var closedStatusAuditTrail = await GetClosedStatusAuditTrail(mappedTicketDto.Id.Value, ticketClosedStatus);

            if (closedStatusAuditTrail is not null && closedStatusAuditTrail.SourceId.HasValue)
            {
                closedById = closedStatusAuditTrail.SourceId.Value;
                userIds.Add(closedById.Value);
            }

        }
        var userProfilesResponse = await _directoryApiService.GetUsersProfilesAsync(new GetUsersProfilesRequest(userIds, null));
        var userProfiles = MappedUserProfile.MapFromUserProfiles(userProfilesResponse);

        //set creator profile
        mappedTicketDto.Creator = GetIdentity(mappedTicketDto.Creator.Id, userProfiles);

        //set assignee profile
        if (isUserAssignee)
        {
            var ticketAssignee = userProfiles.Where(x => x.Id == mappedTicketDto.Assignee.Id).FirstOrDefault();
            if (ticketAssignee is not null)
            {
                mappedTicketDto.Assignee = new MappedAssignee
                {
                    Id = ticketAssignee.Id,
                    Name = ticketAssignee.Name,
                    Email = ticketAssignee.Email
                };
            }
            else
            {
                var userExternalProfile = _context.ExternalProfiles.Where(x => x.Id == mappedTicketDto.Assignee.Id).FirstOrDefault();
                if (userExternalProfile is not null)
                {
                    mappedTicketDto.Assignee.Name = userExternalProfile.Name;
                    mappedTicketDto.Assignee.Email = userExternalProfile.Email;
                }
            }

        }
        else if (isWorkgroupAssignee)
        {
            foreach (var workgroupMember in mappedTicketDto.AssigneeWorkgroup.Assignees)
            {
                var ticketAssignee = userProfiles.Where(x => x.Id == workgroupMember.Id).FirstOrDefault();
                if (ticketAssignee is not null)
                {
                    workgroupMember.Name = ticketAssignee.Name;
                    workgroupMember.Email = ticketAssignee.Email;
                }
            }
        }
        // set closed by identity
        if (closedById.HasValue)
        {
            mappedTicketDto.ClosedBy = GetIdentity(closedById.Value, userProfiles);
        }
    }

    /// <summary>
    /// set identities for the list of tickets
    /// </summary>
    /// <param name="mappedTicketDtos"></param>
    /// <returns></returns>
    public async Task SetIdentitiesAsync(List<MappedTicketDto> mappedTicketDtos)
    {
        var userIds = new List<Guid>();
        userIds.AddRange(mappedTicketDtos.Select(x => x.Creator.Id));
        userIds.AddRange(mappedTicketDtos.Where(x => x.IsUserAssignee()).Select(x => x.Assignee.Id));
        var workgroupIds = mappedTicketDtos.Where(x => x.IsWorkgroupAssignee()).Select(x => x.AssigneeWorkgroup.Id).ToList();
        var workgroupUserIds = _context.WorkgroupMembers
                            .Where(x => workgroupIds.Contains(x.WorkgroupId))
                            .Select(x => x.MemberId)
                            .ToList();

        userIds.AddRange(workgroupUserIds);
        var userProfilesResponse = await _directoryApiService.GetUsersProfilesAsync(new GetUsersProfilesRequest(userIds, null));
        var userProfiles = MappedUserProfile.MapFromUserProfiles(userProfilesResponse);
        // get ids of external profiles
        var externalIds = userIds.Except(userProfiles.Select(x => x.Id)).ToList();
        var externalProfiles = new List<ExternalProfileEntity>();
        if (externalIds.Any())
        {
            externalProfiles = _context.ExternalProfiles.Where(x => externalIds.Distinct().Contains(x.Id)).ToList();
        }


        foreach (var ticket in mappedTicketDtos)
        {
            // set creator profile
            var ticketCreator = userProfiles.Where(x => x.Id == ticket.Creator.Id).FirstOrDefault();
            if (ticketCreator is not null)
            {
                ticket.Creator = ticketCreator;
            }
            else
            {
                var ticketCreatorExternalProfile = externalProfiles.Where(x => x.Id == ticket.Creator.Id).FirstOrDefault();
                if (ticketCreatorExternalProfile is not null)
                {
                    ticket.Creator.Name = ticketCreatorExternalProfile.Name;
                    ticket.Creator.Email = ticketCreatorExternalProfile.Email;
                    ticket.Creator.Phone = ticketCreatorExternalProfile.Phone;
                    ticket.Creator.Company = ticketCreatorExternalProfile.Company;
                }
            }

            // set assignee profile
            if (ticket.IsUserAssignee())
            {
                var ticketAssignee = userProfiles.Where(x => x.Id == ticket.Assignee.Id).FirstOrDefault();
                if (ticketAssignee is not null)
                {
                    ticket.Assignee = new MappedAssignee
                    {
                        Id = ticketAssignee.Id,
                        Name = ticketAssignee.Name,
                        Email = ticketAssignee.Email
                    };
                }
                else
                {
                    var ticketAssigneeExternalProfile = externalProfiles.Where(x => x.Id == ticket.Assignee.Id).FirstOrDefault();
                    if (ticketAssigneeExternalProfile is not null)
                    {
                        ticket.Assignee.Name = ticketAssigneeExternalProfile.Name;
                        ticket.Assignee.Email = ticketAssigneeExternalProfile.Email;
                    }
                }

            }
            else if (ticket.IsWorkgroupAssignee())
            {
                foreach (var workgroupMember in ticket.AssigneeWorkgroup.Assignees)
                {
                    var ticketAssignee = userProfiles.Where(x => x.Id == workgroupMember.Id).FirstOrDefault();
                    if (ticketAssignee is not null)
                    {
                        workgroupMember.Name = ticketAssignee.Name;
                        workgroupMember.Email = ticketAssignee.Email;
                    }


                }
            }
        }



    }

    #region private methods
    /// <summary>
    /// 1- check existing willow user
    //  2- if willow user is not exist , check existing external profile
    //  3- if external profile is not exist, create new external profile
    /// </summary>
    /// <param name="userProfiles"></param>
    /// <param name="assignee"></param>
    private void SetAssigneeId(List<MappedUserProfile> userProfiles, MappedAssignee assignee)
    {
        // check existing willow user
        var assigneeProfile = userProfiles.FirstOrDefault(x => x.Email.Equals(assignee.Email, StringComparison.CurrentCultureIgnoreCase));
        if (assigneeProfile is not null)
        {
            assignee.Id = assigneeProfile.Id;
            assignee.Name = assigneeProfile.Name;
            return;
        }

        // check the external profile table
        var existingExternalProfile = _context.ExternalProfiles.FirstOrDefault(x => x.Email == assignee.Email);
        if (existingExternalProfile is not null)
        {
            assignee.Id = existingExternalProfile.Id;
            assignee.Name = existingExternalProfile.Name;
            return;

        }
        //  create new external profile
        var externalUserProfile = new ExternalProfileEntity
        {
            Id = Guid.NewGuid(),
            Email = assignee.Email,
            Name = assignee.Name,
        };
        // we don't save the changes here, we just add the external profile to the context
        // the changes will be saved in mapped service to create transactional consistency
        _context.ExternalProfiles.Add(externalUserProfile);
        assignee.Id = externalUserProfile.Id;
        assignee.Name = externalUserProfile.Name;
    }

    private void SetWorkgroupAssignee(Guid siteId, MappedWorkgroup assigneeWorkgroup)
    {
        // check existing workgroup
        var existingWorkgroup = _context.Workgroups.FirstOrDefault(x => x.Name == assigneeWorkgroup.Name && x.SiteId == siteId);
        if (existingWorkgroup is not null)
        {
            assigneeWorkgroup.Id = existingWorkgroup.Id;
            assigneeWorkgroup.Name = existingWorkgroup.Name;
            return;
        }
        //if workgroup is not exist, create new workgroup
        var newWorkgroup = new WorkgroupEntity
        {
            Id = Guid.NewGuid(),
            Name = assigneeWorkgroup.Name,
            SiteId = siteId
        };
        _context.Workgroups.Add(newWorkgroup);
        assigneeWorkgroup.Id = newWorkgroup.Id;
        assigneeWorkgroup.Name = newWorkgroup.Name;
    }

    /// <summary>
    /// Set Identity id
    /// First check if user exists in Willow identity
    /// if not, check the existing external profile
    /// if not create new external profile
    /// </summary>
    /// <param name="userProfiles"></param>
    /// <param name="identity"></param>
    private void SetIdentityId(List<MappedUserProfile> userProfiles, MappedUserProfile identity)
    {
        // check willow identities
        var identityProfile = userProfiles.FirstOrDefault(x => x.Email.Equals(identity.Email, StringComparison.CurrentCultureIgnoreCase));
        if (identityProfile is not null)
        {
            identity.Id = identityProfile.Id;
            return;
        }
        // check the external profile table
        var existingExternalProfile = _context.ExternalProfiles.FirstOrDefault(x => x.Email == identity.Email);
        if (existingExternalProfile is not null)
        {
            identity.Id = existingExternalProfile.Id;
            return;
        }
        // create external profile
        var externalUserProfile = new ExternalProfileEntity
        {
            Id = Guid.NewGuid(),
            Email = identity.Email,
            Name = identity.Name,
            Phone = identity.Phone,
            Company = identity.Company
        };
        // we don't save the changes here, we just add the external profile to the context
        // the changes will be saved in mapped service to create transactional consistency
        _context.ExternalProfiles.Add(externalUserProfile);
        identity.Id = externalUserProfile.Id;

    }
    private void SetReporter(Guid CustomerId, Guid SiteId, MappedReporter reporter)
    {
        if(reporter is null)
        {
            return;
        }
        var existingReporter = _context.Reporters.FirstOrDefault(x => x.Email == reporter.ReporterEmail);
        if (existingReporter is not null)
        {
            reporter.ReporterId = existingReporter.Id;
            reporter.ReporterName = existingReporter.Name;
            reporter.ReporterPhone = existingReporter.Phone;
            reporter.ReporterCompany = existingReporter.Company;
        }
        else
        {
            var newReporter = new ReporterEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = CustomerId,
                SiteId = SiteId,
                Email = reporter.ReporterEmail,
                Name = reporter.ReporterName,
                Phone = reporter.ReporterPhone ?? string.Empty,
                Company = reporter.ReporterCompany ?? string.Empty
            };
            // we don't save the changes here, we just add the external profile to the context
            // the changes will be saved mapped service to create transactional consistency
            _context.Reporters.Add(newReporter);
            reporter.ReporterId = newReporter.Id;
            reporter.ReporterName = newReporter.Name;
            reporter.ReporterPhone = newReporter.Phone;
            reporter.ReporterCompany = newReporter.Company;
        }
    }

    private async Task<AuditTrailEntity> GetClosedStatusAuditTrail(Guid ticketId, List<int> ticketStatus)
    {
        var auditTrails = await _auditTrailRepository.GetAuditTrailsAsync(nameof(TicketEntity),
                                                                          nameof(TicketEntity.Status),
                                                                          ticketId);
        var closedStatusAuditTrail = auditTrails.Where(x => ticketStatus
                                                .Select(x => x.ToString())
                                                .Contains(x.NewValue))
                                   .DefaultIfEmpty()
                                   .MaxBy(x => x.Timestamp);

        return closedStatusAuditTrail;

    }

    private MappedUserProfile GetIdentity(Guid userId, List<MappedUserProfile> userProfiles)
    {
        var ticketUserProfile = new MappedUserProfile();
        var existingWillowProfile = userProfiles.Where(x => x.Id == userId).FirstOrDefault();
        if (existingWillowProfile is not null)
        {
           return existingWillowProfile;
        }
        else
        {
            var userExternalProfile = _context.ExternalProfiles.Where(x => x.Id == userId).FirstOrDefault();
            if (userExternalProfile is not null)
            {
                ticketUserProfile.Id = userExternalProfile.Id;
                ticketUserProfile.Name = userExternalProfile.Name;
                ticketUserProfile.Email = userExternalProfile.Email;
                ticketUserProfile.Phone = userExternalProfile.Phone;
                ticketUserProfile.Company = userExternalProfile.Company;
            }

            return ticketUserProfile;
        }
    }

    #endregion
}

