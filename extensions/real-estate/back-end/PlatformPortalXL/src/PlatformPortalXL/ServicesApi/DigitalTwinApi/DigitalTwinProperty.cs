using System;

namespace PlatformPortalXL.ServicesApi.DigitalTwinApi
{
    [Serializable]
    public class DigitalTwinProperty
    {
        public string DisplayName { get; set; }
        public object Value { get; set; }
        public DigitalTwinPropertyKind Kind { get; set; }
    }

    public enum DigitalTwinPropertyKind
    {
        Other,
        Property,
        Relationship,
        Component
    }
}
