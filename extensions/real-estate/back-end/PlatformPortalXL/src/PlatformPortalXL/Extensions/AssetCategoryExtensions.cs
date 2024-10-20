using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PlatformPortalXL.Models;

namespace PlatformPortalXL
{
    public static class AssetCategoryExtensions
    {
        public static IEnumerable<Asset> GetAssets(this IEnumerable<AssetCategory> categories, Guid? categoryId, bool subCategories)
        {
            var assets = new List<Asset>();

            if(categoryId.HasValue)
            {
                var category = categories.FindCategory(categoryId.Value);

                if(category != null)
                    GetAssets(new List<AssetCategory> { category }, categoryId, subCategories, assets);
            }
            else
                GetAssets(categories, categoryId, subCategories, assets);

            return assets;
        }

        public static AssetCategory FindCategory(this IEnumerable<AssetCategory> categories, Guid categoryId)
        {
            foreach(var category in categories)
            {
                if(category.Id == categoryId)
                    return category;

                if(category.Categories != null)
                { 
                    var result = FindCategory(category.Categories, categoryId);

                    if(result != null)
                        return result;
                }
            }

            return null;
        }

        private static void GetAssets(IEnumerable<AssetCategory> categories, Guid? categoryId, bool subCategories, List<Asset> assets)
        {
            foreach(var category in categories)
            {
                if(!categoryId.HasValue || category.Id == categoryId.Value)
                {
                    assets.AddRange(category.Assets);

                    if(subCategories && category.Categories != null)
                        GetAssets(category.Categories, null, true, assets);
                }
            }
        }
    }
}
