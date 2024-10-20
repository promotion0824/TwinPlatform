using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalTwinCore.Entities
{
    [Table("DT_GeometryViewerModels")]
    public class GeometryViewerModelEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100)]
        public string TwinId { get; set; }

        public bool Is3D { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(1024)]
        public string Urn { get; set; }

        [InverseProperty(nameof(GeometryViewerReferenceEntity.GeometryViewerModel))]
        public List<GeometryViewerReferenceEntity> References { get; set; } = new List<GeometryViewerReferenceEntity>();
    }
}
