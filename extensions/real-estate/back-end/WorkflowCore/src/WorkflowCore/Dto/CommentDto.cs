using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Dto
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public string Text { get; set; }
        public CommentCreatorType CreatorType { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime CreatedDate { get; set; }

        public static CommentDto MapFromModel(Comment model)
        {
            return new CommentDto
            {
                Id = model.Id,
                TicketId = model.TicketId,
                Text = model.Text,
                CreatorType = model.CreatorType,
                CreatorId = model.CreatorId,
                CreatedDate = model.CreatedDate
            };
        }

        public static List<CommentDto> MapFromModels(List<Comment> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}
