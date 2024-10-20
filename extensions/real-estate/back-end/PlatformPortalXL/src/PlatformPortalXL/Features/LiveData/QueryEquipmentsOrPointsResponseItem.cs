using System;

namespace PlatformPortalXL.Features.LiveData
{
    public class QueryEquipmentsOrPointsResponseItem
    {
        public Guid? EquipmentId { get; set; }
        public Guid? PointId { get; set; }
        public Guid? PointEntityId { get; set; }
        public Guid SiteId { get; set; }
    }
}
