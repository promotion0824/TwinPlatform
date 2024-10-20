using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwinCore.Models
{
    [Serializable]
    public class Property
    {
        public string DisplayName { get; set; }
        public object Value { get; set; }
        public PropertyKind Kind { get; set; }
    }

    public enum PropertyKind
    {
        Other,
        Property,
        Relationship,
        Component
    }
}
