using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;
using WorkflowCore.Services.Apis;

namespace WorkflowCore.Dto
{
    public class AttachmentDto
    {
        public Guid Id { get; set; }
        public AttachmentType Type { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }

        public string Path { get; set; }

        public static AttachmentDto MapFromTicketModel(TicketAttachment model, IImagePathHelper helper, TicketBase ticket)
        {
            return new AttachmentDto
            {
                Id = model.Id,
                Type = model.Type,
                FileName = model.FileName,
                CreatedDate = model.CreatedDate,

                Path = helper.GetTicketAttachmentsPath(ticket.CustomerId, ticket.SiteId, ticket.Id)
            };
        }

        public static AttachmentDto MapFromModel(AttachmentBase model, IImagePathHelper helper, TicketBase ticket)
        {
            return new AttachmentDto
            {
                Id = model.Id,
                Type = model.Type,
                FileName = model.FileName,
                CreatedDate = model.CreatedDate,

                Path = helper.GetTicketAttachmentsPath(ticket.CustomerId, ticket.SiteId, ticket.Id)
            };
        }

        public static List<AttachmentDto> MapFromTicketModels(List<TicketAttachment> models, IImagePathHelper helper, TicketBase ticket)
        {
            return models?.Select(x => MapFromTicketModel(x, helper, ticket)).ToList();
        }

        public static AttachmentDto MapFromCheckRecordModel(AttachmentBase model, IImagePathHelper helper, Guid customerId, Guid siteId, Guid checkRecordId)
        {
            return new AttachmentDto
            {
                Id = model.Id,
                Type = model.Type,
                FileName = model.FileName,
                CreatedDate = model.CreatedDate,

                Path = helper.GetCheckRecordAttachmentsPath(customerId, siteId, checkRecordId)
            };
        }

        public static List<AttachmentDto> MapFromCheckRecordModels(List<AttachmentBase> models, IImagePathHelper helper, Guid customerId, Guid siteId, Guid checkRecordId)
        {
            return models?.Select(x => MapFromCheckRecordModel(x, helper, customerId, siteId, checkRecordId)).ToList();
        }
    }
}
