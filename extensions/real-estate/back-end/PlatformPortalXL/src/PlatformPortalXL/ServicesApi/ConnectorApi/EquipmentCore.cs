using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.ServicesApi.ConnectorApi
{
    public class EquipmentCore
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid ClientId { get; set; }

        public Guid SiteId { get; set; }

        public Guid FloorId { get; set; }

        public string ExternalEquipmentId { get; set; }

        public string Category { get; set; }

        public Guid? ParentEquipmentId { get; set; }

        public List<PointCore> Points { get; set; } = new List<PointCore>();

        public List<Tag> Tags { get; set; } = new List<Tag>();

        public List<Tag> PointTags { get; set; } = new List<Tag>();

        public static Equipment MapToModel(EquipmentCore equipmentCore)
        {
            var equipment = new Equipment
            {
                Id = equipmentCore.Id,
                Name = equipmentCore.Name,
                CustomerId = equipmentCore.ClientId,
                SiteId = equipmentCore.SiteId,
                FloorId = equipmentCore.FloorId,
                Category = equipmentCore.Category,
                ExternalEquipmentId = equipmentCore.ExternalEquipmentId,
                ParentEquipmentId = equipmentCore.ParentEquipmentId,
                Points = PointCore.MapToModels(equipmentCore.Points),
                Tags = equipmentCore.Tags.ToList(),
                PointTags = equipmentCore.PointTags.ToList()
            };

            return equipment;
        }

        public static List<Equipment> MapToModels(IList<EquipmentCore> equipmentCores)
        {
            return equipmentCores.Select(MapToModel).ToList();
        }
    }
}
