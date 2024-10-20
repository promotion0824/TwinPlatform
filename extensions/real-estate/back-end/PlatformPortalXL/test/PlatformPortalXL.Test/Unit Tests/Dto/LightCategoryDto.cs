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
    public class LightCategoryDtoTests
    {
        [Fact]
        public void LightCategoryDto_Localize()
        {
            var localizer = new AssetLocalizer("fr", "ModelId,EnglishValue,TranslatedValue\r\nair,Air Handling FrequencyUnit,Unité de traitement d’air\r\nfan,Exhaust Fan,Ventilateur d’extraction", "");
            var categories = new List<LightCategoryDto>
            {
                new LightCategoryDto
                {
                   ModelId = "air",
                    Name = "Air Handling FrequencyUnit",
                    ChildCategories = new List<LightCategoryDto>    
                    {
                        new LightCategoryDto
                        {
                            ModelId = "air",
                            Name = "Air Handling FrequencyUnit",
                        },
                        new LightCategoryDto
                        {
                            ModelId = "fan",
                            Name = "Exhaust Fan",
                        }
                    }
                }
            };

           localizer.Localize(categories);

            Assert.Equal("Unité de traitement d’air",   categories[0].Name);
            Assert.Equal("Unité de traitement d’air",   categories[0].ChildCategories.First().Name);
            Assert.Equal("Ventilateur d’extraction",    categories[0].ChildCategories.ToList()[1].Name);
        }
    }
}
