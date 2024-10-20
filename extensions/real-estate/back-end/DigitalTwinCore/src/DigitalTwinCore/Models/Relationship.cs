using System;
using System.Collections.Generic;
using Azure.DigitalTwins.Core;
using DigitalTwinCore.Dto;

namespace DigitalTwinCore.Models
{
    public class Relationship
    {
        public string Id { get; set; }
        public string TargetId { get; set; }
        public string SourceId { get; set; }
        public string Name { get; set; }
        public string Substance { get; set; }

        public IDictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();

        public DateTime ExportTime => DateTime.UtcNow;

        internal static Relationship MapFrom(BasicRelationship dto)
        {
            return new Relationship
            {
                Id = dto.Id,
                Name = dto.Name,
                SourceId = dto.SourceId,
                TargetId = dto.TargetId,
                CustomProperties = dto.Properties
            };
        }

        public static Relationship MapFrom(RelationshipDto dto)
        {
            return new Relationship
            {
                Id = dto.Id,
                Name = dto.Name,
                SourceId = dto.SourceId,
                TargetId = dto.TargetId,
                Substance = dto.Substance,
                CustomProperties = dto.CustomProperties
            };
        }

        public BasicRelationship MapToDto()
        {
            if(!string.IsNullOrWhiteSpace(Substance))
                CustomProperties?.Add("substance", Substance);

            return new BasicRelationship
            {
                Id = Id,
                Name = Name,
                Properties = CustomProperties,
                SourceId = SourceId,
                TargetId = TargetId
            };
        }
    }
}
