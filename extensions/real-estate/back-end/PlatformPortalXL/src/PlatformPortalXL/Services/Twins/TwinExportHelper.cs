using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlatformPortalXL.Features.Twins;

namespace PlatformPortalXL.Services.Twins
{
    public static class TwinExportHelper
    {
        private static readonly string[] FixedHeaders = { "modelId", "id", "name" };
        private static readonly string[] JsonFields = { "Raw", "Contents", "Metadata" };
        private static readonly string[] IgnoredHeaders = { "displayName", "customProperties", "deleted", "uniqueIdFromProperties", "exportTime" };
        private const string MetaData = "$metadata";
        private const string Contents = "Contents";
        private const string CustomProperties = "customProperties";

        public static async Task ExportTwins(Stream streamToWrite, TwinSearchResponse.SearchTwin[] searchTwins)
        {
            var dynamicHeaders = new HashSet<string>();

            var twins = GetRecords(searchTwins, dynamicHeaders).ToArray();

            var headers = FixedHeaders.Concat(dynamicHeaders.OrderBy(x => x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

            await using var writer = new StreamWriter(streamToWrite)
            {
                AutoFlush = true
            };
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            foreach (var header in headers)
            {
                csv.WriteField(header);
            }
            await csv.NextRecordAsync();

            foreach (var twin in twins)
            {
                foreach (var header in headers)
                {
                    csv.WriteField(twin.TryGetValue(header.ToUpperInvariant(), out var value) ? value : string.Empty);
                }

                await csv.NextRecordAsync();
            }

            streamToWrite.Position = 0;
        }

        private static IEnumerable<IDictionary<string, string>> GetRecords(IEnumerable<TwinSearchResponse.SearchTwin> twins, ISet<string> headers)
        {
            return twins.Select(twin => GetRecord(twin, headers)).Where(x => x != null);
        }

        private static IDictionary<string, string> GetRecord(TwinSearchResponse.SearchTwin searchTwin, ISet<string> headers)
        {
            var twin = JsonConvert.DeserializeObject<JObject>(searchTwin.RawTwin);
            if (twin == null)
            {
                return new Dictionary<string, string>();
            }

            var record = new Dictionary<string, string>();

            var properties = new Dictionary<string, string>();

            foreach (var propertyName in twin.Properties().Select(p => p.Name))
            {
                if (JsonFields.Contains(propertyName, StringComparer.OrdinalIgnoreCase))
                {
                    var content = JObject.Parse(twin.SelectToken(propertyName)?.ToString());
                    foreach (var property in content.Properties().Where(p => !ShouldSkipProperty(propertyName, p.Name)))
                    {
                        content.SelectToken($"{property.Name}.{MetaData}")?.Parent.Remove();
                        var value = property.Value.ToString();
                        if (property.Value.ToString().Equals("{}"))
                        {
                            value = string.Empty;
                        }
                        properties.TryAdd(property.Name, value);
                    }
                    continue;
                }
            }

            foreach (var (key, value) in properties)
            {
                if (!FixedHeaders.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    headers.Add(key);
                }

                record.TryAdd(key.ToUpperInvariant(), value);
            }

            return record;
        }

        private static bool ShouldSkipProperty(string propertyName, string customPropertyName)
        {
            if (propertyName.Equals(Contents, StringComparison.OrdinalIgnoreCase) &&
                                      customPropertyName.Equals(CustomProperties, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (IgnoredHeaders.Contains(customPropertyName, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}
