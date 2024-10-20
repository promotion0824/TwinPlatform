using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Common;
using Willow.Calendar;
using Willow.Scheduler;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Repository;
using WorkflowCore.Models;
using WorkflowCore.Services.Apis;
using Willow.ExceptionHandling.Exceptions;

namespace WorkflowCore.Services
{
    public interface ITicketTemplateService : IScheduleRecipient
    {
        Task<IEnumerable<TicketTemplate>>   GetTicketTemplates(Guid siteId, bool? archived);
        Task<TicketTemplate>                GetTicketTemplate(Guid templateId);
        Task<TicketTemplate>                CreateTicketTemplate(Guid siteId, CreateTicketTemplateRequest request, string language);
        Task<TicketTemplate>                UpdateTicketTemplate(Guid templateId, UpdateTicketTemplateRequest request, string language);

        Task<List<ScheduledTicketTwin>>     GetScheduledTwins(ScheduleHit scheduleHit);
        [Obsolete("Instead use Task<List<ScheduledTicketTwin>> GetScheduledTwins(ScheduleHit scheduleHit)")]
        Task<List<ScheduledTicketAsset>>    GetScheduledAssets(ScheduleHit scheduleHit);
        Task                                CreateScheduledTicketForTwin(ScheduledTicketTwin twin);
        [Obsolete("Instead use Task CreateScheduledTicketForTwin(ScheduledTicketTwin twin)")]
        Task                                CreateScheduledTicketForAsset(ScheduledTicketAsset asset);
     }

    public class TicketTemplateService : ITicketTemplateService
    {
        private readonly IDateTimeService           _dateTimeService;
        private readonly ITicketTemplateRepository  _repository;
        private readonly IWorkflowService           _workflowService;
        private readonly IDigitalTwinServiceApi     _digitalTwinApi;
        private readonly int                        _advance;

        public TicketTemplateService(IDateTimeService          dateTimeService, 
                                     ITicketTemplateRepository repository, 
                                     IWorkflowService          workflowService,
                                     IDigitalTwinServiceApi    digitalTwinApi,
                                     int                       advance)
        {
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _repository      = repository ?? throw new ArgumentNullException(nameof(repository));
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _digitalTwinApi  = digitalTwinApi ?? throw new ArgumentNullException(nameof(digitalTwinApi));
            _advance         = advance;
        }

        #region ITicketTemplateService

        public Task<IEnumerable<TicketTemplate>> GetTicketTemplates(Guid siteId, bool? archived)
        {
            return _repository.GetTicketTemplates(siteId, archived);
        }

        public Task<TicketTemplate> GetTicketTemplate(Guid templateId)
        {
            return _repository.GetTicketTemplate(templateId);
        }

        public async Task<TicketTemplate> CreateTicketTemplate(Guid siteId, CreateTicketTemplateRequest request, string language)
        {
            var sequenceNumber = await _workflowService.GenerateSequenceNumber(request.SequenceNumberPrefix);
            var utcNow         = _dateTimeService.UtcNow;
            var template       = new TicketTemplate
            {
                Id               = Guid.NewGuid(),
                CustomerId       = request.CustomerId,
                SiteId           = siteId,
                FloorCode        = request.FloorCode ?? string.Empty,
                SequenceNumber   = sequenceNumber,
                Priority         = request.Priority,
                Status           = request.Status,
                Summary          = request.Summary ?? string.Empty,
                Description      = request.Description ?? string.Empty,

                ReporterId       = request.ReporterId,
                ReporterName     = request.ReporterName ?? string.Empty,
                ReporterPhone    = request.ReporterPhone ?? string.Empty,
                ReporterEmail    = request.ReporterEmail ?? string.Empty,
                ReporterCompany  = request.ReporterCompany ?? string.Empty,

                AssigneeType     = request.AssigneeType,
                AssigneeId       = request.AssigneeId,
                CategoryId       = request.CategoryId,

                CreatedDate      = utcNow,
                UpdatedDate      = utcNow,
                ClosedDate       = null,

                SourceType       = request.SourceType,

                Assets           = request.Assets,
                Twins            = request.Twins,
                Tasks            = request.Tasks,
                DataValue        = request.DataValue,
                Recurrence       = new Event
                {
                    Name            = request.Recurrence.Name,
                    StartDate       = DateTime.Parse(request.Recurrence.StartDate),
                    EndDate         = !string.IsNullOrWhiteSpace(request.Recurrence.EndDate) ? DateTime.Parse(request.Recurrence.EndDate) : null,
                    Timezone        = request.Recurrence.Timezone,
                    Occurs          = request.Recurrence.Occurs,
                    MaxOccurrences  = request.Recurrence.MaxOccurrences,
                    Interval        = request.Recurrence.Interval,
                    DayOccurrences  = request.Recurrence.DayOccurrences,
                    Days            = request.Recurrence.Days
                },
                OverdueThreshold = request.OverdueThreshold
            };

            var newTemplate = await _repository.CreateTicketTemplate(template);

            var schedule = CheckSchedule(template.Id, EventDto.MapToModel(request.Recurrence), utcNow);

            if (schedule.IsScheduleHitPending)
            { 
                await PerformScheduleHit(schedule.ScheduleHit, language);
            }

            return newTemplate;
        }

