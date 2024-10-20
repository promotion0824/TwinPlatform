using System;
using System.Collections.Generic;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Dto
{
    public class EquipmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SiteId { get; set; }
        public string ExternalEquipmentId { get; set; }
        public Guid? ParentEquipmentId { get; set; }
        public List<PointSimpleDto> Points { get; set; }
        public List<TagDto> Tags { get; set; }
        public List<TagDto> PointTags { get; set; }

        public static EquipmentDto MapFrom(Equipment model, bool useDigitalTwinAsset = false)
        {
            if (model == null)
            {
                return null;
            }

            return new EquipmentDto
            {
                Id = model.Id,
                Name = model.Name,
                CustomerId = model.CustomerId,
                ExternalEquipmentId = model.ExternalEquipmentId,
                ParentEquipmentId = model.ParentEquipmentId,
                SiteId = model.SiteId,
                Points = PointSimpleDto.MapFrom(model.Points, useDigitalTwinAsset),
                Tags = TagDto.MapFrom(model.Tags),
                PointTags = TagDto.MapFrom(model.PointTags)
            };
        }
    }
}
