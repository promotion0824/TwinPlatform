using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Dto;

namespace PlatformPortalXL.ServicesApi.SiteApi
{
    public class LayerGroupListCore
    {
        public List<LayerGroupCore> LayerGroups { get; set; }

        public List<LayerGroupModuleCore> Modules2D { get; set; }

        public List<LayerGroupModuleCore> Modules3D { get; set; }

        public Guid FloorId { get; set; }

        public string FloorName { get; set; }

        public static LayerGroupList MapToModel(LayerGroupListCore coreList, IImageUrlHelper urlHelper, ILogger<FloorManagementService> _logger = null)
        {
            if (coreList == null)
            {
                return null;
            }

            var layerGroupList = new LayerGroupList
            {
                LayerGroups = LayerGroupCore.MapToModels(coreList.LayerGroups, _logger),
                Modules2D = LayerGroupModuleCore.MapToModels(coreList.Modules2D, urlHelper, false),
                Modules3D = LayerGroupModuleCore.MapToModels(coreList.Modules3D, urlHelper, true),
                FloorName = coreList.FloorName,
                FloorId = coreList.FloorId
            };

            return layerGroupList;
        }
    }

    public class LayerGroupModuleCore
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid VisualId { get; set; }

        public string Path { get; set; }

        public int SortOrder { get; set; }

        public bool CanBeDeleted { get; set; }

        public string TypeName { get; set; }

        public string GroupType { get; set; }

        public Guid ModuleTypeId { get; set; }

        public ModuleGroupDto ModuleGroup { get; set; }

        public bool IsDefault { get; set; }

        public static LayerGroupModule MapToModel(LayerGroupModuleCore coreModule, IImageUrlHelper urlHelper, bool is3D)
        {
            if (coreModule == null)
            {
                return null;
            }

            return new LayerGroupModule
            {
                Name = coreModule.Name,
                Id = coreModule.Id,
                CanBeDeleted = coreModule.CanBeDeleted,
                GroupType = coreModule.GroupType,
                VisualId = coreModule.VisualId,
                SortOrder = coreModule.SortOrder,
                Url = is3D ? coreModule.Path : urlHelper.GetModuleUrl(coreModule.Path, coreModule.VisualId),
                TypeName = coreModule.TypeName,
                ModuleTypeId = coreModule.ModuleTypeId,
                ModuleGroup = coreModule.ModuleGroup,
                IsDefault = coreModule.IsDefault
            };
        }

        public static List<LayerGroupModule> MapToModels(IEnumerable<LayerGroupModuleCore> coreModules, IImageUrlHelper urlHelper, bool is3D)
        {
            return coreModules?.Select(m => MapToModel(m, urlHelper, is3D)).ToList();
        }
    }

    public class LayerGroupCore
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool Is3D { get; set; }

        public List<LayerGroupZoneCore> Zones { get; set; } = new List<LayerGroupZoneCore>();

        public List<LayerGroupLayerCore> Layers { get; set; } = new List<LayerGroupLayerCore>();

        public List<LayerGroupEquipmentCore> Equipments { get; set; } = new List<LayerGroupEquipmentCore>();

        public static LayerGroup MapToModel(LayerGroupCore coreGroup)
        {
            if (coreGroup == null)
            {
                return null;
            }

            var layerGroup = new LayerGroup
            {
                Id = coreGroup.Id,
                Name = coreGroup.Name,
                Is3D = coreGroup.Is3D,
                Zones = LayerGroupZone.MapFrom(coreGroup.Zones),
                Layers = LayerGroupLayer.MapFrom(coreGroup.Layers),
                Equipments = LayerGroupEquipment.MapFrom(coreGroup.Equipments)
            };

            return layerGroup;
        }

        public static List<LayerGroup> MapToModels(IEnumerable<LayerGroupCore> coreGroups)
        {
            return coreGroups?.Select(MapToModel).ToList();
        }

        public static List<LayerGroup> MapToModels(IEnumerable<LayerGroupCore> coreList, ILogger<FloorManagementService> _logger = null)
        {
            if (coreList == null)
            {
                return null;
            }

            if (!coreList.Any())
            {
                return [];
            }

            return MapToModels(coreList);
        }
    }

    public class LayerGroupZoneCore
    {
        public Guid Id { get; set; }
        public bool Is3D { get; set; }
        public List<List<int>> Geometry { get; set; } = new List<List<int>>();
        public List<Guid> EquipmentIds { get; set; } = new List<Guid>();
    }

    public class LayerGroupLayerCore
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TagName { get; set; }
    }

    public class LayerGroupEquipmentCore
    {
        public Guid Id { get; set; }

        public List<List<int>> Geometry { get; set; } = new List<List<int>>();

        public Guid ZoneId { get; set; }
    }
}
