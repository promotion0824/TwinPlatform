using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkflowCore.Models;

namespace WorkflowCore.Entities
{
    [Table("WF_TicketCategory")]
    public class TicketCategoryEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? SiteId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(80)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// represents the last time the record was updated in utc time
        /// the default value in database is the current time in utc
        /// </summary>
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

        public ICollection<TicketEntity> Tickets { get; set; }

        public static TicketCategory MapToModel(TicketCategoryEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new TicketCategory
            {
                Id = entity.Id,
                SiteId = entity.SiteId,
                Name = entity.Name
            };
        }

        public static List<TicketCategory> MapToModels(IEnumerable<TicketCategoryEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

        public static TicketCategoryEntity MapFromModel(TicketCategory model)
        {
            if (model == null)
            {
                return null;
            }

            return new TicketCategoryEntity
            {
                Id = model.Id,
                SiteId = model.SiteId,
                Name = model.Name
            };
        }
    }
}
