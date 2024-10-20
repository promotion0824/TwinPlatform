using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetCoreTwinCreator.MappingId.Models
{
    [Table("FloorMapping")]
    public class FloorMapping
    {
        [Key]
        public Guid FloorId { get; set; }

        public int BuildingId { get; set; }

        [Required]
        public string FloorCode { get; set; }
    }
}
