using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

using Xunit;
using Moq;

using Willow.Common;

namespace Willow.Platform.Localization.UnitTests
{
    public class AssetLocalizerFactoryTests
    {
        private const string _frenchProperties  = "ModelId,EnglishValue,TranslatedValue\r\nfoo,Name,Nom\r\nfoo,Air Handling Unit,Unité de traitement d’air\r\nfoo,Exhaust Fan,Ventilateur d’extraction";
        private const string _russianProperties = "ModelId,EnglishValue,TranslatedValue\r\nfoo,Name,Имя\r\nfoo,Air Handling Unit,\"Приточно-вытяжная установка\"\r\nfoo,Exhaust Fan,Вытяжной вентиляторn";
        private const string _frenchAssets      = "ModelId,EnglishValue,TranslatedValue\r\nair,Air Handling Unit,Unité de traitement d’air\r\nexhaust,Exhaust Fan,Ventilateur d’extraction";
        private const string _russianAssets     = "ModelId,EnglishValue,TranslatedValue\r\nair,Air Handling Unit,\"Приточно-вытяжная установка\"\r\nexhaust,Exhaust Fan,Вытяжной вентиляторn";

        private readonly Mock<IBlobStore> _blobStore = new Mock<IBlobStore>();
        private readonly IMemoryCache _cache;
        private readonly IAssetLocalizerFactory _factory;

        public AssetLocalizerFactoryTests()
        {
            _blobStore.Setup( b=> b.Get("fr/assets.csv", It.IsAny<Stream>()) ).Callback((string id, Stream content)=> content.Write(UTF8Encoding.Default.GetBytes(_frenchAssets), 0, UTF8Encoding.Default.GetByteCount(_frenchAssets)));
            _blobStore.Setup( b=> b.Get("ru/assets.csv", It.IsAny<Stream>()) ).Callback((string id, Stream content)=> content.Write(UTF8Encoding.Default.GetBytes(_russianAssets), 0, UTF8Encoding.Default.GetByteCount(_russianAssets)));
            _blobStore.Setup( b=> b.Get("fr/properties.csv", It.IsAny<Stream>()) ).Callback((string id, Stream content)=> content.Write(UTF8Encoding.Default.GetBytes(_frenchProperties), 0, UTF8Encoding.Default.GetByteCount(_frenchProperties)));
            _blobStore.Setup( b=> b.Get("ru/properties.csv", It.IsAny<Stream>()) ).Callback((string id, Stream content)=> content.Write(UTF8Encoding.Default.GetBytes(_russianProperties), 0, UTF8Encoding.Default.GetByteCount(_russianProperties)));
            _cache = new MemoryCache(new MemoryCacheOptions());

            _factory = new AssetLocalizerFactory(_cache, _blobStore.Object, null);
        }

        [Theory]
        [InlineData("fr")]
        [InlineData("fr-FR")]
        [InlineData("fr-CA")]
        public async Task AssetLocalizerFactory_TranslateProperty_french(string locale)
        {
            var localizer = await _factory.GetLocalizer(locale);

            Assert.Equal("Nom",                         localizer.TranslateProperty("foo", "Name",              "Name"));
            Assert.Equal("Unité de traitement d’air",   localizer.TranslateProperty("foo", "Air Handling Unit", "Air Handling Unit"));
            Assert.Equal("Ventilateur d’extraction",    localizer.TranslateProperty("foo", "Exhaust Fan",       "Exhaust Fan"));
        }

        [Theory]
        [InlineData("ru")]
        [InlineData("ru-RU")]
        [InlineData("ru-UA")]
        public async Task AssetLocalizerFactory_TranslateProperty_russian(string locale)
        {
            var localizer = await _factory.GetLocalizer(locale);

            Assert.Equal("Имя",                          localizer.TranslateProperty("foo", "Name",              "Name"));
            Assert.Equal("Приточно-вытяжная установка",  localizer.TranslateProperty("foo", "Air Handling Unit", "Air Handling Unit"));
            Assert.Equal("Вытяжной вентиляторn",         localizer.TranslateProperty("foo", "Exhaust Fan",       "Exhaust Fan"));
        }

        [Theory]
        [InlineData("joun")]
        [InlineData("frank")]
        [InlineData("es-Es")]
        public async Task AssetLocalizerFactory_TranslateProperty_fallback(string locale)
        {
            var localizer = await _factory.GetLocalizer(locale);

            Assert.Equal("Name",               localizer.TranslateProperty("foo", "frank",  "Name"));
            Assert.Equal("Air Handling Unit",  localizer.TranslateProperty("foo", "george", "Air Handling Unit"));
            Assert.Equal("Exhaust Fan",        localizer.TranslateProperty("foo", "mary",   "Exhaust Fan"));
        }

        [Theory]
        [InlineData("fr")]
        [InlineData("fr-FR")]
        [InlineData("fr-CA")]
        public async Task AssetLocalizerFactory_TranslateAssetName_french(string locale)
        {
            var localizer = await _factory.GetLocalizer(locale);

            Assert.Equal("Unité de traitement d’air",   localizer.TranslateAssetName("air",     "Air Handling Unit"));
            Assert.Equal("Ventilateur d’extraction",    localizer.TranslateAssetName("exhaust", "Exhaust Fan"));
        }

        [Theory]
        [InlineData("ru")]
        [InlineData("ru-RU")]
        [InlineData("ru-UA")]
        public async Task AssetLocalizerFactory_TranslateAssetName_russian(string locale)
        {
            var localizer = await _factory.GetLocalizer(locale);

            Assert.Equal("Приточно-вытяжная установка",  localizer.TranslateAssetName("air",     "Air Handling Unit"));
            Assert.Equal("Вытяжной вентиляторn",         localizer.TranslateAssetName("exhaust", "Exhaust Fan"));
        }

        [Theory]
        [InlineData("joun")]
        [InlineData("ja-JP")]
        [InlineData("es-Es")]
        public async Task AssetLocalizerFactory_TranslateAssetName_fallback(string locale)
        {
            var localizer = await _factory.GetLocalizer(locale);

            Assert.Equal("Air Handling Unit",  localizer.TranslateAssetName("air",     "Air Handling Unit"));
            Assert.Equal("Exhaust Fan",        localizer.TranslateAssetName("exhaust", "Exhaust Fan"));
        }
    }
}
