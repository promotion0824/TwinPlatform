using System;
using MobileXL.Models;

namespace MobileXL.Services.Apis.WorkflowApi.Requests
{
    public class WorkflowCreateCommentRequest
    {
        public string Text { get; set; }
        public CommentCreatorType CreatorType { get; set; }
        public Guid CreatorId { get; set; }
    }
}
