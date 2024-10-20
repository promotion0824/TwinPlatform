using System.ComponentModel.DataAnnotations;
using Willow.Model.Mapping;

namespace Willow.AzureDigitalTwins.Api.Persistence.Models.Mapped
{
    public class MappedEntry
    {
        [Key]
        [MaxLength(48)]
        public required string MappedId { get; set; }

        [MaxLength(256)]
        public string? WillowId { get; set; }

        [MaxLength(128)]
        public required string MappedModelId { get; set; }

        [MaxLength(256)]
        public string? WillowModelId { get; set; }

        [MaxLength(48)]
        public string? ParentMappedId { get; set; }

        [MaxLength(256)]
        public string? ParentWillowId { get; set; }

        [MaxLength(32)]
        public string? WillowParentRel { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public string? ModelInformation { get; set; }

        public string? StatusNotes { get; set; }

        public required Status Status { get; set; }

        public string? AuditInformation { get; set; }

        public required DateTimeOffset TimeCreated { get; set; } = DateTimeOffset.UtcNow;

        public required DateTimeOffset TimeLastUpdated { get; set; } = DateTimeOffset.UtcNow;

        [MaxLength(256)]
        public string? ConnectorId { get; set; }

        [MaxLength(256)]
        public string? BuildingId { get; set; }

        public bool IsExistingTwin { get; set; } = false;

        [MaxLength(256)]
        public string? Unit { get; set; }
        [MaxLength(256)]
        public string? DataType { get; set; }
    }
}
