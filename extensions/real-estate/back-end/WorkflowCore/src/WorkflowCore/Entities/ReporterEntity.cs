using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Entities
{
    [Table("WF_Reporter")]
    public class ReporterEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SiteId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(500)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(32)]
        public string Phone { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(64)]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(64)]
        public string Company { get; set; }

        public static Reporter MapToModel(ReporterEntity entity)
        {
            return new Reporter
            {
                Id = entity.Id,
                CustomerId = entity.CustomerId,
                SiteId = entity.SiteId,
                Name = entity.Name,
                Phone = entity.Phone,
                Email = entity.Email,
                Company = entity.Company,
            };
        }

        public static List<Reporter> MapToModels(IEnumerable<ReporterEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

        public static ReporterEntity MapFromModel(Reporter model)
        {
            return new ReporterEntity
            {
                Id = model.Id,
                CustomerId = model.CustomerId,
                SiteId = model.SiteId,
                Name = model.Name,
                Phone = model.Phone,
                Email = model.Email,
                Company = model.Company,
            };
        }
    }
}
