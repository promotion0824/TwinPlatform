using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;

namespace PlatformPortalXL.ServicesApi.DigitalTwinApi
{
    public class DigitalTwinDevice
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
        public List<DigitalTwinPoint> Points { get; set; }
        public Dictionary<string, DigitalTwinProperty> Properties { get; set; }
        public Guid? ConnectorId { get; set; }

        public Device MapToModel()
        {
            return new Device
            {
                Id = Id,
                Name = Name,
                ConnectorId = ConnectorId.GetValueOrDefault(),
                IsEnabled = IsEnabled.GetValueOrDefault(),
                IsDetected = IsDetected.GetValueOrDefault()
            };
        }

        public DeviceDto MapToDto()
        {
            return new DeviceDto
            {
                ModelId = ModelId,
                TwinId = TwinId,
                Id = Id,
                Name = Name,
                RegistrationId = RegistrationId,
                RegistrationKey = RegistrationKey,
                ExternalId = ExternalId,
                IsEnabled = IsEnabled,
                IsDetected = IsDetected,
                Properties = Properties,
                ConnectorId = ConnectorId
            };
        }
    }
}
