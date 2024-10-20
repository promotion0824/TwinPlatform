using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.Dto
{
    public class DeviceDto
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
        public Dictionary<string, DigitalTwinProperty> Properties { get; set; }
        public Guid? ConnectorId { get; set; }
        public string ConnectorName { get; set; }
    }
}
