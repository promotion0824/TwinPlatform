using System.Threading.Tasks;
using System.Text;

using Xunit;
using Moq;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Willow.Common;
using Willow.Azure.Storage;
using Willow.Api.AzureStorage;
using Willow.Platform.Localization;

namespace Willow.Platform.Localization.FunctionalTests
{
    public class AssetLocalizerFactoryTests
    {
        [Fact(Skip = "Put in storage key to test")]
        public async Task AssetLocalizerFactory_GetLocalizer()
        {
            var storageKey = "";

            var logger  = new Mock<ILogger>();
            var cache   = new Mock<IMemoryCache>();
            var config  = new BlobStorageConfig { AccountName   = "wiluatpltaue1contentsto",
                                                  ContainerName = "realestate",
                                                  AccountKey    = storageKey};

            var store   = config.CreateBlobStore("assets", logger.Object, false);

            var factory = new AssetLocalizerFactory(cache.Object, store, logger.Object);

            var localizer = await factory.GetLocalizer("fr");

            Assert.Equal("fr", localizer.Locale);
            Assert.Equal("Production & Stockage d'Électricité", localizer.TranslateAssetName("dtmi:com:willowinc:ElectricalGenerationStorageEquipment;1", "not right value"));
            Assert.Equal("Équipements de Transport",            localizer.TranslateAssetName("dtmi:com:willowinc:ConveyanceEquipment;1", "not right value"));
            Assert.Equal("Extinction Incendie",                 localizer.TranslateAssetName("dtmi:com:willowinc:FireSuppressionEquipment;1", "not right value"));
            Assert.Equal("Menuiseries et Tourniquets",          localizer.TranslateAssetName("dtmi:com:willowinc:BarrierAsset;1", "not right value"));
            Assert.Equal("Éléments Architecturaux",             localizer.TranslateAssetName("dtmi:com:willowinc:ArchitecturalAsset;1", "not right value"));
        }
    }
}