        public async Task<TicketTemplate> UpdateTicketTemplate(Guid templateId, UpdateTicketTemplateRequest request, string language)
        {
            var customerTicketStatuses = await _workflowService.GetTicketStatus(request.CustomerId);

            if (request.Status.HasValue &&
                    !customerTicketStatuses.Any(ts => ts.StatusCode == request.Status) &&
                        !Enum.IsDefined(typeof(TicketStatusEnum), request.Status))
            {
                throw new ArgumentException("Not a valid status").WithData(new { Status = request.Status });
            }

            if ((request.PerformScheduleHitOnAddedAssets ?? false) && request.Assets != null)
            {
                var template = await _repository.GetTicketTemplate(templateId);
                var addedTemplateAssets = request.Assets.Where(x => !template.Assets.Any(y => y.AssetId == x.AssetId)).ToList();
                var addedTemplateTwins = request.Twins?.Where(x => template.Twins != null && !template.Twins.Any(y => y.TwinId == x.TwinId))?.ToList();

                await _repository.UpdateTicketTemplate(templateId, request);

                if (addedTemplateAssets.Any())
                {
                    var recurrence = request.Recurrence != null ? EventDto.MapToModel(request.Recurrence) : template.Recurrence;
                    var schedule  = CheckSchedule(templateId, recurrence);

                    await PerformScheduleHit(schedule.ScheduleHit, addedTemplateAssets, language);
                }

                if (addedTemplateTwins?.Any() ?? false)
                {
                    var recurrence = request.Recurrence != null ? EventDto.MapToModel(request.Recurrence) : template.Recurrence;
                    var schedule = CheckSchedule(templateId, recurrence);

                    await PerformScheduleHit(schedule.ScheduleHit, addedTemplateTwins, language);
                }
            }
            else
            {
                await _repository.UpdateTicketTemplate(templateId, request);
            }

            return await _repository.GetTicketTemplate(templateId);
        }

        public async Task<List<ScheduledTicketTwin>> GetScheduledTwins(ScheduleHit scheduleHit)
        {
            var template = await GetScheduleHitTemplate(scheduleHit);
            var occurrence = scheduleHit.HitDate.AddDays(-_advance).Daydex();
            var utcNow = _dateTimeService.UtcNow;
            var siteCode = template.SequenceNumber.SubstringBefore("-S-");
            var result = new List<ScheduledTicketTwin>();

            foreach (var twin in template.Twins)
            {
                if (await _workflowService.TicketOccurrenceExists(template.Id, twin.TwinId, occurrence))
                    continue;

                result.Add(new ScheduledTicketTwin
                {
                    TemplateId = template.Id,
                    TwinId = twin.TwinId,
                    TwinName = twin.TwinName,
                    SequenceNumber = await _workflowService.GenerateSequenceNumber(siteCode), // Generate sequence number to guarentee the order
                    Occurrence = occurrence,
                    ScheduleHitDate = scheduleHit.HitDate,
                    UtcNow = utcNow
                });
            }

            if (template.Assets?.Any() ?? false)
            {
                var assetTwindIdDto = await _digitalTwinApi.GetTwinIdsByUniqueIdsAsync(template.SiteId, template.Assets.Select(x => x.AssetId));

                foreach (var asset in template.Assets)
                {
                    var twinId = assetTwindIdDto.FirstOrDefault(x => x.UniqueId == asset.AssetId.ToString()).Id;

                    if (await _workflowService.TicketOccurrenceExists(template.Id, asset.AssetId, occurrence)
                        || await _workflowService.TicketOccurrenceExists(template.Id, twinId, occurrence))
                        continue;

                    result.Add(new ScheduledTicketTwin
                    {
                        TemplateId = template.Id,
                        TwinId = twinId,
                        TwinName = asset.AssetName,
                        SequenceNumber = await _workflowService.GenerateSequenceNumber(siteCode), // Generate sequence number to guarentee the order
                        Occurrence = occurrence,
                        ScheduleHitDate = scheduleHit.HitDate,
                        UtcNow = utcNow
                    });
                }
            }

            return result;
        }

