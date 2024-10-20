using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Extensions
{
    public static class ObjectExtensions
    {
        public static IEnumerable<KeyValuePair<string, string>> ToFormData(this Connector source, bool ignoreNulls)
        {
            var collection = new List<KeyValuePair<string, string>>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                var value = property.GetValue(source);

                if (ignoreNulls && value == null)
                {
                    continue;
                }

                if (property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)) && property.PropertyType != typeof(string))
                {
                    foreach (var listItem in value as IEnumerable)
                    {
                        collection.Add(new KeyValuePair<string, string>(property.Name, listItem?.ToString()));
                    }
                }
                else
                {
                    collection.Add(new KeyValuePair<string, string>(property.Name, value?.ToString()));
                }
            }
            return collection;
        }

        public static object FromNewtonsoftJsonObject(this object value)
        {
            if (value is JObject)
                return JsonSerializerHelper.Deserialize<object>(JsonConvert.SerializeObject(value));
            return value;
        }

        public static T FirstOrDefault<T>(this ExpandoObject obj, string key)
        {
            return obj.FirstOrDefault<T>(key, default);
        }

        public static T FirstOrDefault<T>(this ExpandoObject obj, string key, T defaultValue)
        {
            var value = obj?.FirstOrDefault(x => x.Key == key).Value;

            if (value is T typedValue)
            {
                return typedValue;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T)) ?? defaultValue;
            }
            catch 
            { 
                return defaultValue; 
            }
        }

        public static IDictionary<string, string> AllOrDefault(this ExpandoObject obj, string prefix)
        {
            // perform a double lookup: identify key value pairs whose key belongs to the set of values keyed with prefix 
            // e.g. given { controls1: test1, test1: value1 }, controls -> return ( test1 : value1 )  
            // i.e. we can identify all control properties without explicitly knowing their names
            return obj?
                .Where(x => x.Key.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrWhiteSpace(x.Value.ToString()))
                .Select(x => x.Value.ToString())
                .Distinct()
                .ToDictionary(x => x, x => obj.FirstOrDefault<string>(x))
                ?? new Dictionary<string, string>();
        }
    }
}
