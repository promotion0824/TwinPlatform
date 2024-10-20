using System;
using DigitalTwinCore.Models;


namespace DigitalTwinCore.DTO
{
    public class TwinGeometryViewerIdDto
    {
        public string TwinId { get; set; }
        public Guid? UniqueId { get; set; }
        public Guid? GeometryViewerId { get; set; }

        public static TwinGeometryViewerIdDto MapFromModel(TwinWithRelationships model)
        {
            if (model == null) return null;
            return new TwinGeometryViewerIdDto
            {
                TwinId = model.Id,
                GeometryViewerId = model.GeometryViewerId,
                UniqueId = model.UniqueId
            };
        }
    }
}
