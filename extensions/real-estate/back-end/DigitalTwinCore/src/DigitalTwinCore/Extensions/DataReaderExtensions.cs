using System;
using System.Collections.Generic;
using System.Data;

namespace DigitalTwinCore.Extensions
{
    public static class DataReaderExtensions
    {
        public static IEnumerable<T> Parse<T>(this IDataReader reader) where T : new()
        {
            var properties = typeof(T).GetProperties();

            while (reader.Read())
            {
                var item = new T();

                foreach (var property in properties)
                {
                    var propertyOrdinal = reader.GetOrdinal(property.Name);

                    if (propertyOrdinal == -1 || reader.IsDBNull(propertyOrdinal))
                    {
                        continue;
                    }

                    if (property.PropertyType == typeof(Guid))
                    {
                        property.SetValue(item, reader.GetGuid(propertyOrdinal));
                    } else if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(item, reader.GetString(propertyOrdinal));
                    } else if (property.PropertyType == typeof(bool))
                    {
                        property.SetValue(item, reader.GetBoolean(propertyOrdinal));
                    } else if (property.PropertyType == typeof(DateTime))
                    {
                        property.SetValue(item, reader.GetDateTime(propertyOrdinal));
                    } else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(item, reader.GetInt32(propertyOrdinal));
                    } else if (property.PropertyType == typeof(long))
                    {
                        property.SetValue(item, reader.GetInt64(propertyOrdinal));
                    } else // This list is limited, feel free to add more types as required
                    {
                        property.SetValue(item, reader.GetValue(propertyOrdinal));
                    }
                }

                yield return item;
            }
        }
    }
}
