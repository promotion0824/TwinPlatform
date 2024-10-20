using DigitalTwinCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Dto
{
    public class AssetTreeCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<AssetTreeCategoryDto> Categories { get; set; }
        public List<AssetTreeAssetDto> Assets { get; set; }

        public static AssetTreeCategoryDto Map(Category category)
        {
            return new AssetTreeCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Categories = Map(category.Categories),
                Assets = category.Assets.Select(a => new AssetTreeAssetDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    TwinId = a.TwinId,
                    FloorId = a.FloorId,
                    Identifier = a.Identifier,
                    ForgeViewerModelId = a.ForgeViewerModelId,
                    Tags = TagDto.MapFrom(a.Tags),
                    PointTags = TagDto.MapFrom(a.PointTags),
                    HasLiveData = a.Points.Any(),
                    ModuleTypeNamePath = a.ModuleTypeNamePath,
                    Properties = new Dictionary<string, Property>(a.Properties)
                }).ToList()
            };
        }

        public static List<AssetTreeCategoryDto> Map(List<Category> categories)
        {
            return categories.Select(Map).ToList();
        }
    }
}
