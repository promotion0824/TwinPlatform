using System;
using System.Collections.Generic;
using System.Linq;
using MobileXL.Models;

namespace MobileXL.Dto
{
    public class AssetCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public List<AssetCategoryDto> ChildCategories { get; set; }

        public int AssetCount { get; set; }

        public bool HasChildren { get; set; }

        public static AssetCategoryDto MapFromModel(AssetCategory category)
        {
            if (category == null)
            {
                return null;
            }

            return new AssetCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                AssetCount = (category.Assets?.Count()).GetValueOrDefault(),
                ChildCategories = MapFromModels(category.Categories),
                HasChildren = ((category.Assets != null && category.Assets.Any()) || (category.Categories != null && category.Categories.Any()))
            };
        }

        public static List<AssetCategoryDto> MapFromModels(IEnumerable<AssetCategory> categories)
        {
            return categories?.Select(MapFromModel).ToList();
        }
    }
}
