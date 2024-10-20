using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Dto
{
    public class LayerGroupListDto
    {
        // Remove LayerGroups property when LayerGroups2D and LayerGroups3D are implemented in the Web
        public List<LayerGroupDto> LayerGroups { get; set; } 

        public List<LayerGroupDto> LayerGroups2D { get; set; }

        public List<LayerGroupDto> LayerGroups3D { get; set; }

        public List<LayerGroupModuleDto> Modules2D { get; set; }

        public List<LayerGroupModuleDto> Modules3D { get; set; }

        public Guid FloorId { get; set; }

        public string FloorName { get; set; }

        public static LayerGroupListDto MapFrom(LayerGroupList groupList)
        {
            if (groupList == null)
            {
                return null;
            }

            var layerGroupDtos = LayerGroupDto.MapFrom(groupList.LayerGroups);

            return new LayerGroupListDto
            {
                LayerGroups = layerGroupDtos,
                LayerGroups2D = layerGroupDtos.Where(x => !x.Is3D).ToList(),
                LayerGroups3D = layerGroupDtos.Where(x => x.Is3D).ToList(),
                Modules2D = LayerGroupModuleDto.MapFrom(groupList.Modules2D),
                Modules3D = LayerGroupModuleDto.MapFrom(groupList.Modules3D),
                FloorName = groupList.FloorName,
                FloorId = groupList.FloorId
            };
        }
    }

    public class LayerGroupModuleDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid VisualId { get; set; }

        public string Url { get; set; }

        public int SortOrder { get; set; }

        public bool CanBeDeleted { get; set; }

        public bool IsDefault { get; set; }

        public string TypeName { get; set; }

        public string GroupType { get; set; }

        public Guid ModuleTypeId { get; set; }

        public ModuleGroupDto ModuleGroup { get; set; }

        public static LayerGroupModuleDto MapFrom(LayerGroupModule module)
        {
            if (module == null)
            {
                return null;
            }

            return new LayerGroupModuleDto
            {
                Name = module.Name,
                SortOrder = module.SortOrder,
                Id = module.Id,
                VisualId = module.VisualId,
                CanBeDeleted = module.CanBeDeleted,
                IsDefault = module.IsDefault,
                Url = module.Url,
                GroupType = module.GroupType,
                TypeName = module.TypeName,
                ModuleTypeId = module.ModuleTypeId,
                ModuleGroup = module.ModuleGroup
            };
        }

        public static List<LayerGroupModuleDto> MapFrom(IEnumerable<LayerGroupModule> xlModels)
        {
            return xlModels?.Select(MapFrom).ToList();
        }
    }


    public class LayerGroupDto
    {
        public Guid Id { get; set; }

        public string Name => Id.ToString();

        public bool Is3D { get; set; }

        public List<LayerGroupZoneDto> Zones { get; set; } = new List<LayerGroupZoneDto>();

        public List<LayerGroupLayerDto> Layers { get; set; } = new List<LayerGroupLayerDto>();

        public List<LayerGroupEquipmentDto> Equipments { get; set; } = new List<LayerGroupEquipmentDto>();

        public static LayerGroupDto MapFrom(LayerGroup layerGroup)
        {
            if (layerGroup == null)
            {
                return null;
            }

            var layerDto = new LayerGroupDto
            {
                Id = layerGroup.Id,
                Is3D = layerGroup.Is3D,
                Zones = LayerGroupZoneDto.MapFrom(layerGroup.Zones),
                Layers = LayerGroupLayerDto.MapFrom(layerGroup.Layers),
                Equipments = LayerGroupEquipmentDto.MapFrom(layerGroup.Equipments)
            };

            return layerDto;
        }

        public static List<LayerGroupDto> MapFrom(IEnumerable<LayerGroup> zoneGroups)
        {
            return zoneGroups?.Select(MapFrom).ToList();
        }
    }

    public class LayerGroupEquipmentDto
    {
        public Guid Id { get; set; }

        public List<List<int>> Geometry { get; set; } = new List<List<int>>();

        public string Name { get; set; }

        public bool HasLiveData { get; set; }

        public IList<TagDto> Tags { get; set; } = new List<TagDto>();

        public IList<TagDto> PointTags { get; set; } = new List<TagDto>();

        public static List<LayerGroupEquipmentDto> MapFrom(IEnumerable<LayerGroupEquipment> layerEquipments)
        {
            return layerEquipments?.Select(MapFrom).ToList();
        }

        public static LayerGroupEquipmentDto MapFrom(LayerGroupEquipment layerLayerGroupEquipmentSiteCore)
        {
            if (layerLayerGroupEquipmentSiteCore == null)
            {
                return null;
            }

            return new LayerGroupEquipmentDto
            {
                Id = layerLayerGroupEquipmentSiteCore.Id,
                Geometry = layerLayerGroupEquipmentSiteCore.Geometry.Select(list => list.Select(item => item).ToList()).ToList(),
                Name = layerLayerGroupEquipmentSiteCore.Name,
                HasLiveData = layerLayerGroupEquipmentSiteCore.HasLiveData,
                Tags = TagDto.MapFrom(layerLayerGroupEquipmentSiteCore.Tags),
                PointTags = TagDto.MapFrom(layerLayerGroupEquipmentSiteCore.PointTags)
            };
        }
    }

    public class LayerGroupLayerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TagName { get; set; }

        public static LayerGroupLayerDto MapFrom(LayerGroupLayer layerGroupLayerSiteCore)
        {
            if (layerGroupLayerSiteCore == null)
            {
                return null;
            }

            return new LayerGroupLayerDto
            {
                Id = layerGroupLayerSiteCore.Id,
                Name = layerGroupLayerSiteCore.Name,
                TagName = layerGroupLayerSiteCore.TagName,
            };
        }

        public static List<LayerGroupLayerDto> MapFrom(IEnumerable<LayerGroupLayer> layers)
        {
            return layers?.Select(MapFrom).ToList();
        }

        
    }

    public class LayerGroupZoneDto
    {
        public Guid Id { get; set; }
        public List<List<int>> Geometry { get; set; }
        public List<Guid> EquipmentIds { get; set; } = new List<Guid>();

        public static LayerGroupZoneDto MapFrom(LayerGroupZone layerGroupZoneSiteCore)
        {
            if (layerGroupZoneSiteCore == null)
            {
                return null;
            }

            return new LayerGroupZoneDto
            {
                Id = layerGroupZoneSiteCore.Id,
                Geometry = layerGroupZoneSiteCore.Geometry,
                EquipmentIds = layerGroupZoneSiteCore.EquipmentIds.Select(g => g).ToList()
            };
        }

        public static List<LayerGroupZoneDto> MapFrom(IEnumerable<LayerGroupZone> zones)
        {
            return zones?.Select(MapFrom).ToList();
        }
    }
}
