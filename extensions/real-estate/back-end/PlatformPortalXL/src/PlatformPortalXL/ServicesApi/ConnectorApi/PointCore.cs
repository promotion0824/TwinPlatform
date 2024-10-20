using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.ServicesApi.ConnectorApi
{
    public class PointCore
    {
        public Guid Id { get; set; }

        public string TwinId { get; set; }

        public Guid EntityId { get; set; }

        public string Name { get; set; }

        public Guid ClientId { get; set; }

        public Guid SiteId { get; set; }

        public string Unit { get; set; }

        public PointType Type { get; set; }

        public string ExternalPointId { get; set; }

        public List<EquipmentCore> Equipment { get; set; } = new List<EquipmentCore>();

        public IList<Tag> Tags { get; set; }

        public Guid DeviceId { get; set; }


        public static Point MapToModel(PointCore pointCore)
        {
            return new Point
            {
                Name = pointCore.Name,
                Id = pointCore.Id,
                TwinId = pointCore.TwinId,
                CustomerId = pointCore.ClientId,
                SiteId = pointCore.SiteId,
                EntityId = pointCore.EntityId,
                ExternalPointId = pointCore.ExternalPointId,
                Type = pointCore.Type,
                Unit = pointCore.Unit,
                EquipmentId = pointCore.Equipment?.FirstOrDefault()?.Id ?? Guid.Empty,
                Tags = pointCore.Tags,
                Equipment = EquipmentCore.MapToModels(pointCore.Equipment),
                DeviceId = pointCore.DeviceId
            };
        }

        public static List<Point> MapToModels(IEnumerable<PointCore> connectorPoints)
        {
            return connectorPoints?.Select(MapToModel).ToList();
        }
    }
}
