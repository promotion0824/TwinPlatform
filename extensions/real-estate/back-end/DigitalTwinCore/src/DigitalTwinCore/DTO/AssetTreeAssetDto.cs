using DigitalTwinCore.Models;
using System;
using System.Collections.Generic;

namespace DigitalTwinCore.Dto
{
    public class AssetTreeAssetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TwinId { get; set; }
        public Guid? FloorId { get; set; }
        public string Identifier { get; set; }
        public string ForgeViewerModelId { get; set; }
        public string ModuleTypeNamePath { get; set; }
        public List<TagDto> Tags { get; set; } = new List<TagDto>();
        public List<TagDto> PointTags { get; set; } = new List<TagDto>();
        public bool HasLiveData { get; set; }
        public Dictionary<string, Property> Properties { get; set; }
    }
}
