using System;
using System.Collections.Generic;
using Azure.DigitalTwins.Core;
using DigitalTwinCore.Constants;
using DigitalTwinCore.Extensions;

namespace DigitalTwinCore.Models
{
	[Serializable]
    public class TwinWithRelationships : Twin
    {
        public IReadOnlyCollection<TwinRelationship> Relationships { get; set; } = new List<TwinRelationship>().AsReadOnly();        

        private NullableNullable<Guid> _floorId;
        // If we find that a twin has no floorID, keep it null and don't try to look it up again
        // TODO: Currently if we add/change relationships after this is cached, we won't get updates until we re-sync to ADT
        public Guid? GetFloorId(string[] levelModelIds = null) =>
            _floorId.HasValue ? _floorId.Value : (_floorId.Value = this.FindFloorId(levelModelIds));

        public string GetFloorIdString(string[] levelMdelIds = null) =>
            GetFloorId(levelMdelIds).HasValue ? _floorId.Value.ToString() : null;

        public static new TwinWithRelationships MapFrom(BasicDigitalTwin dto)
        {
            var tr = new TwinWithRelationships
            {
                Id = dto.Id,
                CustomProperties = MapCustomProperties(dto.Contents),
                Metadata = TwinMetadata.MapFrom(dto.Metadata),
                Etag = dto.ETag.GetValueOrDefault().ToString()
            };
            return tr;
        }
	}
}
