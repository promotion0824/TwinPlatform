using DigitalTwinCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Dto
{
    [Serializable]
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
        public List<PointDto> Points { get; set; }
        public Dictionary<string, Property> Properties { get; set; }
        public Guid? ConnectorId { get; set; }

        internal static DeviceDto MapFrom(Device model)
        {
            if (model == null)
            {
                return null;
            }

            return new DeviceDto
            {
                ModelId = model.ModelId,
                TwinId = model.TwinId,
                Id = model.Id,
                Name = model.Name,
                RegistrationId = model.RegistrationId,
                RegistrationKey = model.RegistrationKey,
                ExternalId = model.ExternalId,
                IsEnabled = model.IsEnabled,
                IsDetected = model.IsDetected,
                Points = PointDto.MapFrom(model.Points, true, true),
                Properties = model.Properties,
                ConnectorId = model.ConnectorId
            };
        }

        internal static List<DeviceDto> MapFrom(List<Device> models)
        {
            return models?.Select(MapFrom).ToList() ?? new List<DeviceDto>();
        }
    }
}
