using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetCoreTwinCreator.MappingId.Models
{
    [Table("AssetEquipmentMapping")]
    public class AssetEquipmentMapping
    {
        [Key]
        public int AssetRegisterId { get; set; }

        public Guid EquipmentId { get; set; }
    }
}
