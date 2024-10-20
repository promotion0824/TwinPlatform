using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetCoreTwinCreator.MappingId.Models
{
    [Table("SiteBuildingMapping")]
    public class SiteMapping
    {
        [Key]
        public Guid SiteId { get; set; }

        public int BuildingId { get; set; }
    }
}
