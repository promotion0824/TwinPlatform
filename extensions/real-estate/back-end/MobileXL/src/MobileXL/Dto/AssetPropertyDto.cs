using System.Collections.Generic;
using System.Linq;
using MobileXL.Models;

namespace MobileXL.Dto
{
    public class AssetPropertyDto
    {
        public string DisplayName { get; set; }
        public object Value { get; set; }

        public static AssetPropertyDto MapFromModel(AssetProperty assetParameter)
        {
            if (assetParameter == null)
            {
                return null;
            }

            return new AssetPropertyDto
            {
                DisplayName = assetParameter.DisplayName,
                Value = assetParameter.Value
            };
        }

        public static List<AssetPropertyDto> MapFromModels(IEnumerable<AssetProperty> assetParameters)
        {
            return assetParameters?.Select(MapFromModel).ToList();
        }
    }
}
