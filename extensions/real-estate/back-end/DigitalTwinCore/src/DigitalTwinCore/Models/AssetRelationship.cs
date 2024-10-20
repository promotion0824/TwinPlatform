using System;

namespace DigitalTwinCore.Models
{
    public class AssetRelationship
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public Guid TargetId { get; set; }
        public string TargetName { get; set; }
        public string TargetType { get; set; }
    }
}
