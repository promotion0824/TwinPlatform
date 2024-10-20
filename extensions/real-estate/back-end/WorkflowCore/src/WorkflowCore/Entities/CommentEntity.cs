using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Entities
{
    [Table("WF_Comment")]
    public class CommentEntity
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        [Required(AllowEmptyStrings = true)]
        [MaxLength(2048)]
        public string Text { get; set; }
        public CommentCreatorType CreatorType { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime CreatedDate { get; set; }

        public TicketEntity Ticket { get; set; }

        public static Comment MapToModel(CommentEntity entity)
        {
            return new Comment
            {
                Id = entity.Id,
                TicketId = entity.TicketId,
                Text = entity.Text,
                CreatorType = entity.CreatorType,
                CreatorId = entity.CreatorId,
                CreatedDate = entity.CreatedDate,
            };
        }

        public static List<Comment> MapToModels(IEnumerable<CommentEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

        public static CommentEntity MapFromModel(Comment model)
        {
            return new CommentEntity
            {
                Id = model.Id,
                TicketId = model.TicketId,
                Text = model.Text,
                CreatorType = model.CreatorType,
                CreatorId = model.CreatorId,
                CreatedDate = model.CreatedDate,
            };
        }
    }
}
