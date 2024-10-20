using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SiteCore.Infrastructure
{
    /// <summary>
    /// This class is shared from the PortalXL to convert parameters to query string
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// Convert query parameter objects to query string
        /// e.g. List of uniqueIds to <![CDATA[ uniqueIds=Id1&uniqueIds=Id2 ]]>
        /// </summary>
        /// <param name="parameters">query parameter objects</param>
        /// <returns>Query string</returns>
        public static string ToQueryString(object parameters)
        {
            var fields = new List<(string key, object value, bool isEnumerable)>();

            // Turn parameters objects to list of key value pair
            parameters.GetType().GetProperties()
                .ToList()
                .ForEach(pi =>
                {
                    var key = pi.Name;
                    var value = pi.GetValue(parameters, null);
                    var isEnumerable = pi.PropertyType.IsArray ||
                                       pi.PropertyType.IsAssignableTo(typeof(Enumerable)) ||
                                       pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>);

                    if (value != null)
                    {
                        fields.Add((key, value, isEnumerable));
                    }
                });
            // Convert the list of key value pair to query string with http encoding
            var array = fields.SelectMany(field =>
            {
                var (key, value, isEnumerable) = field;
                if (isEnumerable)
                {
                    return ((IEnumerable)value).Cast<object>()
                        .Where(x => !string.IsNullOrWhiteSpace(x.ToString()))
                        .Select(x => ToEncodedKeyValue(key, x));
                }
                return !string.IsNullOrWhiteSpace(value.ToString()) ? new[] { ToEncodedKeyValue(key, value) } : System.Array.Empty<string>();
            }).ToArray();
            return string.Join("&", array);
        }

        private static string ToEncodedKeyValue(object key, object value)
        {
            return $"{HttpUtility.UrlEncode(key.ToString())}={HttpUtility.UrlEncode(value.ToString())}";
        }
    }
}