        [Obsolete("Instead use Task<List<ScheduledTicketTwin>> GetScheduledTwins(ScheduleHit scheduleHit)")]
        public async Task<List<ScheduledTicketAsset>> GetScheduledAssets(ScheduleHit scheduleHit)
        {
            var template   = await GetScheduleHitTemplate(scheduleHit);
            var occurrence = scheduleHit.HitDate.AddDays(-_advance).Daydex();
            var utcNow     = _dateTimeService.UtcNow;
            var siteCode   = template.SequenceNumber.SubstringBefore("-S-");
            var result     = new List<ScheduledTicketAsset>();

            foreach(var asset in template.Assets)
            {
                if(await _workflowService.TicketOccurrenceExists(template.Id, asset.AssetId, occurrence))
                    continue;

                result.Add(new ScheduledTicketAsset
                {
                    Id              = asset.Id,
                    TemplateId      = template.Id,
                    AssetId         = asset.AssetId,
                    AssetName       = asset.AssetName,
                    SequenceNumber  = await _workflowService.GenerateSequenceNumber(siteCode), // Generate sequence number to guarentee the order
                    Occurrence      = occurrence,
                    ScheduleHitDate = scheduleHit.HitDate,
                    UtcNow          = utcNow
                });
            }

            if (template.Twins?.Any() ?? false)
            {
                foreach (var twin in template.Twins)
                {
                    if (await _workflowService.TicketOccurrenceExists(template.Id, twin.TwinId, occurrence))
                        continue;

                    result.Add(new ScheduledTicketAsset
                    {
                        TemplateId = template.Id,
                        TwinId = twin.TwinId,
                        AssetId = Guid.NewGuid(),
                        AssetName = twin.TwinName,
                        SequenceNumber = await _workflowService.GenerateSequenceNumber(siteCode), // Generate sequence number to guarentee the order
                        Occurrence = occurrence,
                        ScheduleHitDate = scheduleHit.HitDate,
                        UtcNow = utcNow
                    });
                }
            }

            return result;
        }

        public async Task CreateScheduledTicketForTwin(ScheduledTicketTwin twin)
        {
            var template = await GetTicketTemplate(twin.TemplateId);

            await CreateScheduledTicketForTwin(template, twin.TwinId, twin.TwinName, twin.Occurrence, twin.ScheduleHitDate, twin.UtcNow, twin.SequenceNumber, "");
        }

        [Obsolete("Instead use Task CreateScheduledTicketForTwin(ScheduledTicketTwin twin)")]
        public async Task CreateScheduledTicketForAsset(ScheduledTicketAsset asset)
        {
            var template = await GetTicketTemplate(asset.TemplateId);

            var twinIds = await _digitalTwinApi.GetTwinIdsByUniqueIdsAsync(template.SiteId, new List<Guid> { asset.AssetId });
            var twinId = twinIds?.FirstOrDefault()?.Id;

            if (twinId != null)
            {
                await CreateScheduledTicketForTwin(template, twinId, asset.AssetName, asset.Occurrence, asset.ScheduleHitDate, asset.UtcNow, asset.SequenceNumber, "");
            }
        }

