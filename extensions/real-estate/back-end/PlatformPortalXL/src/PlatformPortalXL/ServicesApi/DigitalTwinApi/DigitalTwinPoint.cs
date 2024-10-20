using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.ServicesApi.DigitalTwinApi
{
    public class DigitalTwinPoint
    {
        public string ModelId { get; set; }
        public string TwinId { get; set; }
        public Guid Id { get; set; }
        public Guid TrendId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Tag> Tags { get; set; }
        public PointType Type { get; set; }

        public string Unit
        {
            get
            {
                if (Properties != null && Properties.TryGetValue("unit", out var unitPointValue))
                {
                    return unitPointValue.Value.ToString();
                }

                return null;
            }
            set
            {
                Properties ??= new Dictionary<string, DigitalTwinProperty>();

                Properties["unit"] = new DigitalTwinProperty
                {
                    DisplayName = "FrequencyUnit",
                    Kind = DigitalTwinPropertyKind.Property,
                    Value = value
                };
            }
        }

        public decimal? DisplayPriority { get; set; }
        public string DisplayName { get; set; }
        public List<PointAssetDto> Assets { get; set; }
        public Dictionary<string, DigitalTwinProperty> Properties { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? IsDetected { get; set; }

        public Guid? DeviceId { get; set; }
        public string TrendInterval { get; set; }
        public string CategoryName { get; set; }

        public Point MapToModel(Guid siteId)
        {
            Properties.TryGetValue("unit", out var unitProperty);
            Properties.TryGetValue("type", out var type);
            return new Point
            {
                Id = Id,
                TwinId = TwinId,
                SiteId = siteId,
                DeviceId = DeviceId.GetValueOrDefault(),
                EntityId = TrendId,
                EquipmentId = (Assets?.FirstOrDefault()?.Id).GetValueOrDefault(),
                Equipment = Assets?.Select(a => new Equipment { Id = a.Id, Name = a.Name }).ToList(),
                ExternalPointId = ExternalId,
                Name = Name,
                Tags = Tags?.Select(t => new Tag { Name = t.Name, Feature = t.Feature }).ToList(),
                Type = Enum.TryParse<PointType>(type?.Value.ToString(), true, out var pointType) ? pointType : PointType.Undefined,
                Unit = unitProperty?.Value.ToString(),
                DisplayPriority = DisplayPriority,
                DisplayName = DisplayName,
                Properties = Properties
            };
        }

        public Point MapToModel()
        {
            if (Properties.TryGetValue("siteID", out var siteIdProperty) &&
                Guid.TryParse(siteIdProperty?.Value?.ToString(), out var siteId))
            {
                return MapToModel(siteId);
            }

            throw new InvalidOperationException("Cannot parse siteId from properties.");
        }

        public class PointValue
        {
            public string Unit { get; set; }
            public object Value { get; set; }
        }

        public class PointAssetDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }
    }
}
