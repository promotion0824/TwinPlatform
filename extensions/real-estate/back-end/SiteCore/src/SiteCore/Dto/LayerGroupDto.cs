using SiteCore.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteCore.Dto
{
    public class LayerGroupListDto
    {
        public List<LayerGroupDto> LayerGroups { get; set; }

        public List<LayerGroupModuleDto> Modules2D { get; set; }

        public List<LayerGroupModuleDto> Modules3D { get; set; }

        public Guid FloorId { get; set; }

        public string FloorName { get; set; }

        public static LayerGroupListDto MapFrom(
            IEnumerable<LayerGroup> groups, 
            IEnumerable<Module> modules, Floor floor)
        {
            return new LayerGroupListDto
            {
                Modules2D = LayerGroupModuleDto.MapFrom(modules.Where(m => !m.ModuleType.Is3D)),
                Modules3D = LayerGroupModuleDto.MapFrom(modules.Where(m => m.ModuleType.Is3D)),
                LayerGroups = LayerGroupDto.MapFrom(groups),
                FloorId = floor.Id,
                FloorName = floor.Name
            };
        }
    }

    public class LayerGroupModuleDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid VisualId { get; set; }

        public string Path { get; set; }

        public int SortOrder { get; set; }

        public bool CanBeDeleted { get; set; }

        public string TypeName { get; set; }

        public Guid ModuleTypeId { get; set; }

        public string GroupType { get; set; }

        public bool IsDefault { get; set; }

        public ModuleGroupDto ModuleGroup { get; set; }

        public static List<LayerGroupModuleDto> MapFrom(IEnumerable<Module> modules)
        {
            return modules?.Select(MapFrom).ToList() ?? new List<LayerGroupModuleDto>();
        }

        public static LayerGroupModuleDto MapFrom(Module module)
        {
            if (module == null)
            {
                return null;
            }

            return new LayerGroupModuleDto
            {
                Id = module.Id,
                Name = module.Name,
                VisualId = module.VisualId,
                SortOrder = module.ModuleType?.SortOrder ?? 0,
                CanBeDeleted = module.ModuleType?.CanBeDeleted ?? false,
                GroupType = module.ModuleType?.Group?.Name ?? string.Empty,
                Path = module.Path,
                TypeName = module.ModuleType?.Name,
                IsDefault = module.ModuleType?.IsDefault ?? false,
                ModuleTypeId = module.ModuleTypeId,
                ModuleGroup = module.ModuleType != null && module.ModuleType.Group != null ? new ModuleGroupDto
                {
                    Id = module.ModuleType.Group.Id,
                    SiteId = module.ModuleType.Group.SiteId,
                    SortOrder = module.ModuleType.Group.SortOrder,
                    Name = module.ModuleType.Group.Name,
                } : null
            };
        }
    }

    public class LayerGroupDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool Is3D { get; set; }

        public List<ZoneDto> Zones { get; set; } = new List<ZoneDto>();

        public List<LayerDto> Layers { get; set; } = new List<LayerDto>();

        public List<EquipmentDto> Equipments { get; set; } = new List<EquipmentDto>();

        public static LayerGroupDto MapFrom(LayerGroup layerGroup)
        {
            if (layerGroup == null)
            {
                return null;
            }

            var layerDto = new LayerGroupDto
            {
                Id = layerGroup.Id,
                Name = layerGroup.Name,
                Is3D = layerGroup.Is3D,
                Zones = layerGroup.Zones.Select(ZoneDto.MapFrom).ToList(),
                Layers = layerGroup.Layers.Select(LayerDto.MapFrom).ToList(),
                Equipments = layerGroup.Equipments.Select(EquipmentDto.MapFrom).ToList()
            };

            foreach (var layerDtoZone in layerDto.Zones)
            {
                layerDtoZone.EquipmentIds = layerGroup.Equipments
                    .Where(eq => eq.ZoneId == layerDtoZone.Id)
                    .Select(eq => eq.EquipmentId).ToList();
            }

            return layerDto;
        }

        public static List<LayerGroupDto> MapFrom(IEnumerable<LayerGroup> zoneGroups)
        {
            return zoneGroups?.Select(MapFrom).ToList();
        }
    }
}
