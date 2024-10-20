using DigitalTwinCore.Models;
using DTDLParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Dto
{
    [Serializable]
    public class PointAssetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }

        internal static PointAssetDto MapFrom(Twin twin) =>
            (twin == null) ? null : new PointAssetDto
            {
                Id = twin.UniqueId,
                Name = twin.DisplayName,
                CategoryName = (new Dtmi(twin.Metadata.ModelId)).Labels.Last()
            };

        internal static List<PointAssetDto> MapFrom(IEnumerable<Twin> twins) =>
            twins?.Select(MapFrom).ToList() ?? new List<PointAssetDto>();
    }
}
