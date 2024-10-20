using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Willow.Common;

namespace Willow.Platform.Localization
{
    /// <summary>
    /// Interface to translate a single twin property label
    /// </summary>
    public interface IAssetLocalizer
    {
        string TranslateProperty(string assetModelId, string propertyName, string defaultValue);
        string TranslateAssetName(string assetModelId, string defaultValue);

        string Locale { get; }
    }

    /// <summary>
    /// Interface to return an IAssetLocalizer for a specific locale or language
    /// </summary>
    public interface IAssetLocalizerFactory
    {
        Task<IAssetLocalizer> GetLocalizer(string locale);
    }

    public class AssetLocalizer : IAssetLocalizer
    {
        private readonly IDictionary<string, TranslationValue> _translationMap;

        public AssetLocalizer(string locale, string assetNamesFile, string propertyNamesFile)
        {
            _translationMap = new Dictionary<string, TranslationValue>();
            this.Locale = locale;

            AddToMap(assetNamesFile, (fields)=> fields[0]);
            AddToMap(propertyNamesFile, (fields)=> $"{fields[0].Trim()}_{fields[1].Trim()}");
        }

        public string Locale { get; }

        public string TranslateAssetName(string assetModelId, string defaultValue)
        {
            var key = assetModelId.ToKey();

            if (_translationMap.ContainsKey(key))
            { 
                return _translationMap[key].Value;
            }

           return defaultValue;
        }

        public string TranslateProperty(string assetModelId, string propertyName, string defaultValue)
        {
            var key = $"{assetModelId}_{propertyName}".ToKey();

            if (_translationMap.ContainsKey(key))
            { 
                return _translationMap[key].Value;
            }

            // Try without model id
            key = $"_{propertyName}".ToKey();

            if (_translationMap.ContainsKey(key))
            { 
                return _translationMap[key].Value;
            }

            return defaultValue;
        }

        #region Private

        private void AddToMap(string csvFile, Func<IList<string>, string> createKey)
        {
            var lines = csvFile.ParseCSV().Where( fields=> fields.Count >= 3 && fields[0] != "ModelId").ToList();

            foreach(var fields in lines)
            { 
                var key = createKey(fields).ToKey();

                if(!_translationMap.ContainsKey(key))
                {                             
                    _translationMap.Add(key, new TranslationValue { Value = fields[2].Trim() });
                }
            }
        }

        #endregion
    }
    
    internal class TranslationValue
    {
        public string Value { get; init; }
    }

    public class PassThruAssetLocalizer : IAssetLocalizer
    {
        public PassThruAssetLocalizer()
        {
        }

        public string TranslateAssetName(string assetModelId, string defaultValue)
        {
            return defaultValue;
        }

        public string TranslateProperty(string assetModelId, string propertyName, string defaultValue)
        {
            return defaultValue;
        }

        public string Locale => "en";
    }

    public class AssetLocalizerFactory : IAssetLocalizerFactory
    {
        private readonly IMemoryCache _cache;
        private readonly IBlobStore   _blobStore;
        private readonly ILogger      _logger;

        public AssetLocalizerFactory(IMemoryCache cache, IBlobStore blobStore, ILogger logger)
        {
            _cache = cache;
            _blobStore = blobStore;
            _logger = logger;
        }

        public async Task<IAssetLocalizer> GetLocalizer(string locale)
        {
            locale ??= "en";

            var key = "AssetLocalizer." + locale;

            if(_cache.TryGetValue(key, out IAssetLocalizer localizer))
                return localizer;

            IAssetLocalizer newLocalizer;

            try
            { 
                var assetNamesFile    = await _blobStore.Get($"{locale}/assets.csv", Encoding.UTF8);
                var propertyNamesFile = await _blobStore.Get($"{locale}/properties.csv", Encoding.UTF8);
                
                if(string.IsNullOrWhiteSpace(assetNamesFile))
                    throw new FileNotFoundException("Asset translation file not found", assetNamesFile).WithData("Locale", locale);

                if(string.IsNullOrWhiteSpace(propertyNamesFile))
                    throw new FileNotFoundException("Asset property translation file not found", propertyNamesFile).WithData("Locale", locale);

                newLocalizer = new AssetLocalizer(locale, assetNamesFile, propertyNamesFile);
            }
            catch(FileNotFoundException)
            {
                // If current locale is English then just return a pass thru localizer
                if(locale == "en")
                    newLocalizer = new PassThruAssetLocalizer();
                else
                { 
                    // If locale is actually an ISO locale, eg. en-US then try just the language part
                    if(locale.Length > 2)
                       return await GetLocalizer(locale.Substring(0, 2));

                    // Just use English
                    return await GetLocalizer("en");
                }
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "Unable to load translation files");
                newLocalizer = new PassThruAssetLocalizer();
            }

            try
            { 
                _cache.Set(key, newLocalizer, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromHours(2) });
            }
            catch
            {
                // Don't fail if fail to set cache
            }

            return newLocalizer;
        }
    }
    
    public class PassThruAssetLocalizerFactory : IAssetLocalizerFactory
    {
        private readonly IAssetLocalizer _localizer = new PassThruAssetLocalizer();

        public PassThruAssetLocalizerFactory()
        {
        }

        public Task<IAssetLocalizer> GetLocalizer(string locale)
        {
            return Task.FromResult(_localizer);
        }
    }

    public static class Extensions
    {
        public static string ToKey(this string s)
        {
            var key = s.Trim().ToLowerInvariant().Replace("dtmi:com:willowinc:", "").Replace(";", "");
            var sb = new StringBuilder();

            foreach(var ch in key)
            {
                // Translation file has non-visible characters, so let's strip them
                if(char.IsAscii(ch))
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }
    }
}
