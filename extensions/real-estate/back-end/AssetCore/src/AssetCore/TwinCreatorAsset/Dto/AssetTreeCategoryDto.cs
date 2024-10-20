using AssetCoreTwinCreator.Dto;
using AssetCoreTwinCreator.MappingId.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetCore.TwinCreatorAsset.Dto
{
    public class AssetTreeCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ModuleTypeNamePath { get; set; }
        public List<AssetTreeCategoryDto> Categories { get; set; }
        public List<AssetTreeAssetDto> Assets { get; set; }

        public static List<AssetTreeCategoryDto> Map(List<CategoryDto> categoryDtos, List<AssetSimpleDto> assetDtos, Dictionary<int, Guid> assetEquipmentMappings)
        {
            return categoryDtos.Select(c => new AssetTreeCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ModuleTypeNamePath = c.ModuleTypeNamePath,
                Categories = AssetTreeCategoryDto.Map(c.ChildCategories, assetDtos, assetEquipmentMappings),
                Assets = assetDtos.Where(a => a.CategoryId == c.Id).Select(a => new AssetTreeAssetDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    EquipmentId = assetEquipmentMappings.ContainsKey(a.Id.ToAssetId()) ? assetEquipmentMappings[a.Id.ToAssetId()] : default(Guid?),
                    FloorId = a.FloorId,
                    Identifier = a.Identifier,
                    ForgeViewerModelId = a.ForgeViewerModelId
                }).ToList()
            }).ToList();
        }
    }
}
