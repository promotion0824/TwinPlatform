using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Dto;
using PlatformPortalXL.ServicesApi.SiteApi;

namespace PlatformPortalXL.Models
{
    public class LayerGroupList
    {
        public List<LayerGroup> LayerGroups { get; set; }

        public List<LayerGroupModule> Modules2D { get; set; }

        public List<LayerGroupModule> Modules3D { get; set; }

        public Guid FloorId { get; set; }

        public string FloorName { get; set; }
    }

    public class LayerGroupModule
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid VisualId { get; set; }

        public string Url { get; set; }

        public int SortOrder { get; set; }

        public bool CanBeDeleted { get; set; }

        public string TypeName { get; set; }

        public string GroupType { get; set; }

        public Guid ModuleTypeId { get; set; }

        public ModuleGroupDto ModuleGroup { get; set; }

        public bool IsDefault { get; set; }
    }

    public class LayerGroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Is3D { get; set; }

        public List<LayerGroupZone> Zones { get; set; } = new List<LayerGroupZone>();

        public List<LayerGroupLayer> Layers { get; set; } = new List<LayerGroupLayer>();

        public List<LayerGroupEquipment> Equipments { get; set; } = new List<LayerGroupEquipment>();
    }

    public class LayerGroupZone
    {
        public Guid Id { get; set; }
        public List<List<int>> Geometry { get; set; } = new List<List<int>>();
        public List<Guid> EquipmentIds { get; set; } = new List<Guid>();

        public static LayerGroupZone MapFrom(LayerGroupZoneCore layerGroupZoneCore)
        {
            if (layerGroupZoneCore == null)
            {
                return null;
            }

            return new LayerGroupZone
            {
                Id = layerGroupZoneCore.Id,
                Geometry = layerGroupZoneCore.Geometry,
                EquipmentIds = layerGroupZoneCore.EquipmentIds.Select(g => g).ToList()
            };
        }

        public static List<LayerGroupZone> MapFrom(IEnumerable<LayerGroupZoneCore> zones)
        {
            return zones?.Select(MapFrom).ToList();
        }
    }

    public class LayerGroupLayer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TagName { get; set; }

        public static LayerGroupLayer MapFrom(LayerGroupLayerCore layerGroupLayerCore)
        {
            if (layerGroupLayerCore == null)
            {
                return null;
            }

            return new LayerGroupLayer
            {
                Id = layerGroupLayerCore.Id,
                Name = layerGroupLayerCore.Name,
                TagName = layerGroupLayerCore.TagName,
            };
        }

        public static List<LayerGroupLayer> MapFrom(IEnumerable<LayerGroupLayerCore> layers)
        {
            return layers?.Select(MapFrom).ToList();
        }

        
    }

    public class LayerGroupEquipment
    {
        public Guid Id { get; set; }

        public List<List<int>> Geometry { get; set; } = new List<List<int>>();

        public string Name { get; set; }

        public bool HasLiveData { get; set; }

        public Guid ZoneId { get; set; }

        public IList<Tag> Tags { get; set; } = new List<Tag>();

        public IList<Tag> PointTags { get; set; } = new List<Tag>();

        public static List<LayerGroupEquipment> MapFrom(IEnumerable<LayerGroupEquipmentCore> layerEquipments)
        {
            return layerEquipments?.Select(MapFrom).ToList();
        }

        public static LayerGroupEquipment MapFrom(LayerGroupEquipmentCore layerLayerGroupEquipmentCore)
        {
            if (layerLayerGroupEquipmentCore == null)
            {
                return null;
            }

            return new LayerGroupEquipment
            {
                Id = layerLayerGroupEquipmentCore.Id,
                Geometry = layerLayerGroupEquipmentCore.Geometry, //.Select(list => list.Select(item => item).ToList()).ToList(),
                ZoneId = layerLayerGroupEquipmentCore.ZoneId
            };
        }
    }
}
