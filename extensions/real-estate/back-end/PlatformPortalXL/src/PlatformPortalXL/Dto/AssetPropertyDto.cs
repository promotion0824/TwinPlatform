using PlatformPortalXL.Models;
using System.Collections.Generic;
using System.Linq;
using Willow.Platform.Localization;

namespace PlatformPortalXL.Dto
{
    public class AssetPropertyDto
    {
        public string DisplayName { get; set; }
        public object Value { get; set; }

        public static AssetPropertyDto MapFromModel(AssetProperty assetParameter, IAssetLocalizer assetLocalizer)
        {
            if (assetParameter == null)
            {
                return null;
            }

            return new AssetPropertyDto
            {
                DisplayName = assetLocalizer.TranslateProperty("", assetParameter.DisplayName, assetParameter.DisplayName),
                Value = assetParameter.Value
            };
        }

        public static List<AssetPropertyDto> MapFromModels(IEnumerable<AssetProperty> assetParameters, IAssetLocalizer assetLocalizer)
        {
            return assetParameters?.Select( a=> MapFromModel(a, assetLocalizer)).ToList();
        }
    }
}
