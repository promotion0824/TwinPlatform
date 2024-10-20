using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using WorkflowCore.Controllers.Request;
using Willow.Common;
using Willow.Data;
using Newtonsoft.Json;
using Willow.ExceptionHandling.Exceptions;
using WorkflowCore.Services;

namespace WorkflowCore.Repository
{
    public interface ITicketTemplateRepository
    {
        Task<TicketTemplate>                GetTicketTemplate(Guid templateId);
        Task<IEnumerable<TicketTemplate>>   GetTicketTemplates(Guid siteId, bool? archived);
        Task<TicketTemplate>                CreateTicketTemplate(TicketTemplate template);
        Task                                UpdateTicketTemplate(Guid templateId, UpdateTicketTemplateRequest request);
        Task<long>                          GenerateSequenceNumber(string sequenceNumberPrefix);
    }

    public class TicketTemplateRepository : ITicketTemplateRepository, IReadRepository<Guid, TicketTemplate>
    {
        private readonly WorkflowContext _context;
        private readonly IDateTimeService _dateTimeService;
        private readonly ITicketStatusService _ticketStatusService;

        public TicketTemplateRepository(WorkflowContext context, IDateTimeService dateTimeService, ITicketStatusService ticketStatusService)
        {
            _context = context;
            _dateTimeService = dateTimeService;
            _ticketStatusService = ticketStatusService;
        }

        #region IReadRepository

        public Task<TicketTemplate> Get(Guid id)
        {
            return GetTicketTemplate(id);
        }

        public async IAsyncEnumerable<TicketTemplate> Get(IEnumerable<Guid> ids)
        {
           foreach(var id in ids)
                yield return await Get(id);
        }
        
        #endregion

        #region ITicketTemplateRepository

        public async Task<TicketTemplate> GetTicketTemplate(Guid templateId)
        {
            var entity = await _context.TicketTemplates.Where(x => x.Id == templateId).FirstOrDefaultAsync();

            if(entity == null)
                return null;

            return TicketTemplateEntity.MapToModel(entity);
        }

        public async Task<IEnumerable<TicketTemplate>> GetTicketTemplates(Guid siteId, bool? archived)
        {
            var templates = _context.TicketTemplates.Where(x => x.SiteId == siteId);
            var closedStatus = await _ticketStatusService.GetClosedStatus();

            if (archived.HasValue && archived.Value)
                templates  = templates.Where(x => closedStatus.Contains(x.Status));
            else if(archived.HasValue && !archived.Value)
                templates  = templates.Where(x =>  !closedStatus.Contains(x.Status));

            return templates.OrderBy(x => x.CreatedDate).Select(TicketTemplateEntity.MapToModel);
        }

        public async Task<TicketTemplate> CreateTicketTemplate(TicketTemplate template)
        {
            var category = await _context.TicketCategories.Where( c=> c.Id == template.CategoryId).FirstOrDefaultAsync();

            template.CategoryName = category?.Name;

            var ticketEntity = TicketTemplateEntity.MapFromModel(template);

            _context.TicketTemplates.Add(ticketEntity);

            if(template.Recurrence != null)
            { 
                _context.Schedules.Add(new ScheduleEntity
                {
                    Id              = Guid.NewGuid(),
                    OwnerId         = template.Id,
                    Recurrence      = JsonConvert.SerializeObject(template.Recurrence),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                });
            }

            await _context.SaveChangesAsync();

            return template;
        }

        public async Task<long> GenerateSequenceNumber(string sequenceNumberPrefix)
        {
            var ticketSequenceNumber = await _context.TicketTemplateNextNumbers.AsTracking().FirstOrDefaultAsync(x => x.Prefix == sequenceNumberPrefix);
            if (ticketSequenceNumber == null)
            {
                ticketSequenceNumber = new TicketNextNumberEntity
                {
                    Prefix = sequenceNumberPrefix,
                    NextNumber = 1
                };
                _context.TicketTemplateNextNumbers.Add(ticketSequenceNumber);
            }
            var sequenceNumber = ticketSequenceNumber.NextNumber;
            ticketSequenceNumber.NextNumber++;
            await _context.SaveChangesAsync();
            return sequenceNumber;
        }

        public async Task UpdateTicketTemplate(Guid templateId, UpdateTicketTemplateRequest request)
        {
            var category = await _context.TicketCategories.Where( c=> c.Id == request.CategoryId).FirstOrDefaultAsync();
            var template = await _context.TicketTemplates
                                         .AsTracking()
                                         .FirstOrDefaultAsync(t => t.Id == templateId);

            if(template == null)
                throw new NotFoundException();

            template.CategoryName = category?.Name;

            if (request.Priority.HasValue)
                template.Priority = request.Priority.Value;

            if (request.Status.HasValue && request.Status.Value != template.Status)
                template.Status = request.Status.Value;

            if (request.FloorCode != null)
                template.FloorCode = request.FloorCode;

            if (request.Summary != null)
                template.Summary = request.Summary;

            if (request.Description != null)
                template.Description = request.Description;

            if (request.ShouldUpdateReporterId)
                template.ReporterId = request.ReporterId;

            if (request.ReporterName != null)
                template.ReporterName = request.ReporterName;

            if (request.ReporterPhone != null)
                template.ReporterPhone = request.ReporterPhone;

            if (request.ReporterEmail != null)
                template.ReporterEmail = request.ReporterEmail;

            if (request.ReporterCompany != null)
                template.ReporterCompany = request.ReporterCompany;

            if (request.AssigneeType.HasValue)
            {
                template.AssigneeType = request.AssigneeType.Value;
                template.AssigneeId = request.AssigneeId;
            }

            if(request.Recurrence != null)
            {
                var recurrence     = JsonConvert.SerializeObject(request.Recurrence);
                var removeSchedule = _context.Schedules.Where( s=> s.OwnerId == templateId);

                template.Recurrence = recurrence;

                _context.Schedules.RemoveRange(removeSchedule);
                _context.Schedules.Add(new ScheduleEntity
                {
                    Id              = Guid.NewGuid(),
                    OwnerId         = templateId,
                    Recurrence      = recurrence,
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                });
            }

            if (request.OverdueThreshold != null)
                template.OverdueThreshold = request.OverdueThreshold.ToString();

            if(request.Attachments != null)
                template.Attachments = JsonConvert.SerializeObject(request.Attachments);

            if(request.Assets != null)
                template.Assets = JsonConvert.SerializeObject(request.Assets);

            if (request.Twins != null)
                template.Twins = JsonConvert.SerializeObject(request.Twins);

            if (request.Tasks != null)
                template.Tasks = JsonConvert.SerializeObject(request.Tasks);

            if (request.DataValue != null)
                template.DataValue = JsonConvert.SerializeObject(request.DataValue);

            template.UpdatedDate = _dateTimeService.UtcNow;

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
