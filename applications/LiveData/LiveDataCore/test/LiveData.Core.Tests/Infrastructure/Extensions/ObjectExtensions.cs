namespace Willow.Tests.Infrastructure.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;

    public static class ObjectExtensions
    {
        public static IDictionary<string, string> ToDictionary(this object source, bool ignoreNulls = false)
        {
            if (source == null)
            {
                throw new NullReferenceException("Unable to convert anonymous object to a dictionary. The source anonymous object is null.");
            }

            var dictionary = new Dictionary<string, string>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                var value = property.GetValue(source);
                if (ignoreNulls && value == null)
                {
                    continue;
                }

                dictionary.Add(property.Name, value?.ToString());
            }

            return dictionary;
        }

        public static IEnumerable<KeyValuePair<string, string>> ToFormData(this object source, bool ignoreNulls = false)
        {
            if (source == null)
            {
                throw new NullReferenceException("Unable to convert anonymous object to a dictionary. The source anonymous object is null.");
            }

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
    }
}
