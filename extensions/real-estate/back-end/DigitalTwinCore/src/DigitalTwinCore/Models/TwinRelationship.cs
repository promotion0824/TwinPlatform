using System;
using System.Collections.Generic;

namespace DigitalTwinCore.Models
{

    [Serializable]
    public class TwinRelationship
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TwinWithRelationships Target { get; set; }
        public TwinWithRelationships Source { get; set; }
        public IDictionary<string, object> CustomProperties { get; set; }
    }
}
