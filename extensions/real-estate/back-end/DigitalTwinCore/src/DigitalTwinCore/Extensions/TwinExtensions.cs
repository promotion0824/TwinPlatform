using System;
using System.Collections.Generic;
using System.Linq;
using DigitalTwinCore.Constants;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Linq;

namespace DigitalTwinCore.Models
{
    public static class TwinExtensions
    {
        public static string GetStringProperty(this Twin twin, string propertyName) =>
            twin.GetProperty<string>(propertyName);

        public static T GetProperty<T>(this Twin twin, string propertyName) where T : class =>
            GetPropertyValueAsObject(twin, propertyName) as T;

        public static T? GetPropertyValue<T>(this Twin twin, string propertyName) where T : struct
        {
            var value = GetPropertyValueAsObject(twin, propertyName);

            if (value == null)
            {
                return null;
            }

            return value switch
            {
                T typedValue => typedValue,

                string stringValue => (typeof(T) == typeof(Guid) && Guid.TryParse(stringValue, out var guidValue)) ? guidValue as T? : null,

                double doubleValue => typeof(T) == typeof(decimal)
                    ? Convert.ToDouble(doubleValue) as T?
                    : null,

                long longValue => typeof(T) == typeof(double)
                    ? Convert.ToDouble(longValue) as T?
                    : null,

                _ => (value.GetType().IsValueType == typeof(T).IsValueType) ? (T?)value : null,
            };
        }

		// TODO: delete when old AssetService is deleted
		public static IDictionary<string, object> GetObjectProperty(this Twin twin, string propertyName)
		{
			return GetPropertyValueAsObject(twin, propertyName) as IDictionary<string, object>;
		}

        public static JObject GetJObjectProperty(this Twin twin, string propertyName)
        {
            return GetPropertyValueAsObject(twin, propertyName) as JObject;
        }

        // This function and the following obviously have a similar shape that can be re-factored out -- do so if we add another lookup by rel
        public static Guid? FindSiteId(this Twin twin, string[] siteModelIds = null)
        {
            var siteId = twin.GetStringProperty(Properties.SiteId);
            if (siteId != null)
            {
                return Guid.Parse(siteId); // The Twin has a siteID property 
            }
            if (siteModelIds?.Contains(twin.Metadata.ModelId) == true)
            {
                return twin.UniqueId; // The Twin *is* a site
            }

            // Otherwise, keep following locatedIn or isPartOf links until we find a 
            //   twin that is the site or has a siteID property
            siteModelIds ??= new[] { WillowInc.SiteModelId };
            if (twin is TwinWithRelationships twinWithRelationships)
            {
                var locations = Enumerable.Repeat(twinWithRelationships, 1);
                do
                {
                    siteId = twin.GetStringProperty(Properties.SiteId);

                    if (siteId != null)
                    {
                        return Guid.Parse(siteId); // The Twin has a siteID property 
                    }
                    var siteTwin = locations.FirstOrDefault(l => siteModelIds.Contains(l.Metadata.ModelId));
                    if (siteTwin != null)
                    {
                        return siteTwin.UniqueId;
                    }

                    locations = locations.SelectMany(l => l.Relationships.Where(r => 
                            r.Name == Relationships.LocatedIn || r.Name == Relationships.IsPartOf
                         || r.Name == Relationships.IsCapabilityOf || r.Name == Relationships.HostedBy
                         || r.Name == Relationships.IsDocumentOf
                     ).Select(r => r.Target)).ToList();
                } while (locations.Any());
            }

            return null;
        }

        public static Guid? FindFloorId(this TwinWithRelationships twin, string[] levelModelIds = null)
        {
            levelModelIds ??= new[] { WillowInc.LevelModelId };
            var locations = Enumerable.Repeat(twin, 1);
            do
            {
                var floorTwin = locations.FirstOrDefault(l => levelModelIds.Contains(l.Metadata.ModelId));
                if (floorTwin != null)
                {
                    return floorTwin.UniqueId;
                }
                locations = locations.SelectMany(l => l.Relationships.Where(r =>
                            r.Name == Relationships.LocatedIn || r.Name == Relationships.IsPartOf
                         || r.Name == Relationships.IsCapabilityOf || r.Name == Relationships.HostedBy
                         || r.Name == Relationships.IsDocumentOf
                    ).Select(r => r.Target));
            } while (locations.Any());

            return null;
        }

        private static object GetPropertyValueAsObject(Twin twin, string propertyName)
        {
            if (twin.CustomProperties?.ContainsKey(propertyName) ?? false)
            {
                return twin.CustomProperties[propertyName];
            }
            return null;
        }

        public static T ApplyCachedTwinPatch<T>(this T twin, JsonPatchDocument patch) where  T : Twin
        {
            foreach (var op in patch.Operations)
            {
                var property = ensureAndGetSimplePropertyFromPath(op.path);
                switch (op.OperationType)
                {
                    case OperationType.Replace:
                    case OperationType.Add:
                        twin.CustomProperties[property] = op.value;
                        break;

                    case OperationType.Remove:
                        twin.CustomProperties.Remove(property);
                        break;

                    default:
                        throw new NotSupportedException($"Unsupported twin PATCH operation: {op.OperationType}");
                }
            }

            // We have modified the cached twin in-place
            return twin;
        }

        public static object ToCollection(object element)
        {
            if (element is JObject jo)
            {
                return jo.ToObject<IDictionary<string, object>>().ToDictionary(k => k.Key, v => ToCollection(v.Value));
            }
            if (element is JArray ja)
            {
                return ja.ToObject<List<object>>().Select(ToCollection).ToList();
            }
            return element;
        }

        // See JsonPatch spec here: // https://docs.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-5.0
        private static string ensureAndGetSimplePropertyFromPath(string path)
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1 || char.IsDigit( parts[0][0]) || parts[0][0] == '-')
            {
                throw new NotSupportedException($"Only simple root-level PATCH operations supported: {path} ");
            }
            return parts[0];
        }
    }
}
