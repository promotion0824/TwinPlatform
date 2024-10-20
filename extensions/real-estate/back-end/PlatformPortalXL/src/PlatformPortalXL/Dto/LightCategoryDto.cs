using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

using Willow.Platform.Localization;

namespace PlatformPortalXL.Dto
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
                Id              = category.Id,
                Name            = category.Name,
                ModelId         = category.ModuleTypeNamePath,
                AssetCount      = (category.Assets?.Count()).GetValueOrDefault(),
                ChildCategories = MapFromModels(category.Categories),
                HasChildren     = ((category.Assets != null && category.Assets.Any()) || (category.Categories != null && category.Categories.Any()))
            };
        }

        public static List<LightCategoryDto> MapFromModels(IEnumerable<AssetCategory> categories)
        {
            return categories?.Select(MapFromModel).ToList();
        }
    }

    public static class AssetLocalizerExtensions
    {
        public static void Localize(this IAssetLocalizer localizer, IEnumerable<LightCategoryDto> categories)
        {
            foreach(var category in categories)
            {
                category.Name = localizer.TranslateAssetName(category.ModelId, category.Name);

                if(category.ChildCategories != null)
                {
                    localizer.Localize(category.ChildCategories);
                }
            }
        }
    }
}
