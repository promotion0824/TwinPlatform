using System;
using System.Collections.Generic;
using System.Linq;
using MobileXL.Models;

namespace MobileXL.Dto
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }

        public CommentCreator Creator { get; set; }

        public static CommentDto MapFromModel(Comment model)
        {
            return new CommentDto
            {
                Id = model.Id,
                TicketId = model.TicketId,
                Text = model.Text,
                CreatedDate = model.CreatedDate,

                Creator = model.Creator
            };
        }

        public static List<CommentDto> MapFromModels(List<Comment> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}
