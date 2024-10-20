using System;

namespace WorkflowCore.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public string Text { get; set; }
        public CommentCreatorType CreatorType { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
