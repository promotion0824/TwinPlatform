using System;
using WorkflowCore.Models;

namespace WorkflowCore.Controllers.Request
{
    public class CreateCommentRequest
    {
        public string Text { get; set; }
        public CommentCreatorType CreatorType { get; set; }
        public Guid CreatorId { get; set; }
    }
}
