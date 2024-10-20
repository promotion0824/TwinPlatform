using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.Common;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Models;
using WorkflowCore.Repository;

namespace WorkflowCore.Services
{
    public interface ICommentsService
    {
        Task<Comment> CreateComment(Guid siteId, Guid ticketId, CreateCommentRequest request);
        Task<bool> DeleteComment(Guid siteId, Guid ticketId, Guid commentId);
		Task<List<TicketActivity>> GetInsightTicketCommentsAsync(Guid insightId);

	}

    public class CommentsService : ICommentsService
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IWorkflowRepository _repository;

        public CommentsService(IDateTimeService dateTimeService, IWorkflowRepository repository)
        {
            _dateTimeService = dateTimeService;
            _repository = repository;
        }

        public async Task<Comment> CreateComment(Guid siteId, Guid ticketId, CreateCommentRequest request)
        {
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                Text = request.Text,
                CreatorType = request.CreatorType,
                CreatorId = request.CreatorId,
                CreatedDate = _dateTimeService.UtcNow
            };
            await _repository.CreateComment(comment);
            return comment;
        }

        public async Task<bool> DeleteComment(Guid siteId, Guid ticketId, Guid commentId)
        {
            return await _repository.DeleteComment(siteId, ticketId, commentId);
        }

		public async Task<List<TicketActivity>> GetInsightTicketCommentsAsync(Guid insightId)
		{
			return await _repository.GetInsightTicketCommentsAsync(insightId);
		}

	}
}
