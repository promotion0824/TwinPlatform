using MobileXL.Models;
using MobileXL.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobileXL.Dto
{
    public class TicketTemplateDto
    {
        public Guid Id { get; set; }
        public string FloorCode { get; set; }
        public string SequenceNumber { get; set; }
        public int Priority { get; set; }
        public TicketStatus Status { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public Guid? ReporterId { get; set; }
        public string ReporterName { get; set; }
        public string ReporterPhone { get; set; }
        public string ReporterEmail { get; set; }
        public string ReporterCompany { get; set; }
        public TicketAssigneeType AssigneeType { get; set; }
        public Guid? AssigneeId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public TicketSourceType SourceType { get; set; }
        public List<TicketAssetDto> Assets { get; set; }
        public List<TicketTaskDto> Tasks { get; set; }
        public List<AttachmentDto> Attachments { get; set; }

        public static TicketTemplateDto MapFromModel(TicketTemplate model, IImageUrlHelper helper)
        {
            return new TicketTemplateDto
            {
                Id = model.Id,
                FloorCode = model.FloorCode,
                SequenceNumber = model.SequenceNumber,
                Priority = model.Priority,
                Status = model.Status,
                Summary = model.Summary,
                Description = model.Description,
                ReporterId = model.ReporterId,
                ReporterName = model.ReporterName,
                ReporterPhone = model.ReporterPhone,
                ReporterEmail = model.ReporterEmail,
                ReporterCompany = model.ReporterCompany,
                AssigneeType = model.AssigneeType,
                AssigneeId = model.AssigneeId,
                CreatedDate = model.CreatedDate,
                UpdatedDate = model.UpdatedDate,
                ClosedDate = model.ClosedDate,
                SourceType = model.SourceType,
                Attachments = AttachmentDto.MapFromModels(model.Attachments, helper),
                Assets = TicketAssetDto.MapFromModels(model.Assets),
                Tasks = TicketTaskDto.MapFromModels(model.Tasks)
            };
        }

        public static List<TicketTemplateDto> MapFromModels(List<TicketTemplate> models, IImageUrlHelper helper)
        {
            return models?.Select(x => MapFromModel(x, helper)).ToList();
        }
    }
}
