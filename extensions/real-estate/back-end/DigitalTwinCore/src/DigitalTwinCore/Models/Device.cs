using System;
using System.Collections.Generic;

namespace DigitalTwinCore.Models
{
    public class Device
    {
        public string ModelId { get; set; }
        public string TwinId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string RegistrationId { get; set; }
        public string RegistrationKey { get; set; }
        public string ExternalId { get; set; }
        public bool? IsEnabled { get; set; } 
        public bool? IsDetected { get; set; }
        public List<Point> Points { get; set; }
        public Dictionary<string, Property> Properties { get; set; }
        public Guid? ConnectorId { get; set; }
    }
}
