using System;
using System.Collections.Generic;
using System.Linq;

using PlatformPortalXL.Services;

using Willow.Calendar;
using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class TicketTemplateDto
    {
        public Guid         Id               { get; set; }
        public Guid         CustomerId       { get; set; }
        public Guid         SiteId           { get; set; }
        public string       FloorCode        { get; set; }
        public string       SequenceNumber   { get; set; }
        public int          Priority         { get; set; }
        public TicketStatus Status           { get; set; }
        public string       Summary          { get; set; }
        public string       Description      { get; set; }
        
        public Guid?        ReporterId       { get; set; }
        public string       ReporterName     { get; set; }
        public string       ReporterPhone    { get; set; }
        public string       ReporterEmail    { get; set; }
        public string       ReporterCompany  { get; set; }
        
        public TicketAssigneeType AssigneeType { get; set; }
        public Guid?        AssigneeId       { get; set; }
        public string       AssigneeName     { get; set; }

        public DateTime     CreatedDate      { get; set; }
        public DateTime     UpdatedDate      { get; set; }       
        public DateTime?    ClosedDate       { get; set; }

        public TicketSourceType SourceType   { get; set; }
        public EventDto     Recurrence       { get; set; }
        public string       NextTicketDate   { get; set; }
        public Duration     OverdueThreshold { get; set; }
        public string       Category         { get; set; }
        public Guid?        CategoryId       { get; set; }        
        
        public TicketAssignee Assignee { get; set; }
        public List<CommentDto> Comments { get; set; }
        public List<AttachmentDto> Attachments { get; set; }

        public List<TicketAssetDto> Assets { get; set; }
        public List<TicketTwinDto> Twins { get; set; }
        public List<TicketTaskTemplateDto> Tasks { get; set; }
        public DataValue    DataValue       { get; set; }

        public static TicketTemplateDto MapFromModel(TicketTemplate model, IImageUrlHelper helper)
        {
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
                AssigneeName     = model.Assignee == null ? null : (model.Assignee.FirstName + " " + model.Assignee.LastName).Trim(),
                CreatedDate      = model.CreatedDate,
                UpdatedDate      = model.UpdatedDate,
                ClosedDate       = model.ClosedDate,
                SourceType       = model.SourceType,
                CategoryId       = model.CategoryId,
                Category         = model.Category,
                Assignee         = model.Assignee,
                Recurrence       = model.Recurrence,
                NextTicketDate   = string.IsNullOrWhiteSpace(model.NextTicketDate) ? null : model.NextTicketDate,
                OverdueThreshold = model.OverdueThreshold,
                Comments         = CommentDto.MapFromModels(model.Comments),
                Attachments      = AttachmentDto.MapFromModels(model.Attachments, helper),
                Assets           = TicketAssetDto.MapFromModels(model.Assets),
                Twins            = TicketTwinDto.MapFromModels(model.Twins),
                Tasks            = TicketTaskTemplateDto.MapFromModels(model.Tasks),
                DataValue        = model.DataValue
            };
        }

        public static List<TicketTemplateDto> MapFromModels(List<TicketTemplate> models, IImageUrlHelper helper)
        {
            return models?.Select(x => MapFromModel(x, helper)).ToList();
        }
    }
}
