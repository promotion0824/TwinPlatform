using System;
using System.Collections.Generic;
using WorkflowCore.Models;
using WorkflowCore.Services.Apis;

using Willow.Calendar;
using Willow.Common;
using System.Linq;

namespace WorkflowCore.Dto
{
    public class TicketTemplateDto
    {
        public Guid         Id               { get; set; }
        public Guid         CustomerId       { get; set; }
        public Guid         SiteId           { get; set; }
        public string       FloorCode        { get; set; }
        public string       SequenceNumber   { get; set; }
        public int          Priority         { get; set; }
        public int          Status           { get; set; }
        public string       Summary          { get; set; }
        public string       Description      { get; set; }
        
        public Guid?        ReporterId       { get; set; }
        public string       ReporterName     { get; set; }
        public string       ReporterPhone    { get; set; }
        public string       ReporterEmail    { get; set; }
        public string       ReporterCompany  { get; set; }
        
        public AssigneeType AssigneeType     { get; set; }
        public Guid?        AssigneeId       { get; set; }
        
        public DateTime     CreatedDate      { get; set; }
        public DateTime     UpdatedDate      { get; set; }       
        public DateTime?    ClosedDate       { get; set; }

        public SourceType   SourceType       { get; set; }
        public EventDto     Recurrence       { get; set; }
        public string       NextTicketDate   { get; set; }
        public Duration     OverdueThreshold { get; set; }
        public string       Category         { get; set; }
        public Guid?        CategoryId       { get; set; }

        public List<AttachmentDto> Attachments { get; set; }
        public List<TicketAsset>   Assets      { get; set; }
        public List<TicketTwin>    Twins       { get; set; }
        public List<TicketTaskTemplate> Tasks  { get; set; }
        public DataValue           DataValue   { get; set; }

        public static IEnumerable<TicketTemplateDto> MapFromModel(IEnumerable<TicketTemplate> models, IImagePathHelper helper, IDateTimeService dtService)
        {
            return models?.Select(x => MapFromModel(x, helper, dtService));
        }

        public static TicketTemplateDto MapFromModel(TicketTemplate model, IImagePathHelper helper, IDateTimeService dtService)
        {
            var nextTicketDate = model.Recurrence.NextOccurrence(dtService.UtcNow.InTimeZone(model.Recurrence.Timezone));

            return new TicketTemplateDto
            {
                Id               = model.Id,
                CustomerId       = model.CustomerId,
                SiteId           = model.SiteId,
                FloorCode        = model.FloorCode,
                SequenceNumber   = model.SequenceNumber,
                Priority         = model.Priority,
                Status           = model.Status,
                Summary          = model.Summary,
                Description      = model.Description,
                ReporterId       = model.ReporterId,
                ReporterName     = model.ReporterName,
                ReporterPhone    = model.ReporterPhone,
                ReporterEmail    = model.ReporterEmail,
                ReporterCompany  = model.ReporterCompany,
                AssigneeType     = model.AssigneeType,
                AssigneeId       = model.AssigneeId,
                CreatedDate      = model.CreatedDate,
                UpdatedDate      = model.UpdatedDate,
                ClosedDate       = model.ClosedDate,
                SourceType       = model.SourceType,
                Recurrence       = new EventDto
                {
                    Name            = model.Recurrence.Name,
                    StartDate       = model.Recurrence.StartDate.ToString("s"),
                    EndDate         = model.Recurrence.EndDate.HasValue ? model.Recurrence.EndDate.Value.ToString("s") : "",
                    Timezone        = model.Recurrence.Timezone,
                    Occurs          = model.Recurrence.Occurs,
                    MaxOccurrences  = model.Recurrence.MaxOccurrences,
                    Interval        = model.Recurrence.Interval,
                    DayOccurrences  = model.Recurrence.DayOccurrences,
                    Days            = model.Recurrence.Days
                },
                NextTicketDate   = nextTicketDate == DateTime.MaxValue ? null : nextTicketDate.ToString("s"),
                OverdueThreshold = model.OverdueThreshold,
                CategoryId       = model.CategoryId,
                Category         = string.IsNullOrWhiteSpace(model.CategoryName) ? "Unspecified" : model.CategoryName,

                Attachments      = AttachmentDto.MapFromTicketModels(model.Attachments, helper, model),
                Assets           = model.Assets,
                Twins            = model.Twins,
                Tasks            = model.Tasks,
                DataValue        = model.DataValue
            };
        }
    }
}