        private (bool IsScheduleHitPending, ScheduleHit ScheduleHit) CheckSchedule(Guid templateId, Event recurrence, DateTime? utcNow = null)
        {
            var siteNow      = (utcNow ?? _dateTimeService.UtcNow).InTimeZone(recurrence.Timezone).Date;
            var startDate    = recurrence.StartDate.Date;
            var okToRun      = (siteNow <= startDate && startDate < siteNow.AddDays(_advance)) ||
                               (siteNow >= startDate && siteNow < startDate.AddDays(_advance));

            return (okToRun, new ScheduleHit
                             {
                                 ScheduleId = Guid.NewGuid(),
                                 OwnerId = templateId,
                                 HitDate = startDate.AddDays(-_advance)
                             }
                   );
        }

        #endregion

        #region IScheduleRecipient

         /// <summary>
        /// Creates a ticket from a template via a scheduler "hit" (specific occurrence)
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="createRequest"></param>
        /// <returns></returns>
        public async Task PerformScheduleHit(ScheduleHit scheduleHit, string language)
        {
            var template = await GetScheduleHitTemplate(scheduleHit);
			var occurrence = 0;
			switch (scheduleHit.Recurrence)
			{
				case Event.Recurrence.Weekly:
					occurrence = scheduleHit.HitDate.WeekIndex();
					break;
				case Event.Recurrence.Monthly:
					occurrence = scheduleHit.HitDate.MonthIndex();
					break;
				case Event.Recurrence.Yearly:
					occurrence = scheduleHit.HitDate.YearIndex();
					break;
				default:
					occurrence = scheduleHit.HitDate.Daydex();
					break;
			}

            template.Twins ??= new List<TicketTwin>();

            var templateAssets = template.Assets?.Where(x => !template.Twins.Any(y => y.TwinId == x.AssetId.ToString()));
            if (templateAssets?.Any() ?? false)
            {
                var assetTwindIdDtos = await _digitalTwinApi.GetTwinIdsByUniqueIdsAsync(template.SiteId, templateAssets.Select(x => x.AssetId));

                template.Twins.AddRange(assetTwindIdDtos.Select(x => new TicketTwin
                {
                    TwinId = x.Id,
                    TwinName = templateAssets.FirstOrDefault(y => y.AssetId.ToString() == x.UniqueId).AssetName
                }));
            }

            await CreateScheduledTicketsForTwins(template, template.Twins, occurrence, scheduleHit.HitDate, language);
        }

        public async Task PerformScheduleHit(ScheduleHit scheduleHit, IEnumerable<TicketTwin> addedTemplateTwins, string language)
        {
            if (addedTemplateTwins != null)
            {
                var template = await GetScheduleHitTemplate(scheduleHit);

                var occurrence = scheduleHit.HitDate.Daydex();

                await CreateScheduledTicketsForTwins(template, addedTemplateTwins, occurrence, scheduleHit.HitDate, language);
            }
        }

        [Obsolete("Task PerformScheduleHit(ScheduleHit scheduleHit, IEnumerable<TicketTwin> addedTemplateTwins, string language)")]
        public async Task PerformScheduleHit(ScheduleHit scheduleHit, IEnumerable<TicketAsset> addedTemplateAssets, string language)
        {
            if (addedTemplateAssets != null)
            {
                var template = await GetScheduleHitTemplate(scheduleHit);

                var occurrence = scheduleHit.HitDate.Daydex();

                await CreateScheduledTicketsForAssets(template, addedTemplateAssets, occurrence, scheduleHit.HitDate, language);
            }
        }

        private async Task<TicketTemplate> GetScheduleHitTemplate(ScheduleHit scheduleHit)
        {
            var template = await _repository.GetTicketTemplate(scheduleHit.OwnerId);

            if (template == null)
                throw new NotFoundException("TicketTemplate not found");

            return template;
        }

