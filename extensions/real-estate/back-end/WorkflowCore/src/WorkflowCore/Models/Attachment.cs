using System;

namespace WorkflowCore.Models
{
    public class AttachmentBase
    {
        public Guid Id { get; set; }
        public AttachmentType Type { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class TicketAttachment : AttachmentBase
    {
        public Guid TicketId { get; set; }
    }
}
