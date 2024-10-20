using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalTwinCore.Entities
{
    [Table("DT_GeometryViewerReferences")]
    public class GeometryViewerReferenceEntity
    {
		[Key]
		public Guid Id { get; set; }

		[Required]
        public string GeometryViewerId { get; set; }

        [Required]
        public Guid GeometryViewerModelId { get; set; }

        [ForeignKey(nameof(GeometryViewerModelId))]
        public GeometryViewerModelEntity GeometryViewerModel { get; set; }
    }
}
