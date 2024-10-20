namespace Willow.Batch
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;

    /// <summary>
    /// Extensions for <see cref="object"/>.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Does the collection have any elements.
        /// </summary>
        /// <typeparam name="T">The type of object to check for.</typeparam>
        /// <param name="value">The collection of objects.</param>
        /// <returns>True if there are elements. False otherwise.</returns>
        public static bool HasAny<T>(this T value)
        {
            return (value as IEnumerable)?.GetEnumerator().MoveNext() ?? false || value.HasAnyJsonElement();
        }

        /// <summary>
        /// Is the object an enumerable of Type T.
        /// </summary>
        /// <typeparam name="T">The type of the object in the enumerable.</typeparam>
        /// <param name="value">The Enumerable object.</param>
        /// <returns>True if the object is enumerable or is an Enumerable Json Element. False otherwise.</returns>
        public static bool IsEnumerable<T>(this T value)
        {
            return (value as IEnumerable) != null || value.IsEnumerableJsonElement();
        }

        /// <summary>
        /// Is this type an enumerable and not a string.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <returns>True if the object inherits from IEnumerable and is not a string.</returns>
        public static bool IsEnumerable(this Type type) => type.Name != nameof(String) && type.GetInterface(nameof(IEnumerable)) != null;

        /// <summary>
        /// Is this type nullable.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <returns>True if the underlying type is nullable.</returns>
        public static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) != null;

        /// <summary>
        /// Get the underlying type of a property.
        /// </summary>
        /// <param name="propertyType">The type of the property.</param>
        /// <param name="value">The value of the object.</param>
        /// <returns>The parsed object.</returns>
        public static object GetPropertyValue(this Type propertyType, object value)
        {
            var propertyValue = value;

            if (value != null && value.GetType() == typeof(JsonElement))
            {
                var jsonEl = (JsonElement)value;

                if (propertyType.IsEnum)
                {
                    propertyValue = jsonEl.ParseOrDefaultEnum(propertyType);
                }
                else if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                {
                    propertyValue = jsonEl.ParseOrDefault<Guid>(Guid.TryParse);

                    if (jsonEl.ValueKind == JsonValueKind.Array && propertyType == typeof(Guid?))
                    {
                        propertyValue = (propertyValue as IEnumerable<Guid>).Select(x => x as Guid?).ToList();
                    }
                }
                else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                {
                    propertyValue = jsonEl.ParseOrDefault<DateTime>(DateTime.TryParse);
                }
                else if (propertyType == typeof(DateTimeOffset) || propertyType == typeof(DateTimeOffset?))
                {
                    propertyValue = jsonEl.ParseOrDefault<DateTimeOffset>(DateTimeOffset.TryParse);
                }
                else if (propertyType == typeof(int) || propertyType == typeof(int?))
                {
                    propertyValue = jsonEl.ParseOrDefault<int>(int.TryParse);
                }
                else if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                {
                    propertyValue = jsonEl.ParseOrDefault<bool>(bool.TryParse);
                }
                else if (propertyType == typeof(string))
                {
                    propertyValue = jsonEl.ParseOrDefault<string>(ParseOneOrDefaultString);
                }
                else
                {
                    propertyValue = jsonEl.ToString();
                }
            }

            return propertyValue;
        }

        /// <summary>
        /// What does this do? Seems meaningless.
        /// </summary>
        /// <param name="a">Argument A.</param>
        /// <param name="b">Argument B.</param>
        /// <returns>Returns true. Always.</returns>
        public static bool ParseOneOrDefaultString(string a, out string b)
        {
            b = a;

            return true;
        }
    }
}