        private async Task CreateScheduledTicketsForTwins(TicketTemplate template, IEnumerable<TicketTwin> templateTwins, int occurrence, DateTime scheduleHitDate, string language)
        {
            var utcNow = _dateTimeService.UtcNow;
            var siteCode = template.SequenceNumber.SubstringBefore("-S-");

            // Create a separate ticket for each asset in the template
            foreach (var twin in templateTwins)
            {
                if (await _workflowService.TicketOccurrenceExists(template.Id, twin.TwinId, occurrence))
                    continue;

                var sequenceNumber = await _workflowService.GenerateSequenceNumber(siteCode);

                await CreateScheduledTicketForTwin(template, twin.TwinId, twin.TwinName, occurrence, scheduleHitDate, utcNow, sequenceNumber, language);
            }
        }

        [Obsolete("Instead use Task CreateScheduledTicketsForTwins(TicketTemplate template, IEnumerable<TicketTwin> templateTwins, int occurrence, DateTime scheduleHitDate, string language)")]
        private async Task CreateScheduledTicketsForAssets(TicketTemplate template, IEnumerable<TicketAsset> templateAssets, int occurrence, DateTime scheduleHitDate, string language)
        {
            var utcNow = _dateTimeService.UtcNow;
            var siteCode = template.SequenceNumber.SubstringBefore("-S-");

            // Create a separate ticket for each asset in the template
            foreach (var asset in templateAssets)
            {
                var twinIds = await _digitalTwinApi.GetTwinIdsByUniqueIdsAsync(template.SiteId, new List<Guid> { asset.AssetId });
                var twinId = twinIds?.FirstOrDefault()?.Id;

                if (await _workflowService.TicketOccurrenceExists(template.Id, asset.AssetId, occurrence)
                    || await _workflowService.TicketOccurrenceExists(template.Id, twinId, occurrence))
                    continue;

                var sequenceNumber = await _workflowService.GenerateSequenceNumber(siteCode);

                await CreateScheduledTicketForTwin(template, twinId, asset.AssetName, occurrence, scheduleHitDate, utcNow, sequenceNumber, language);
            }
        }

        private async Task CreateScheduledTicketForTwin(TicketTemplate template, string twinId, string twinName, int occurrence, DateTime scheduleHitDate, DateTime utcNow, string sequenceNumber, string language)
        {
            var newId = Guid.NewGuid();
            var taskOrder = 0;

            var ticket = new Ticket
            {
                Id = newId,
                CustomerId = template.CustomerId,
                TemplateId = template.Id,
                SiteId = template.SiteId,
                SequenceNumber = sequenceNumber,
                CreatedDate = utcNow,
                UpdatedDate = utcNow,
                SourceType = template.SourceType,
                Status = template.Status,
                Priority = template.Priority,
                AssigneeId = template.AssigneeId,
                AssigneeType = template.AssigneeType,
                Summary = template.Summary,
                Description = template.Description,
                FloorCode = template.FloorCode,

                ReporterId = template.ReporterId,
                ReporterName = template.ReporterName ?? string.Empty,
                ReporterPhone = template.ReporterPhone ?? string.Empty,
                ReporterEmail = template.ReporterEmail ?? string.Empty,
                ReporterCompany = template.ReporterCompany ?? string.Empty,

                Occurrence = occurrence,
                ScheduledDate = scheduleHitDate,
                DueDate = scheduleHitDate.Add(template.OverdueThreshold),

                TwinId = twinId,
                IssueId = null,
                IssueName = twinName,
                IssueType = IssueType.Asset,

                CategoryId = template.CategoryId,

                Tasks = template.Tasks?.Any() ?? false ? template.Tasks.Select(t => new TicketTask
                {
                    Id = Guid.NewGuid(),
                    IsCompleted = false,
                    TaskName = t.Description,
                    Type = t.Type,
                    DecimalPlaces = t.DecimalPlaces,
                    MinValue = t.MinValue,
                    MaxValue = t.MaxValue,
                    Unit = t.Unit,
                    Order = ++taskOrder
                }).ToList() : null,

                Cause = string.Empty,
                Solution = string.Empty,
                ExternalId = string.Empty,
                ExternalMetadata = string.Empty,
                ExternalStatus = string.Empty,
                InsightName = string.Empty,
                Notes = string.Empty,
            };

            await _workflowService.CreateTicket(new List<Ticket> { ticket }, ticket.SiteId, language);
        }

         #endregion
    }
}
