using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Requests.SiteCore
{
    public class UpdateLayerGroupRequest
    {
        public string Name { get; set; }

        public bool Is3D { get; set; }

        public List<UpdateEquipmentRequest> Equipments { get; set; } = new List<UpdateEquipmentRequest>();

        public List<UpdateZoneRequest> Zones { get; set; } = new List<UpdateZoneRequest>();

        public List<UpdateLayerRequest> Layers { get; set; } = new List<UpdateLayerRequest>();
    }

    public class UpdateEquipmentRequest
    {
        public Guid Id { get; set; }

        public List<List<int>> Geometry { get; set; } = new List<List<int>>();

        public string Name { get; set; }

        public List<string> Tags { get; set; }
    }

    public class UpdateZoneRequest
    {
        public Guid? Id { get; set; }

        public bool Is3D { get; set; }

        public List<List<int>> Geometry { get; set; } = new List<List<int>>();

        public List<Guid> EquipmentIds { get; set; } = new List<Guid>();
    }

    public class UpdateLayerRequest
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string TagName { get; set; }
    }
}
