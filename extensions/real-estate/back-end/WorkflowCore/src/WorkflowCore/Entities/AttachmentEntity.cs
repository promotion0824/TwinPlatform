using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Entities
{
    [Table("WF_Attachment")]
	public class AttachmentEntity : IAuditTrail
	{
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public AttachmentType Type { get; set; }
        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }

        public TicketEntity Ticket { get; set; }

        public static TicketAttachment MapToModel(AttachmentEntity entity)
        {
            return new TicketAttachment
            {
                Id = entity.Id,
                TicketId = entity.TicketId,
                Type = entity.Type,
                FileName = entity.FileName,
                CreatedDate = entity.CreatedDate,
            };
        }

        public static List<TicketAttachment> MapToModels(IEnumerable<AttachmentEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

        public static AttachmentEntity MapFromModel(TicketAttachment model)
        {
            return new AttachmentEntity
            {
                Id = model.Id,
                TicketId = model.TicketId,
                Type = model.Type,
                FileName = model.FileName,
                CreatedDate = model.CreatedDate,
            };
        }

        public List<string> GetTrackedColumns()
        {
            return [nameof(FileName)];
        }

    }
}
