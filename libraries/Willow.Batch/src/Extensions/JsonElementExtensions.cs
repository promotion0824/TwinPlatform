namespace Willow.Batch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;

    /// <summary>
    /// Extension methods for <see cref="JsonElement"/>.
    /// </summary>
    public static class JsonElementExtensions
    {
        /// <summary>
        /// A delegate for parsing an object of one type to another.
        /// </summary>
        /// <typeparam name="T1">The type of the original object.</typeparam>
        /// <typeparam name="T2">The type of the target object.</typeparam>
        /// <param name="a">The original object.</param>
        /// <param name="b">The output object.</param>
        /// <returns>True if the parse was successful. False otherwise.</returns>
        public delegate bool TryParser<T1, T2>(T1 a, out T2 b);

        /// <summary>
        /// Checks if a JsonElement has any elements.
        /// </summary>
        /// <typeparam name="T">The type of the input object.</typeparam>
        /// <param name="value">The value of the input object.</param>
        /// <returns>True if the value is an enumerable JSON element and it has a value, or is an array with any elements.</returns>
        public static bool HasAnyJsonElement<T>(this T value)
        {
            if (value.IsEnumerableJsonElement())
            {
                var el = value as JsonElement?;
                return el.HasValue && el.Value.EnumerateArray().Any();
            }

            return false;
        }

        /// <summary>
        /// Checks if a value is an enumerable JSON element.
        /// </summary>
        /// <typeparam name="T">The type of the input object.</typeparam>
        /// <param name="value">The value of the input object.</param>
        /// <returns>True if the object is a JsonElement and it is Json Array.</returns>
        public static bool IsEnumerableJsonElement<T>(this T value)
        {
            return value?.GetType() == typeof(JsonElement) && (value as JsonElement?).Value.ValueKind == JsonValueKind.Array;
        }

        /// <summary>
        /// Parses a JsonElement to an object of a given type.
        /// </summary>
        /// <param name="el">The Json Element.</param>
        /// <param name="type">The desired type of the object.</param>
        /// <returns>An array of the type of objects, or an instance of the object if the element is not an array.</returns>
        public static object ParseOrDefaultEnum(this JsonElement el, Type type)
        {
            var method = (el.ValueKind == JsonValueKind.Array)
                ? typeof(JsonElementExtensions).GetMethod(nameof(ParseAllOrDefaultEnum))
                : typeof(JsonElementExtensions).GetMethod(nameof(ParseOneOrDefaultEnum));
            var genericMethod = method.MakeGenericMethod(type);

            return (el.ValueKind == JsonValueKind.Array)
                ? genericMethod.Invoke(null, new object[] { el.EnumerateArray().ToList() })
                : genericMethod.Invoke(null, new object[] { el });
        }

        /// <summary>
        /// Parses an Enumerable of JsonElements to a list of objects of a given type.
        /// </summary>
        /// <typeparam name="T">The type of the objects to return.</typeparam>
        /// <param name="values">The enumerable of JSON elements to parse.</param>
        /// <returns>A list of objects of type T.</returns>
        public static List<T> ParseAllOrDefaultEnum<T>(this IEnumerable<JsonElement> values)
            where T : struct
        {
            return values.Select(x => x.ParseOneOrDefaultEnum<T>()).ToList();
        }

        /// <summary>
        /// Parses an object to an object of a given type.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="val">The object to parse.</param>
        /// <returns>The parsed object or the default value for an object of Type T.</returns>
        public static T ParseOneOrDefaultEnum<T>(this object val)
            where T : struct
        {
            val = int.TryParse(val.ToString(), out int intval) ? Enum.ToObject(typeof(T), intval) : val;

            if (Enum.TryParse<T>(val.ToString(), true, out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }

        /// <summary>
        /// Parses a JsonElement to an object of a given type.
        /// </summary>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <param name="el">The input JsonElement.</param>
        /// <param name="parser">The parser method to use for the conversion.</param>
        /// <returns>A parsed object or the default value for an object of type T.</returns>
        public static object ParseOrDefault<T>(this JsonElement el, TryParser<string, T> parser)
        {
            return (el.ValueKind == JsonValueKind.Array)
                ? el.EnumerateArray().ParseAllOrDefault<T>(parser)
                : el.ParseOneOrDefault<T>(parser);
        }

        /// <summary>
        /// Parses an enumerable of JSON elements to a list of objects of a given type.
        /// </summary>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <param name="values">The list of values to convert.</param>
        /// <param name="parser">The parser method to use for the conversion.</param>
        /// <returns>A list of parsed objects or the default value for an object of type T.</returns>
        public static List<T> ParseAllOrDefault<T>(this IEnumerable<JsonElement> values, TryParser<string, T> parser)
        {
            return values.Select(x => x.ParseOneOrDefault<T>(parser)).ToList();
        }

        /// <summary>
        /// Parses an object to an object of a given type.
        /// </summary>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <param name="val">The value to convert.</param>
        /// <param name="parser">The parser method to use for the conversion.</param>
        /// <returns>A parsed object or the default value for an object of type T.</returns>
        public static T ParseOneOrDefault<T>(this object val, TryParser<string, T> parser)
        {
            if (parser != null && parser(val.ToString(), out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }
    }
}
