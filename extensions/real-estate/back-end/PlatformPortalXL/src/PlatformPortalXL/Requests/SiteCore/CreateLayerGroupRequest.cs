using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Requests.SiteCore
{
    public class CreateLayerGroupRequest
    {
        public string Name { get; set; }

        public bool Is3D { get; set; }

        public List<CreateEquipmentRequest> Equipments { get; set; } = new List<CreateEquipmentRequest>();

        public List<CreateZoneRequest> Zones { get; set; } = new List<CreateZoneRequest>();

        public List<CreateLayerRequest> Layers { get; set; } = new List<CreateLayerRequest>();
    }

    public class CreateEquipmentRequest
    {
        public Guid Id { get; set; }

        public List<List<int>> Geometry { get; set; } = new List<List<int>>();

        public string Name { get; set; }

        public List<string> Tags { get; set; }
    }

    public class CreateZoneRequest
    {
        public bool Is3D { get; set; }

        public List<List<int>> Geometry { get; set; } = new List<List<int>>();

        public List<Guid> EquipmentIds { get; set; } = new List<Guid>();
    }

    public class CreateLayerRequest
    {
        public string Name { get; set; }

        public string TagName { get; set; }
    }
}
