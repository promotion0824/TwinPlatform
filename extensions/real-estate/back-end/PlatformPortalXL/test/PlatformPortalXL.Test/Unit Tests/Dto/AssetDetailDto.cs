using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;

using Willow.Platform.Localization;

namespace Willow.PlatformPortal.XL.UnitTests
{
    public class AssetDetailDtoTests
    {
        [Fact]
        public void AssetDetailDto_MapFromModel()
        {
            var localizer = new AssetLocalizer("fr", "", "ModelId,EnglishValue,TranslatedValue\r\n,Name,Nom\r\n,Air Handling FrequencyUnit,Unité de traitement d’air\r\n,Exhaust Fan,Ventilateur d’extraction");
            var asset = new Asset
            {
                Id                 = Guid.NewGuid(),
                Name               = "test1",
                HasLiveData        = false,
                EquipmentId        = Guid.NewGuid(),
                CategoryId         = Guid.NewGuid(),
                FloorId            = Guid.NewGuid(),
                Properties         = new List<AssetProperty>
                {
                    new AssetProperty { DisplayName = "Air Handling FrequencyUnit", Value = "bob" },
                    new AssetProperty { DisplayName = "Exhaust Fan", Value = "frank" }
                }
            };

           var dto = AssetDetailDto.MapFromModel(asset, localizer);
           var props = dto.Properties.ToList();

            Assert.Equal("Unité de traitement d’air",   props[0].DisplayName);
            Assert.Equal("Ventilateur d’extraction",    props[1].DisplayName);
        }
    }
}
