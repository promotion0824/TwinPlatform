using System;
using Xunit;


namespace Willow.Platform.Localization.UnitTests
{
    public class AssetLocalizerTests
    {
        [Fact]
        public void AssetLocalizer_TranslateProperty()
        {
            var localizer = new AssetLocalizer("fr", "", "ModelId,EnglishValue,TranslatedValue\r\nfoo,Name,Nom\r\nfoo,Air Handling Unit,Unit� de traitement d�air\r\nfoo,Exhaust Fan,Ventilateur d�extraction");

            Assert.Equal("Nom",                         localizer.TranslateProperty("foo", "Name",              "Name"));
            Assert.Equal("Unit� de traitement d�air",   localizer.TranslateProperty("foo", "Air Handling Unit", "Air Handling Unit"));
            Assert.Equal("Ventilateur d�extraction",    localizer.TranslateProperty("foo", "Exhaust Fan",       "Exhaust Fan"));
        }

        [Fact]
        public void AssetLocalizer_TranslateProperty_no_model_id()
        {
            var localizer = new AssetLocalizer("fr", "", "ModelId,EnglishValue,TranslatedValue\r\n,Name,Nom\r\n,Air Handling Unit,Unit� de traitement d�air\r\n,Exhaust Fan,Ventilateur d�extraction");

            Assert.Equal("Nom",                         localizer.TranslateProperty("foo", "Name",              "Name"));
            Assert.Equal("Unit� de traitement d�air",   localizer.TranslateProperty("foo", "Air Handling Unit", "Air Handling Unit"));
            Assert.Equal("Ventilateur d�extraction",    localizer.TranslateProperty("foo", "Exhaust Fan",       "Exhaust Fan"));
        }

        [Fact]
        public void AssetLocalizer_TranslateProperty_wQuotes()
        {
            var localizer = new AssetLocalizer("fr", "", "ModelId,EnglishValue,TranslatedValue\r\nfoo,Name,Nom\r\nfoo,Air Handling Unit,\"Unit� de traitement, d�air\"\r\nfoo,Exhaust Fan,Ventilateur d�extraction");

            Assert.Equal("Nom",                         localizer.TranslateProperty("foo",  "Name",              "Name"));
            Assert.Equal("Unit� de traitement, d�air",   localizer.TranslateProperty("foo", "Air Handling Unit", "Air Handling Unit"));
            Assert.Equal("Ventilateur d�extraction",    localizer.TranslateProperty("foo",  "Exhaust Fan",       "Exhaust Fan"));
        }

        [Fact]
        public void AssetLocalizer_TranslateProperty_wrong_model_id()
        {
            var localizer = new AssetLocalizer("fr", "", "ModelId,EnglishValue,TranslatedValue\r\nfoo,Name,Nom\r\nfoo,Air Handling Unit,Unit� de traitement d�air\r\nfoo,Exhaust Fan,Ventilateur d�extraction");

            Assert.Equal("Name",               localizer.TranslateProperty("bob", "Name",              "Name"));
            Assert.Equal("Air Handling Unit",  localizer.TranslateProperty("bob", "Air Handling Unit", "Air Handling Unit"));
            Assert.Equal("Exhaust Fan",        localizer.TranslateProperty("bob", "Exhaust Fan",       "Exhaust Fan"));
        }

        [Fact]
        public void AssetLocalizer_TranslateProperty_missing_id()
        {
            var localizer = new AssetLocalizer("fr", "", "ModelId,EnglishValue,TranslatedValue\r\nfoo,Name,Nom\r\nfoo,Air Handling Unit,Unit� de traitement d�air\r\nfoo,Exhaust Fan,Ventilateur d�extraction");

            Assert.Equal("Name",               localizer.TranslateProperty("foo", "frank",  "Name"));
            Assert.Equal("Air Handling Unit",  localizer.TranslateProperty("foo", "george", "Air Handling Unit"));
            Assert.Equal("Exhaust Fan",        localizer.TranslateProperty("foo", "mary",   "Exhaust Fan"));
        }

        [Fact]
        public void AssetLocalizer_TranslateProperty_no_header()
        {
            var localizer = new AssetLocalizer("fr", "", "foo,Name,Nom\r\nfoo,Air Handling Unit,Unit� de traitement d�air\r\nfoo,Exhaust Fan,Ventilateur d�extraction");

            Assert.Equal("Name",               localizer.TranslateProperty("foo", "frank",  "Name"));
            Assert.Equal("Air Handling Unit",  localizer.TranslateProperty("foo", "george", "Air Handling Unit"));
            Assert.Equal("Exhaust Fan",        localizer.TranslateProperty("foo", "mary",   "Exhaust Fan"));
        }

        [Fact]
        public void AssetLocalizer_TranslateAssetName_no_header()
        {
            var localizer = new AssetLocalizer("fr", "k1,Name,Nom\r\nk2,Air Handling Unit,Unit� de traitement d�air\r\nk3,Exhaust Fan,Ventilateur d�extraction\r\nk6,Bob,�l�ments Architecturaux", "");

            Assert.Equal("Nom",                        localizer.TranslateAssetName("k1", "Name"));
            Assert.Equal("Unit� de traitement d�air",  localizer.TranslateAssetName("k2", "Air Handling Unit"));
            Assert.Equal("Ventilateur d�extraction",   localizer.TranslateAssetName("k3", "Exhaust Fan"));
            Assert.Equal("�l�ments Architecturaux",    localizer.TranslateAssetName("k6", "Frank"));
        }
    }
}
