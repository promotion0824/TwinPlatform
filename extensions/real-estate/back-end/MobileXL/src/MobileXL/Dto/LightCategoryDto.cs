using MobileXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobileXL.Dto
{
    public class LightCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ModelId { get; set; }
        public IEnumerable<LightCategoryDto> ChildCategories { get; set; }
        public long AssetCount { get; set; }
        public bool HasChildren { get; set; }

        public static LightCategoryDto MapFromModel(AssetCategory category)
        {
            if (category == null)
            {
                return null;
            }

            return new LightCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                AssetCount = (category.Assets?.Count()).GetValueOrDefault(),
                ChildCategories = MapFromModels(category.Categories),
                HasChildren = ((category.Assets != null && category.Assets.Any()) || (category.Categories != null && category.Categories.Any()))
            };
        }

        public static List<LightCategoryDto> MapFromModels(IEnumerable<AssetCategory> categories)
        {
            return categories?.Select(MapFromModel).ToList();
        }
    }
}
