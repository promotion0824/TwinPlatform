using System;

namespace MobileXL.Models
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public TicketAttachmentType Type { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Path { get; set; }
    }
}
