using System;

namespace Willow.Workflow
{
    public class WorkflowCreateCommentRequest
    {
        public string Text { get; set; }
        public CommentCreatorType CreatorType { get; set; }
        public Guid CreatorId { get; set; }
    }
}
